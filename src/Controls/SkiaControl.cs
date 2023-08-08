using System.Collections.Generic;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace RayTracingInOneWeekend.Controls;

public class SkiaControl : Control
{
    private IDictionary<ScreenPoint, Vector3>? _framebuffer;
    private int _imageHeight;
    private int _imageWidth;
    private int _samplesPerPixel;
    
    public static readonly DirectProperty<SkiaControl, IDictionary<ScreenPoint, Vector3>?> FramebufferProperty =
        AvaloniaProperty.RegisterDirect<SkiaControl, IDictionary<ScreenPoint, Vector3>?>(nameof(Framebuffer),
            control => control.Framebuffer, (control, framebuffer) => control.Framebuffer = framebuffer);

    public static readonly DirectProperty<SkiaControl, int> ImageHeightProperty =
        AvaloniaProperty.RegisterDirect<SkiaControl, int>(nameof(ImageHeight),
            control => control.ImageHeight, (control, imageHeight) => control.ImageHeight = imageHeight);
    
    public static readonly DirectProperty<SkiaControl, int> ImageWidthProperty =
        AvaloniaProperty.RegisterDirect<SkiaControl, int>(nameof(ImageWidth),
            control => control.ImageWidth, (control, imageWidth) => control.ImageWidth = imageWidth);

    public static readonly DirectProperty<SkiaControl, int> SamplesPerPixelProperty =
        AvaloniaProperty.RegisterDirect<SkiaControl, int>(nameof(SamplesPerPixel),
            control => control.SamplesPerPixel,
            (control, samplesPerPixel) => control.SamplesPerPixel = samplesPerPixel);
    
    public IDictionary<ScreenPoint, Vector3>? Framebuffer
    {
        get => _framebuffer;
        set => SetAndRaise(FramebufferProperty, ref _framebuffer, value);
    }

    public int ImageHeight
    {
        get => _imageHeight;
        set => SetAndRaise(ImageHeightProperty, ref _imageHeight, value);
    }
    
    public int ImageWidth
    {
        get => _imageWidth;
        set => SetAndRaise(ImageWidthProperty, ref _imageWidth, value);
    }
    
    public int SamplesPerPixel
    {
        get => _samplesPerPixel;
        set => SetAndRaise(ImageWidthProperty, ref _samplesPerPixel, value);
    }
    
    public SkiaControl()
    {
        ClipToBounds = true;
    }

    private sealed class CustomDrawOp : ICustomDrawOperation
    {
        private readonly SKBitmap? _drawCache;
        private SkiaControl? _control;

        public CustomDrawOp(SKBitmap? drawCache, SkiaControl control)
        {
            _drawCache = drawCache;
            _control = control;
        }
        
        public void Dispose()
        {
            // no-op
            _control = null;
        }

        public bool HitTest(Point p) => false;

        public void Render(ImmediateDrawingContext context)
        {
            if (_control?.Framebuffer == null || _drawCache == null)
                return;
            
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
                return;
            
            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            canvas.Save();
            canvas.Clear(SKColors.White);

            // flip upside down, pixels are rendered bottom up
            canvas.Scale(1, -1, 0, _control.ImageHeight / 2f);
            var samplesPerPixel = _control.SamplesPerPixel;
            
            foreach (var point in _control.Framebuffer)
            {
                if (point.Value != Vector3.Zero)
                {
                    _drawCache.SetPixel(point.Key.X, point.Key.Y, point.Value.GetColor(samplesPerPixel));
                }
            }

            canvas.DrawBitmap(_drawCache, 0, 0);
            
            canvas.Restore();
        }

        public Rect Bounds { get; init; }

        public bool Equals(ICustomDrawOperation? other) => false;
    }

    private SKBitmap? _drawCache;

    public override void Render(DrawingContext context)
    {
        if (_drawCache == null && ImageHeight > 0)
        {
            _drawCache = new SKBitmap(ImageWidth, ImageHeight, SKColorType.Bgra8888, SKAlphaType.Opaque);
        }
        
        context.Custom(new CustomDrawOp(_drawCache, this)
        {
            Bounds = new Rect(0, 0, Bounds.Width, Bounds.Height)
        });

        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }
}