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
    private readonly RepoSettingsService _repoSettingsService = new();
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
            Vm.EnableTemporaryMenuBar();
            Vm.StatusText = "Temporary settings override enabled for this run (--showsetting).";
        }

        RefreshMenuStateFromSettings();
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
        RefreshMenuStateFromSettings();
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

    private async void LoadJsonFromMenu_OnClick(object? sender, RoutedEventArgs e)
    {
        await RunStep1AndAutoStep2Async();
    }

    private async void OpenSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        await OpenSettingsDialogAsync();
    }

    private async System.Threading.Tasks.Task OpenSettingsDialogAsync(
        Action<SettingsWindow>? preconfigure = null,
        bool clearLoadedDataOnSave = false)
    {
        try
        {
            var settingsWindow = new SettingsWindow(_repoSettingsService);
            preconfigure?.Invoke(settingsWindow);
            var saved = await settingsWindow.ShowDialog<bool>(this);
            if (saved)
            {
                Vm.ReloadSettings();
                if (clearLoadedDataOnSave)
                {
                    Vm.ClearFetchedAndLoadedDataForSettingsChange();
                }
                RefreshMenuStateFromSettings();
                Vm.StatusText = clearLoadedDataOnSave
                    ? "Complete: Settings saved. List cleared due to program/year change. Click Step 1 to fetch again."
                    : "Complete: Settings saved.";
            }
            else
            {
                RefreshMenuStateFromSettings();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
            RefreshMenuStateFromSettings();
        }
    }

    private void Exit_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void ProgramFrc_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.Program = "FRC";
            return true;
        }, "Complete: Program set to FRC. List cleared. Click Step 1 to fetch again.", clearLoadedDataOnSave: true);
    }

    private async void ProgramFtc_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.Program = "FTC";
            return true;
        }, "Complete: Program set to FTC. List cleared. Click Step 1 to fetch again.", clearLoadedDataOnSave: true);
    }

    private async void ProgramFll_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.Program = "FLL";
            return true;
        }, "Complete: Program set to FLL. List cleared. Click Step 1 to fetch again.", clearLoadedDataOnSave: true);
    }

    private async void ProgramOther_OnClick(object? sender, RoutedEventArgs e)
    {
        await OpenSettingsDialogAsync(
            window => window.PrefillForCustomProgram(),
            clearLoadedDataOnSave: true);
    }

    private async void YearLargest_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.YearMode = "largest_year";
            return true;
        }, "Complete: Year mode set to largest available JSON year. List cleared. Click Step 1 to fetch again.", clearLoadedDataOnSave: true);
    }

    private async void YearSystem_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.YearMode = "system_year";
            return true;
        }, "Complete: Year mode set to system year. List cleared. Click Step 1 to fetch again.", clearLoadedDataOnSave: true);
    }

    private async void YearRawFallback_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.RawSystemYearFallbackToPrevious = !settings.RawSystemYearFallbackToPrevious;
            return true;
        }, "Complete: Raw system-year fallback option updated. List cleared. Click Step 1 to fetch again.", clearLoadedDataOnSave: true);
    }

    private async void YearManualPrevious_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.YearMode = "manual_year";
            settings.ManualYear = DateTime.Now.Year - 1;
            return true;
        }, $"Complete: Manual year set to {DateTime.Now.Year - 1}. List cleared. Click Step 1 to fetch again.", clearLoadedDataOnSave: true);
    }

    private async void YearManualCurrent_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.YearMode = "manual_year";
            settings.ManualYear = DateTime.Now.Year;
            return true;
        }, $"Complete: Manual year set to {DateTime.Now.Year}. List cleared. Click Step 1 to fetch again.", clearLoadedDataOnSave: true);
    }

    private async void YearManualNext_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.YearMode = "manual_year";
            settings.ManualYear = DateTime.Now.Year + 1;
            return true;
        }, $"Complete: Manual year set to {DateTime.Now.Year + 1}. List cleared. Click Step 1 to fetch again.", clearLoadedDataOnSave: true);
    }

    private async void YearManualCustom_OnClick(object? sender, RoutedEventArgs e)
    {
        await OpenSettingsDialogAsync(
            window => window.PrefillForManualYear(),
            clearLoadedDataOnSave: true);
    }

    private async void FetchGithubApi_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.FetchMethod = "github_api";
            return true;
        }, "Complete: Fetch method set to GitHub API.");
    }

    private async void FetchRaw_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.FetchMethod = "raw_url";
            if (string.Equals(settings.YearMode, "largest_year", StringComparison.OrdinalIgnoreCase))
            {
                settings.YearMode = "system_year";
            }
            return true;
        }, "Complete: Fetch method set to raw file URL.");
    }

    private async void AutoFetch_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.AutoFetchOnStartup = !settings.AutoFetchOnStartup;
            return true;
        }, "Complete: Auto fetch startup option updated.");
    }

    private async void ViewTag_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.Step3ViewMode = "tag_view";
            return true;
        }, "Complete: View set to tag view.");
    }

    private async void ViewTab_OnClick(object? sender, RoutedEventArgs e)
    {
        await UpdateSettingsAsync(settings =>
        {
            settings.Step3ViewMode = "tab_view";
            return true;
        }, "Complete: View set to tab view.");
    }

    private async System.Threading.Tasks.Task UpdateSettingsAsync(
        Func<AppSettings, bool> updater,
        string successMessage,
        bool clearLoadedDataOnSave = false)
    {
        try
        {
            var settings = _repoSettingsService.LoadSettings();
            var changed = updater(settings);
            if (!changed)
            {
                RefreshMenuStateFromSettings();
                return;
            }

            _repoSettingsService.SaveSettings(settings);
            Vm.ReloadSettings();
            if (clearLoadedDataOnSave)
            {
                Vm.ClearFetchedAndLoadedDataForSettingsChange();
            }
            RefreshMenuStateFromSettings();
            Vm.StatusText = successMessage;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex.Message);
            RefreshMenuStateFromSettings();
        }
    }

    private void RefreshMenuStateFromSettings()
    {
        var settings = _repoSettingsService.LoadSettings();
        var systemYear = DateTime.Now.Year;
        var isRaw = string.Equals(settings.FetchMethod, "raw_url", StringComparison.OrdinalIgnoreCase);
        var yearIsLargest = string.Equals(settings.YearMode, "largest_year", StringComparison.OrdinalIgnoreCase);
        var yearIsSystem = string.Equals(settings.YearMode, "system_year", StringComparison.OrdinalIgnoreCase);
        var yearIsManual = string.Equals(settings.YearMode, "manual_year", StringComparison.OrdinalIgnoreCase);

        var program = settings.Program?.Trim() ?? string.Empty;
        var isFrc = string.Equals(program, "FRC", StringComparison.OrdinalIgnoreCase);
        var isFtc = string.Equals(program, "FTC", StringComparison.OrdinalIgnoreCase);
        var isFll = string.Equals(program, "FLL", StringComparison.OrdinalIgnoreCase);
        var isOther = !isFrc && !isFtc && !isFll;

        ProgramFrcMenuItem.IsChecked = isFrc;
        ProgramFtcMenuItem.IsChecked = isFtc;
        ProgramFllMenuItem.IsChecked = isFll;
        ProgramOtherMenuItem.IsChecked = isOther;

        YearLargestMenuItem.IsChecked = yearIsLargest;
        YearLargestMenuItem.IsEnabled = !isRaw;
        YearSystemMenuItem.IsChecked = yearIsSystem;

        YearRawFallbackMenuItem.IsChecked = settings.RawSystemYearFallbackToPrevious;
        YearRawFallbackMenuItem.IsEnabled = isRaw && yearIsSystem;
        YearManualPreviousMenuItem.Header = (systemYear - 1).ToString();
        YearManualCurrentMenuItem.Header = systemYear.ToString();
        YearManualNextMenuItem.Header = (systemYear + 1).ToString();
        YearManualPreviousMenuItem.IsChecked = yearIsManual && settings.ManualYear == systemYear - 1;
        YearManualCurrentMenuItem.IsChecked = yearIsManual && settings.ManualYear == systemYear;
        YearManualNextMenuItem.IsChecked = yearIsManual && settings.ManualYear == systemYear + 1;
        YearManualCustomMenuItem.IsChecked = yearIsManual
            && settings.ManualYear != systemYear - 1
            && settings.ManualYear != systemYear
            && settings.ManualYear != systemYear + 1;

        FetchGithubApiMenuItem.IsChecked = !isRaw;
        FetchRawMenuItem.IsChecked = isRaw;
        AutoFetchMenuItem.IsChecked = settings.AutoFetchOnStartup;

        var isTabView = string.Equals(settings.Step3ViewMode, "tab_view", StringComparison.OrdinalIgnoreCase);
        ViewTagMenuItem.IsChecked = !isTabView;
        ViewTabMenuItem.IsChecked = isTabView;
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
            Vm.EnableTemporaryMenuBar();
            Vm.StatusText = "Temporary settings override enabled for this run (typed showsetting).";
            _typedKeyBuffer = string.Empty;
        }
    }
}
