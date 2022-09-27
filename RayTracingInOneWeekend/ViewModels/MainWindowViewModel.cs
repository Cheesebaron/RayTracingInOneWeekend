using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using RayTracingInOneWeekend.Scenes;
using SkiaSharp;

namespace RayTracingInOneWeekend.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly DispatcherTimer _timer;
    
    private Stopwatch? _stopwatch;
    private int _samplesPerPixel = 500;
    private string _timerLabel = string.Empty;
    private string _startStopLabel = "Start";
    private CancellationTokenSource? _cancellationSource;
    private int _imageWidth;
    private int _imageHeight;
    private IDictionary<SKPoint, SKColor>? _frameBuffer;

    public int SamplesPerPixel
    {
        get => _samplesPerPixel;
        set
        {
            if (SetField(ref _samplesPerPixel, value) && value < 1)
            {
                SamplesPerPixel = 1;
            }
        }
    }

    public string TimerLabel
    {
        get => _timerLabel;
        set => SetField(ref _timerLabel, value);
    }

    public string StartStopLabel
    {
        get => _startStopLabel;
        set => SetField(ref _startStopLabel, value);
    }
    
    public int ImageHeight
    {
        get => _imageHeight;
        set => SetField(ref _imageHeight, value);
    }
    
    public int ImageWidth
    {
        get => _imageWidth;
        set => SetField(ref _imageWidth, value);
    }

    public IDictionary<SKPoint, SKColor>? Framebuffer
    {
        get => _frameBuffer;
        set => SetField(ref _frameBuffer, value);
    }
    
    public ICommand StartStopCommand { get; }

    public MainWindowViewModel()
    {
        _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, OnTimerElapsed);
        _timer.Stop();
        TimerLabel = TimeSpan.Zero.ToString();

        StartStopCommand = new RelayCommand(DoStartStopCommand);
    }

    private void DoStartStopCommand(object? obj)
    {
        if (_cancellationSource != null)
        {
            _cancellationSource.Cancel();
            _cancellationSource = null;

            StartStopLabel = "Start";
            _timer.Stop();

            return;
        }

        _cancellationSource = new CancellationTokenSource();
        StartStopLabel = "Stop";
        _timer.Start();

        Task.Run(Run);
    }

    private async Task Run()
    {
        var scene = SceneFactory.CreateScene(SceneType.BookScene, SceneFactory.DefaultAspectRatio);
        var sceneDrawer = new SceneDrawer(scene, SamplesPerPixel, (w, h, fb) =>
        {
            ImageWidth = w;
            ImageHeight = h;
            Framebuffer = fb;
        });
        
        _stopwatch = Stopwatch.StartNew();

        try
        {
            await sceneDrawer.Run(_cancellationSource!.Token).ConfigureAwait(false);
        }
        finally
        {
            _stopwatch.Stop();
            _timer.Stop();
            _cancellationSource!.Dispose();
            _cancellationSource = null;
        }
    }

    private void OnTimerElapsed(object? sender, EventArgs e)
    {
        if (_stopwatch == null)
        {
            TimerLabel = TimeSpan.Zero.ToString();
            return;
        }

        TimerLabel = _stopwatch.Elapsed.ToString();
    }
}