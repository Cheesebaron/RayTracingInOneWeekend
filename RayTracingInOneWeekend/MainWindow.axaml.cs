using Avalonia.Controls;
using RayTracingInOneWeekend.ViewModels;

namespace RayTracingInOneWeekend;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
    }
}