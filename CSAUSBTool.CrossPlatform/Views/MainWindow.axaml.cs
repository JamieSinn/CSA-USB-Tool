using Avalonia.Controls;
using CSAUSBTool.CrossPlatform.ViewModels;

namespace CSAUSBTool.CrossPlatform.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainWindowViewModel();
        DataContext = vm;

    }
}
