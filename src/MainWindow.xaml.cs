using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RayTracingInOneWeekend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // image
        private const float _aspectRatio = 16.0f / 9.0f;
        private const int _imageWidth = 1280;
        private const int _imageHeight = (int)(_imageWidth / _aspectRatio);
        private const int _maxDepth = 50;
        private int _samplesPerPixel = 500;

        private readonly DispatcherTimer _timer;
        private readonly ConcurrentDictionary<(int x, int y), SKColor> _frameBuffer = new();
        private CancellationTokenSource? _cancellationSource;
        private Stopwatch? _stopwatch;

        public MainWindow()
        {
            InitializeComponent();

            _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, OnTimerElapsed, Dispatcher);
            _timer.Stop();
            TimerLabel.Content = TimeSpan.Zero;
        }

        private void OnTimerElapsed(object? sender, EventArgs e)
        {
            if (_stopwatch == null)
            {
                TimerLabel.Content = TimeSpan.Zero;
                return;
            }

            TimerLabel.Content = _stopwatch.Elapsed;
        }

        private static Scene CreateBookScene()
        {
            var lookFrom = new Vector3(13, 2, 3);
            var lookAt = new Vector3(0, 0, 0);
            var vUp = Vector3.UnitY;
            var distanceToFocus = 10f;
            var aperture = 0.1f;
            var camera = new Camera(lookFrom, lookAt, vUp, 20, _aspectRatio, aperture, distanceToFocus);

            var scene = new Scene(camera);

            // ground
            scene.Add(new Sphere(new Vector3(0, -1000, 0), 1000), new Lambertian(new Vector3(0.5f, 0.5f, 0.5f)));

            for(var a = -11; a < 11; a++)
            {
                for(var b = -11; b < 11; b++)
                {
                    var chooseMat = RandomUtil.NextFloat();
                    var center = new Vector3(a + 0.9f * RandomUtil.NextFloat(), 0.15f, b + 0.9f * RandomUtil.NextFloat());

                    if ((center - new Vector3(4f, 0.2f, 0f)).Length() > 0.9f)
                    {
                        float randOffset = RandomUtil.NextFloat() * 0.15f;
                        var sphere = new Sphere(center + Vector3.UnitY * randOffset, 0.15f + randOffset);

                        if (chooseMat < 0.8f)
                        {
                            // diffuse
                            var albedo = new Vector3(
                                RandomUtil.NextFloat() * RandomUtil.NextFloat(),
                                RandomUtil.NextFloat() * RandomUtil.NextFloat(),
                                RandomUtil.NextFloat() * RandomUtil.NextFloat());

                            scene.Add(
                                sphere,
                                new Lambertian(albedo));
                        }
                        else if (chooseMat < 0.95f)
                        {
                            var albedo = new Vector3(
                                0.5f * (1 + RandomUtil.NextFloat()),
                                0.5f * (1 + RandomUtil.NextFloat()),
                                0.5f * (1 + RandomUtil.NextFloat()));

                            var fuzz = 0.5f * (1 + RandomUtil.NextFloat());

                            scene.Add(
                                sphere,
                                new Metal(albedo, fuzz));
                        }
                        else
                        {
                            scene.Add(sphere, new Dielectric(1.5f));
                        }
                    }
                }
            }

            scene.Add(new Sphere(new Vector3(0, 1, 0), 1), new Dielectric(1.5f));
            scene.Add(new Sphere(new Vector3(-4, 1, 0), 1), new Lambertian(new Vector3(0.4f, 0.2f, 0.1f)));
            scene.Add(new Sphere(new Vector3(4, 1, 0), 1), new Metal(new Vector3(0.7f, 0.6f, 0.5f), 0f));

            return scene;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationSource != null)
            {
                _cancellationSource.Cancel();
                _cancellationSource = null;

                StartStopButton.Content = "Start";
                _timer.Stop();

                return;
            }

            _cancellationSource = new CancellationTokenSource();
            StartStopButton.Content = "Stop";
            _timer.Start();

            Task.Run(Run);
        }

        private async Task Run()
        {
            var cancellationToken = _cancellationSource!.Token;
            var scene = CreateBookScene();
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
                            var ray = scene.Camera.GetRay(u, v);
                            pixelColor += RayColor(ray, scene, _maxDepth);
                        }

                        _frameBuffer.TryAdd((x, y), pixelColor.GetColor(_samplesPerPixel));
                    }

                    await Dispatcher.InvokeAsync(() => SkiaElement.InvalidateVisual());
                }, cancellationToken));

                _stopwatch = Stopwatch.StartNew();

                await Task.WhenAll(tasks);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            finally
            {
                _stopwatch?.Stop();
                StartStopButton.Content = "Start";
                _cancellationSource = null;
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

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            canvas.Clear(SKColors.White);

            // flip upside down, pixels are rendered bottom up
            canvas.Scale(1, -1, 0, _imageHeight / 2);

            foreach (var point in _frameBuffer)
            {
                canvas.DrawPoint(point.Key.x, point.Key.y, point.Value);
            }
        }

        private void SampleSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SampleSizeText.Content = $"{e.NewValue:0}";
            _samplesPerPixel = (int)e.NewValue;

            // clamp to 1, otherwise pic will end up blank
            if (_samplesPerPixel <= 0)
            {
                _samplesPerPixel = 1;
                SampleSizeText.Content = "1";
            }
        }
    }
}
