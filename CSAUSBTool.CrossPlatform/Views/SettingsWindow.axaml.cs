using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CSAUSBTool.CrossPlatform.Core;

namespace CSAUSBTool.CrossPlatform.Views;

public partial class SettingsWindow : Window
{
    private readonly RepoSettingsService _repoSettingsService;
    private bool _hideSettingWarningShown;
    private bool _isLoading;

    public SettingsWindow() : this(new RepoSettingsService())
    {
    }

    public SettingsWindow(RepoSettingsService repoSettingsService)
    {
        InitializeComponent();
        _repoSettingsService = repoSettingsService;
        LoadFromSettings();
    }

    private void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var settings = BuildSettingsFromInputs();
            _repoSettingsService.SaveSettings(settings);
            Close(true);
        }
        catch (Exception ex)
        {
            ErrorTextBlock.Text = ex.Message;
            ErrorTextBlock.IsVisible = true;
        }
    }

    private void FetchMethod_OnChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateModeSpecificUi();
    }

    private void Program_OnChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateProgramUi();
    }

    private void YearMode_OnChanged(object? sender, RoutedEventArgs e)
    {
        UpdateModeSpecificUi();
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void LoadFromSettings()
    {
        _isLoading = true;
        var settings = _repoSettingsService.LoadSettings();
        SelectComboItemByTag(FetchMethodComboBox, settings.FetchMethod);

        var normalizedProgram = settings.Program;
        if (!string.Equals(normalizedProgram, "FRC", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(normalizedProgram, "FTC", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(normalizedProgram, "FLL", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(normalizedProgram, "Other", StringComparison.OrdinalIgnoreCase))
        {
            settings.OtherProgram = string.IsNullOrWhiteSpace(settings.OtherProgram) ? settings.Program : settings.OtherProgram;
            normalizedProgram = "Other";
        }

        SelectComboItemByTag(ProgramComboBox, normalizedProgram);

        OtherProgramTextBox.Text = settings.OtherProgram;
        RepoApiListsUrlTextBox.Text = settings.RepoApiListsUrl;
        RawListsUriTextBox.Text = settings.RawListsUri;

        LargestYearRadioButton.IsChecked = string.Equals(settings.YearMode, "largest_year", StringComparison.OrdinalIgnoreCase);
        SystemYearRadioButton.IsChecked = string.Equals(settings.YearMode, "system_year", StringComparison.OrdinalIgnoreCase);
        ManualYearRadioButton.IsChecked = string.Equals(settings.YearMode, "manual_year", StringComparison.OrdinalIgnoreCase);
        ManualYearTextBox.Text = settings.ManualYear.ToString();

        RawFallbackCheckBox.IsChecked = settings.RawSystemYearFallbackToPrevious;
        AutoFetchOnStartupCheckBox.IsChecked = settings.AutoFetchOnStartup;
        AutoFetchDelayTextBox.Text = settings.AutoFetchDelaySeconds.ToString();
        DefaultVerifyAfterDownloadCheckBox.IsChecked = settings.DefaultVerifyAfterDownload;
        LockVerifyAfterDownloadCheckBox.IsChecked = settings.LockVerifyAfterDownload;
        DefaultMaxParallelDownloadsTextBox.Text = settings.DefaultMaxParallelDownloads.ToString();
        LockMaxParallelDownloadsCheckBox.IsChecked = settings.LockMaxParallelDownloads;
        RedownloadReplaceRadioButton.IsChecked = string.Equals(settings.FileExistsBehavior, "redownload_replace", StringComparison.OrdinalIgnoreCase);
        VerifyThenSkipRadioButton.IsChecked = string.Equals(settings.FileExistsBehavior, "verify_then_skip_if_match", StringComparison.OrdinalIgnoreCase);
        SkipDoNothingRadioButton.IsChecked = string.Equals(settings.FileExistsBehavior, "skip_do_nothing", StringComparison.OrdinalIgnoreCase);
        Step3TagViewRadioButton.IsChecked = !string.Equals(settings.Step3ViewMode, "tab_view", StringComparison.OrdinalIgnoreCase);
        Step3TabViewRadioButton.IsChecked = string.Equals(settings.Step3ViewMode, "tab_view", StringComparison.OrdinalIgnoreCase);
        HideSettingCheckBox.IsChecked = settings.HideSetting;

        UpdateProgramUi();
        UpdateModeSpecificUi();
        _isLoading = false;
    }

    public void PrefillForCustomProgram()
    {
        SelectComboItemByTag(ProgramComboBox, "Other");
        UpdateProgramUi();
    }

    public void PrefillForManualYear()
    {
        ManualYearRadioButton.IsChecked = true;
        UpdateModeSpecificUi();
    }

    private AppSettings BuildSettingsFromInputs()
    {
        var fetchMethod = GetSelectedComboTag(FetchMethodComboBox) ?? "github_api";
        var program = GetSelectedComboTag(ProgramComboBox) ?? "FRC";
        var otherProgram = (OtherProgramTextBox.Text ?? string.Empty).Trim();

        if (string.Equals(program, "Other", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(otherProgram))
        {
            throw new InvalidOperationException("Program is set to Other. Enter a custom program value.");
        }

        var yearMode = LargestYearRadioButton.IsChecked == true
            ? "largest_year"
            : SystemYearRadioButton.IsChecked == true
                ? "system_year"
                : "manual_year";

        if (!int.TryParse(ManualYearTextBox.Text, out var manualYear) || manualYear is < 1900 or > 3000)
        {
            throw new InvalidOperationException("Manual year must be a number between 1900 and 3000.");
        }

        if (string.Equals(fetchMethod, "raw_url", StringComparison.OrdinalIgnoreCase)
            && string.Equals(yearMode, "largest_year", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Raw URL fetch mode supports system year or manual year only.");
        }

        if (!int.TryParse(AutoFetchDelayTextBox.Text, out var delaySeconds) || delaySeconds is < 0 or > 10)
        {
            throw new InvalidOperationException("Auto fetch delay must be a number between 0 and 10 seconds.");
        }

        if (!int.TryParse(DefaultMaxParallelDownloadsTextBox.Text, out var defaultMaxParallelDownloads)
            || defaultMaxParallelDownloads is < 1 or > 6)
        {
            throw new InvalidOperationException("Default max parallel download must be a number between 1 and 6.");
        }

        var fileExistsBehavior = SkipDoNothingRadioButton.IsChecked == true
            ? "skip_do_nothing"
            : VerifyThenSkipRadioButton.IsChecked == true
                ? "verify_then_skip_if_match"
                : "redownload_replace";
        var step3ViewMode = Step3TabViewRadioButton.IsChecked == true ? "tab_view" : "tag_view";

        return new AppSettings
        {
            FetchMethod = fetchMethod,
            Program = program,
            OtherProgram = otherProgram,
            YearMode = yearMode,
            ManualYear = manualYear,
            RawSystemYearFallbackToPrevious = RawFallbackCheckBox.IsChecked == true,
            AutoFetchOnStartup = AutoFetchOnStartupCheckBox.IsChecked == true,
            AutoFetchDelaySeconds = delaySeconds,
            DefaultVerifyAfterDownload = DefaultVerifyAfterDownloadCheckBox.IsChecked == true,
            LockVerifyAfterDownload = LockVerifyAfterDownloadCheckBox.IsChecked == true,
            DefaultMaxParallelDownloads = defaultMaxParallelDownloads,
            LockMaxParallelDownloads = LockMaxParallelDownloadsCheckBox.IsChecked == true,
            FileExistsBehavior = fileExistsBehavior,
            Step3ViewMode = step3ViewMode,
            HideSetting = HideSettingCheckBox.IsChecked == true,
            RepoApiListsUrl = (RepoApiListsUrlTextBox.Text ?? string.Empty).Trim(),
            RawListsUri = (RawListsUriTextBox.Text ?? string.Empty).Trim()
        };
    }

    private async void HideSetting_OnChecked(object? sender, RoutedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        if (_hideSettingWarningShown)
        {
            return;
        }

        _hideSettingWarningShown = true;
        Window? warningDialog = null;
        var dismissButton = new Button
        {
            Content = "Dismiss",
            MinWidth = 100,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        dismissButton.Click += (_, _) => warningDialog?.Close();

        var warningText = new TextBlock
        {
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Text = "Enabling this will hide the top menu bar (including Menu > Settings) on the main window. Temporary access: start with --showsetting or type showsetting on the main window. To permanently disable hide_setting, either remove the \"hide_setting\" line from config.json, or temporarily show the menu and uncheck this option."
        };

        var warningLayout = new StackPanel
        {
            Margin = new Avalonia.Thickness(16),
            Spacing = 12,
            Children = { warningText, dismissButton }
        };

        var warning = new Window
        {
            Title = "Warning",
            Width = 420,
            Height = 180,
            Content = warningLayout
        };
        warningDialog = warning;

        await warning.ShowDialog(this);
    }

    private void UpdateModeSpecificUi()
    {
        var fetchMethod = GetSelectedComboTag(FetchMethodComboBox) ?? "github_api";
        var isRawMode = string.Equals(fetchMethod, "raw_url", StringComparison.OrdinalIgnoreCase);

        LargestYearRadioButton.IsEnabled = !isRawMode;
        if (isRawMode && LargestYearRadioButton.IsChecked == true)
        {
            LargestYearRadioButton.IsChecked = false;
            SystemYearRadioButton.IsChecked = true;
        }

        RawFallbackCheckBox.IsVisible = isRawMode && SystemYearRadioButton.IsChecked == true;
    }

    private void UpdateProgramUi()
    {
        var program = GetSelectedComboTag(ProgramComboBox) ?? "FRC";
        OtherProgramTextBox.IsVisible = string.Equals(program, "Other", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetSelectedComboTag(ComboBox comboBox)
    {
        return (comboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
    }

    private static void SelectComboItemByTag(ComboBox comboBox, string targetTag)
    {
        foreach (var item in comboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem
                && string.Equals(comboBoxItem.Tag?.ToString(), targetTag, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedItem = comboBoxItem;
                return;
            }
        }
    }
}
