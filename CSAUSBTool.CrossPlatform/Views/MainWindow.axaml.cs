using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CSAUSBTool.CrossPlatform.Core;
using CSAUSBTool.CrossPlatform.ViewModels;

namespace CSAUSBTool.CrossPlatform.Views;

public partial class MainWindow : Window
{
    private bool _startupFetchTriggered;
    private Window? _activeCompletionDialog;
    private string _typedKeyBuffer = string.Empty;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        var tagSelector = this.FindControl<ComboBox>("TagSelector");
        if (tagSelector != null)
        {
            tagSelector.SelectionChanged += TagSelection_OnChanged;
        }

        KeyDown += MainWindow_OnKeyDown;
        if (RuntimeOverrides.EnableSettingFromArg)
        {
            Vm.EnableTemporarySettingsButton();
            Vm.StatusText = "Temporary settings override enabled for this run (--showsetting).";
        }
    }

    private MainWindowViewModel Vm => (MainWindowViewModel)DataContext!;

    private async void FetchJsonList_OnClick(object? sender, RoutedEventArgs e)
    {
        await RunStep1AndAutoStep2Async();
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (_startupFetchTriggered)
        {
            return;
        }

        Vm.ReloadSettings();
        _startupFetchTriggered = true;
        if (!Vm.AutoFetchOnStartup)
        {
            return;
        }

        if (Vm.AutoFetchDelaySeconds > 0)
        {
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(Vm.AutoFetchDelaySeconds));
        }

        await RunStep1AndAutoStep2Async();
    }

    private async System.Threading.Tasks.Task RunStep1AndAutoStep2Async()
    {
        try
        {
            await Vm.FetchJsonListAsync();
            if (Vm.IsStep1Done && !string.IsNullOrWhiteSpace(Vm.SelectedJsonFile))
            {
                await Vm.LoadSelectedJsonAsync();
            }
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

    private async void OpenSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var settingsWindow = new SettingsWindow(new RepoSettingsService());
            var saved = await settingsWindow.ShowDialog<bool>(this);
            if (saved)
            {
                Vm.ReloadSettings();
                Vm.StatusText = "Complete: Settings saved.";
            }
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

    private void SelectAllInTab_OnClick(object? sender, RoutedEventArgs e)
    {
        Vm.SelectAllInSelectedTab();
    }

    private void DeselectAllInTab_OnClick(object? sender, RoutedEventArgs e)
    {
        Vm.DeselectAllInSelectedTab();
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
            CloseActiveCompletionDialog();
            await Vm.DownloadSelectedAsync();
            ShowCompletionIfAny();
            if (Vm.VerifyAfterDownload && Vm.IsStep5Done && Vm.CanVerify)
            {
                await Vm.VerifyMd5Async();
                ShowCompletionIfAny();
            }
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
            CloseActiveCompletionDialog();
            await Vm.VerifyMd5Async();
            ShowCompletionIfAny();
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

    private void ShowCompletionIfAny()
    {
        if (!Vm.TryConsumeCompletionDialog(out var title, out var message))
        {
            return;
        }

        CloseActiveCompletionDialog();
        var isVerifyResult = string.Equals(title, "Verify Result", StringComparison.OrdinalIgnoreCase);
        var dialog = isVerifyResult
            ? CreateVerifyResultDialog(title, message)
            : CreateDownloadResultDialog(title, message);

        if (isVerifyResult)
        {
            Vm.SetStep6Done(true);
        }

        dialog.Closed += (_, _) =>
        {
            if (isVerifyResult)
            {
                Vm.SetStep6Done(false);
            }
            if (ReferenceEquals(_activeCompletionDialog, dialog))
            {
                _activeCompletionDialog = null;
            }
        };
        _activeCompletionDialog = dialog;
        dialog.Show(this);
    }

    private void CloseActiveCompletionDialog()
    {
        if (_activeCompletionDialog == null)
        {
            return;
        }

        var existing = _activeCompletionDialog;
        _activeCompletionDialog = null;
        existing.Close();
    }

    private Window CreateVerifyResultDialog(string title, string message)
    {
        var text = new TextBlock
        {
            Text = $"{message}\n\nPlease eject USB safely using your system eject option before unplugging.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };

        var openDirectoryButton = new Button { Content = "Open Directory", MinWidth = 130 };
        openDirectoryButton.Click += async (_, _) =>
        {
            try
            {
                if (!Vm.IsDownloadFolderPathValid)
                {
                    Vm.StatusText = "Error: Folder path is invalid or does not exist.";
                    return;
                }

                OpenDirectory(Path.GetFullPath(Vm.DownloadFolder));
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        };

        var closeAndContinueButton = new Button { Content = "Close and Continue", MinWidth = 140 };
        closeAndContinueButton.Click += (_, _) => _activeCompletionDialog?.Close();

        var quitButton = new Button { Content = "Step 7: Quit CSA USB Tool", MinWidth = 170 };
        quitButton.Click += (_, _) => Close();

        var buttons = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Children = { openDirectoryButton, closeAndContinueButton, quitButton }
        };

        var layout = new StackPanel
        {
            Margin = new Avalonia.Thickness(16),
            Spacing = 12,
            Children = { text, buttons }
        };

        return new Window
        {
            Title = title,
            Width = 620,
            Height = 250,
            Content = layout
        };
    }

    private Window CreateDownloadResultDialog(string title, string message)
    {
        var text = new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };

        var dismissButton = new Button { Content = "Dismiss", MinWidth = 100 };
        dismissButton.Click += (_, _) => _activeCompletionDialog?.Close();

        var buttons = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Children = { dismissButton }
        };

        var layout = new StackPanel
        {
            Margin = new Avalonia.Thickness(16),
            Spacing = 12,
            Children = { text, buttons }
        };

        return new Window
        {
            Title = title,
            Width = 520,
            Height = 200,
            Content = layout
        };
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

    private void MainWindow_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        if (e.Key == Key.Back)
        {
            _typedKeyBuffer = string.Empty;
            return;
        }

        var keyText = e.Key.ToString();
        if (string.IsNullOrWhiteSpace(keyText) || keyText.Length != 1 || !char.IsLetter(keyText[0]))
        {
            return;
        }

        _typedKeyBuffer += keyText.ToLowerInvariant();
        if (_typedKeyBuffer.Length > 24)
        {
            _typedKeyBuffer = _typedKeyBuffer[^24..];
        }

        if (_typedKeyBuffer.EndsWith("showsetting", StringComparison.Ordinal))
        {
            Vm.EnableTemporarySettingsButton();
            Vm.StatusText = "Temporary settings override enabled for this run (typed showsetting).";
            _typedKeyBuffer = string.Empty;
        }
    }
}
