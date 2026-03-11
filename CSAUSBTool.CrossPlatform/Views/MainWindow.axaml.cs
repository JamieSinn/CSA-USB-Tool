using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CSAUSBTool.CrossPlatform.ViewModels;

namespace CSAUSBTool.CrossPlatform.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        var tagSelector = this.FindControl<ComboBox>("TagSelector");
        if (tagSelector != null)
        {
            tagSelector.SelectionChanged += TagSelection_OnChanged;
        }
    }

    private MainWindowViewModel Vm => (MainWindowViewModel)DataContext!;

    private async void FetchJsonList_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Vm.FetchJsonListAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    private async void LoadSelectedJson_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Vm.LoadSelectedJsonAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    private void TagSelection_OnChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox combo || combo.SelectedItem is not string selectedTag)
        {
            return;
        }

        if (selectedTag == "All Tags")
        {
            return;
        }

        Vm.ToggleTagSelection(selectedTag);

        // Reset so selecting the same tag later triggers again.
        combo.SelectedItem = "All Tags";
    }

    private void SelectAll_OnClick(object? sender, RoutedEventArgs e)
    {
        Vm.SelectAll();
    }

    private void DeselectAll_OnClick(object? sender, RoutedEventArgs e)
    {
        Vm.DeselectAll();
    }

    private async void SelectFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select folder to download to",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            Vm.DownloadFolder = folders[0].Path.LocalPath;
        }
    }

    private async void OpenFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (!Vm.IsDownloadFolderPathValid)
            {
                Vm.StatusText = "Error: Folder path is invalid or does not exist.";
                return;
            }

            var fullPath = Path.GetFullPath(Vm.DownloadFolder);
            OpenDirectory(fullPath);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    private async void DownloadSelected_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Vm.DownloadSelectedAsync();
            await ShowCompletionIfAnyAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    private async void VerifyMd5_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Vm.VerifyMd5Async();
            await ShowCompletionIfAnyAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
        }
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Vm.CancelCurrentOperation();
    }

    private async System.Threading.Tasks.Task ShowErrorAsync(string message)
    {
        Vm.StatusText = $"Error: {message}";

        var dialog = new Window
        {
            Title = "Error",
            Width = 600,
            Height = 180,
            Content = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(16)
            }
        };

        await dialog.ShowDialog(this);
    }

    private async System.Threading.Tasks.Task ShowCompletionIfAnyAsync()
    {
        if (!Vm.TryConsumeCompletionDialog(out var title, out var message))
        {
            return;
        }

        var dialog = new Window
        {
            Title = title,
            Width = 520,
            Height = 180,
            Content = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(16)
            }
        };

        await dialog.ShowDialog(this);
    }

    private static void OpenDirectory(string fullPath)
    {
        ProcessStartInfo psi;
        if (OperatingSystem.IsWindows())
        {
            psi = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{fullPath}\"",
                UseShellExecute = true
            };
        }
        else if (OperatingSystem.IsMacOS())
        {
            psi = new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"\"{fullPath}\"",
                UseShellExecute = false
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            psi = new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = $"\"{fullPath}\"",
                UseShellExecute = false
            };
        }
        else
        {
            throw new PlatformNotSupportedException("Open Directory is not supported on this platform.");
        }

        Process.Start(psi);
    }
}
