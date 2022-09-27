using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using SkiaSharp;

namespace RayTracingInOneWeekend.Scenes;

public class SceneDrawer
{
    private const int DefaultImageWidth = 1280;
    private const int DefaultImageHeight = 768;
    private const int DefaultMaxDepth = 50;
    
    private readonly Scene _scene;
    private readonly int _samplesPerPixel;
    private readonly Action<int, int, IDictionary<SKPoint, SKColor>> _invalidateCanvasCallback;
    private readonly int _imageWidth;
    private readonly int _imageHeight;
    private readonly int _maxDepth;
    
    private readonly ConcurrentDictionary<SKPoint, SKColor> _frameBuffer = new();

    public SceneDrawer(Scene scene,
        int samplesPerPixel,
        Action<int, int, IDictionary<SKPoint, SKColor>> invalidateCanvasCallback,
        int imageWidth = DefaultImageWidth, 
        int imageHeight = DefaultImageHeight, 
        int maxDepth = DefaultMaxDepth)
    {
        _scene = scene;
        _samplesPerPixel = samplesPerPixel;
        _invalidateCanvasCallback = invalidateCanvasCallback;
        _imageWidth = imageWidth;
        _imageHeight = imageHeight;
        _maxDepth = maxDepth;
    }
    
    public async Task Run(CancellationToken cancellationToken = default)
    {
        _frameBuffer.Clear();

        try
        {
            var tasks = Enumerable.Range(0, _imageHeight).Select(y => Task.Run(async () =>
            {
                for (var x = 0; x < _imageWidth; ++x)
                {
                    var pixelColor = Vector3.Zero;
                    for (var s = 0; s < _samplesPerPixel; ++s)
                    {
                        var u = (float)x / (_imageWidth - 1);
                        var v = (float)y / (_imageHeight - 1);
                        var ray = _scene.Camera.GetRay(u, v);
                        pixelColor += RayColor(ray, _scene, _maxDepth);
                    }

                    _frameBuffer.TryAdd(new SKPoint(x, y), pixelColor.GetColor(_samplesPerPixel));
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                    _invalidateCanvasCallback(_imageWidth, _imageHeight, _frameBuffer))
                    .ConfigureAwait(false);
            }, cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
    }
    
    private static Vector3 RayColor(Ray ray, Scene scene, int depth)
    {
        if (depth <= 0)
            return Vector3.Zero;

        if (scene.Hit(ray, 0.001f, float.PositiveInfinity, out RayHit hit, out Material? material))
        {
            if (material!.Scatter(ray, hit, out Vector3 attenuation, out Ray scattered))
                return attenuation * RayColor(scattered, scene, depth - 1);

            return Vector3.Zero;
        }

        var unitDirection = ray.Direction.UnitVector();
        var t = 0.5f * (unitDirection.Y + 1.0f);
        var color = (1.0f - t) * Vector3.One + t * new Vector3(0.5f, 0.7f, 1.0f);

        return color;
    }
}