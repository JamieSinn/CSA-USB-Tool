using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CSAUSBTool.CrossPlatform.Core;

public class RepoSettingsService
{
    private const string RepoApiListsUrlKey = "repo_api_lists_url";
    private const string RawListsUriKey = "raw_lists_uri";
    private const string FetchMethodKey = "fetch_method";
    private const string ProgramKey = "program";
    private const string OtherProgramKey = "other_program";
    private const string YearModeKey = "year_mode";
    private const string Step3ViewModeKey = "step3_view_mode";
    private const string ManualYearKey = "manual_year";
    private const string RawSystemYearFallbackKey = "raw_system_year_fallback_to_previous";
    private const string AutoFetchOnStartupKey = "auto_fetch_on_startup";
    private const string AutoFetchDelaySecondsKey = "auto_fetch_delay_seconds";
    private const string DefaultVerifyAfterDownloadKey = "default_verify_after_download";
    private const string LockVerifyAfterDownloadKey = "lock_verify_after_download";
    private const string DefaultMaxParallelDownloadsKey = "default_max_parallel_downloads";
    private const string LockMaxParallelDownloadsKey = "lock_max_parallel_downloads";
    private const string FileExistsBehaviorKey = "file_exists_behavior";
    private const string HideSettingKey = "hide_setting";

    public AppSettings LoadSettings()
    {
        var settings = new AppSettings();
        var config = LoadConfigurationOrNull();
        if (config == null)
        {
            return NormalizeSettings(settings);
        }

        settings.FetchMethod = ReadString(config, FetchMethodKey, settings.FetchMethod);
        settings.Program = ReadString(config, ProgramKey, settings.Program);
        settings.OtherProgram = ReadString(config, OtherProgramKey, settings.OtherProgram);
        settings.YearMode = ReadString(config, YearModeKey, settings.YearMode);
        settings.Step3ViewMode = ReadString(config, Step3ViewModeKey, settings.Step3ViewMode);
        settings.ManualYear = ReadInt(config, ManualYearKey, settings.ManualYear);
        settings.RawSystemYearFallbackToPrevious = ReadBool(config, RawSystemYearFallbackKey, settings.RawSystemYearFallbackToPrevious);
        settings.AutoFetchOnStartup = ReadBool(config, AutoFetchOnStartupKey, settings.AutoFetchOnStartup);
        settings.AutoFetchDelaySeconds = ReadInt(config, AutoFetchDelaySecondsKey, settings.AutoFetchDelaySeconds);
        settings.DefaultVerifyAfterDownload = ReadBool(config, DefaultVerifyAfterDownloadKey, settings.DefaultVerifyAfterDownload);
        settings.LockVerifyAfterDownload = ReadBool(config, LockVerifyAfterDownloadKey, settings.LockVerifyAfterDownload);
        settings.DefaultMaxParallelDownloads = ReadInt(config, DefaultMaxParallelDownloadsKey, settings.DefaultMaxParallelDownloads);
        settings.LockMaxParallelDownloads = ReadBool(config, LockMaxParallelDownloadsKey, settings.LockMaxParallelDownloads);
        settings.FileExistsBehavior = ReadString(config, FileExistsBehaviorKey, settings.FileExistsBehavior);
        settings.HideSetting = ReadBool(config, HideSettingKey, settings.HideSetting);
        settings.RepoApiListsUrl = ReadString(config, RepoApiListsUrlKey, settings.RepoApiListsUrl);
        settings.RawListsUri = ReadString(config, RawListsUriKey, settings.RawListsUri);

        return NormalizeSettings(settings);
    }

    public void SaveSettings(AppSettings settings)
    {
        var normalized = NormalizeSettings(settings);
        ValidateBeforeSave(normalized);

        var runtimeConfigPath = GetRuntimeConfigPath();
        var root = LoadRootObjectOrCreate(runtimeConfigPath);

        root[FetchMethodKey] = normalized.FetchMethod;
        root[ProgramKey] = normalized.Program;
        root[OtherProgramKey] = normalized.OtherProgram;
        root[YearModeKey] = normalized.YearMode;
        root[Step3ViewModeKey] = normalized.Step3ViewMode;
        root[ManualYearKey] = normalized.ManualYear;
        root[RawSystemYearFallbackKey] = normalized.RawSystemYearFallbackToPrevious;
        root[AutoFetchOnStartupKey] = normalized.AutoFetchOnStartup;
        root[AutoFetchDelaySecondsKey] = normalized.AutoFetchDelaySeconds;
        root[DefaultVerifyAfterDownloadKey] = normalized.DefaultVerifyAfterDownload;
        root[LockVerifyAfterDownloadKey] = normalized.LockVerifyAfterDownload;
        root[DefaultMaxParallelDownloadsKey] = normalized.DefaultMaxParallelDownloads;
        root[LockMaxParallelDownloadsKey] = normalized.LockMaxParallelDownloads;
        root[FileExistsBehaviorKey] = normalized.FileExistsBehavior;
        if (normalized.HideSetting)
        {
            root[HideSettingKey] = true;
        }
        else
        {
            root.Remove(HideSettingKey);
        }
        root[RepoApiListsUrlKey] = normalized.RepoApiListsUrl;
        root[RawListsUriKey] = normalized.RawListsUri;

        var serialized = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        WriteConfig(runtimeConfigPath, serialized);

        var currentDirectoryConfigPath = GetCurrentDirectoryConfigPath();
        if (!string.Equals(runtimeConfigPath, currentDirectoryConfigPath, StringComparison.OrdinalIgnoreCase))
        {
            WriteConfig(currentDirectoryConfigPath, serialized);
        }
    }

    public string GetRepoApiListsUrlRequired()
    {
        var url = LoadSettings().RepoApiListsUrl.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            var existingConfigPath = ResolveExistingConfigPath();
            if (string.IsNullOrWhiteSpace(existingConfigPath))
            {
                throw new InvalidOperationException($"Missing config.json. Add \"{RepoApiListsUrlKey}\" to config.json.");
            }

            throw new InvalidOperationException($"Missing \"{RepoApiListsUrlKey}\" in config file: {existingConfigPath}");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException($"{RepoApiListsUrlKey} must be a valid absolute URL.");
        }

        return url;
    }

    public string GetRawListsUriRequired()
    {
        var rawListsUri = LoadSettings().RawListsUri.Trim();
        if (string.IsNullOrWhiteSpace(rawListsUri))
        {
            var existingConfigPath = ResolveExistingConfigPath();
            if (string.IsNullOrWhiteSpace(existingConfigPath))
            {
                throw new InvalidOperationException($"Missing config.json. Add \"{RawListsUriKey}\" to config.json.");
            }

            throw new InvalidOperationException($"Missing \"{RawListsUriKey}\" in config file: {existingConfigPath}");
        }

        if (!rawListsUri.Contains("{Program}", StringComparison.Ordinal) || !rawListsUri.Contains("{Year}", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{RawListsUriKey} must include {{Program}} and {{Year}} placeholders.");
        }

        return rawListsUri;
    }

    public string GetRepoApiListsUrlOrEmpty()
    {
        return LoadSettings().RepoApiListsUrl;
    }

    public void SaveRepoApiListsUrl(string repoApiListsUrl)
    {
        if (string.IsNullOrWhiteSpace(repoApiListsUrl))
        {
            throw new InvalidOperationException($"{RepoApiListsUrlKey} cannot be empty.");
        }

        var trimmedUrl = repoApiListsUrl.Trim();
        if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException($"{RepoApiListsUrlKey} must be a valid absolute URL.");
        }

        var settings = LoadSettings();
        settings.RepoApiListsUrl = trimmedUrl;
        SaveSettings(settings);
    }

    private static string[] GetConfigCandidates()
    {
        return
        [
            GetRuntimeConfigPath(),
            GetCurrentDirectoryConfigPath()
        ];
    }

    private static string? ResolveExistingConfigPath()
    {
        TryMigrateCurrentDirectoryConfigToRuntime();
        return GetConfigCandidates().FirstOrDefault(File.Exists);
    }

    private static string ResolveConfigPathForWrite()
    {
        TryMigrateCurrentDirectoryConfigToRuntime();
        return GetRuntimeConfigPath();
    }

    private static IConfiguration? LoadConfigurationOrNull()
    {
        TryMigrateCurrentDirectoryConfigToRuntime();
        var path = ResolveExistingConfigPath();
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return LoadConfiguration(path);
    }

    private static IConfiguration LoadConfiguration(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException($"Invalid config path: {path}");
        }

        return new ConfigurationBuilder()
            .SetBasePath(directory)
            .AddJsonFile(Path.GetFileName(path), optional: false, reloadOnChange: false)
            .Build();
    }

    private static JsonObject LoadRootObjectOrCreate(string path)
    {
        if (!File.Exists(path))
        {
            return new JsonObject();
        }

        return LoadRootObject(path);
    }

    private static JsonObject LoadRootObject(string path)
    {
        var root = JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
        if (root == null)
        {
            throw new InvalidOperationException($"Invalid JSON object in config file: {path}");
        }

        return root;
    }

    private static string ReadString(IConfiguration config, string key, string fallback)
    {
        return config[key] ?? fallback;
    }

    private static int ReadInt(IConfiguration config, string key, int fallback)
    {
        var value = config[key];
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static bool ReadBool(IConfiguration config, string key, bool fallback)
    {
        var value = config[key];
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static AppSettings NormalizeSettings(AppSettings settings)
    {
        settings.FetchMethod = NormalizeFetchMethod(settings.FetchMethod);
        settings.Program = string.IsNullOrWhiteSpace(settings.Program) ? "FRC" : settings.Program.Trim();
        settings.OtherProgram = settings.OtherProgram?.Trim() ?? string.Empty;
        settings.YearMode = NormalizeYearMode(settings.YearMode);
        settings.Step3ViewMode = NormalizeStep3ViewMode(settings.Step3ViewMode);
        settings.ManualYear = Math.Clamp(settings.ManualYear, 1900, 3000);
        settings.AutoFetchDelaySeconds = Math.Clamp(settings.AutoFetchDelaySeconds, 0, 10);
        settings.DefaultMaxParallelDownloads = Math.Clamp(settings.DefaultMaxParallelDownloads, 1, 6);
        settings.FileExistsBehavior = NormalizeFileExistsBehavior(settings.FileExistsBehavior);
        settings.RepoApiListsUrl = settings.RepoApiListsUrl?.Trim() ?? string.Empty;
        settings.RawListsUri = settings.RawListsUri?.Trim() ?? string.Empty;
        return settings;
    }

    private static string NormalizeFetchMethod(string value)
    {
        return string.Equals(value, "raw_url", StringComparison.OrdinalIgnoreCase) ? "raw_url" : "github_api";
    }

    private static string NormalizeYearMode(string value)
    {
        if (string.Equals(value, "system_year", StringComparison.OrdinalIgnoreCase))
        {
            return "system_year";
        }

        if (string.Equals(value, "manual_year", StringComparison.OrdinalIgnoreCase))
        {
            return "manual_year";
        }

        return "largest_year";
    }

    private static string NormalizeFileExistsBehavior(string value)
    {
        if (string.Equals(value, "skip_do_nothing", StringComparison.OrdinalIgnoreCase))
        {
            return "skip_do_nothing";
        }

        return string.Equals(value, "verify_then_skip_if_match", StringComparison.OrdinalIgnoreCase)
            ? "verify_then_skip_if_match"
            : "redownload_replace";
    }

    private static string NormalizeStep3ViewMode(string value)
    {
        return string.Equals(value, "tab_view", StringComparison.OrdinalIgnoreCase)
            ? "tab_view"
            : "tag_view";
    }

    private static void ValidateBeforeSave(AppSettings settings)
    {
        var isRawMode = string.Equals(settings.FetchMethod, "raw_url", StringComparison.OrdinalIgnoreCase);
        if (!isRawMode)
        {
            if (string.IsNullOrWhiteSpace(settings.RepoApiListsUrl))
            {
                throw new InvalidOperationException($"{RepoApiListsUrlKey} cannot be empty when fetch_method is github_api.");
            }

            if (!Uri.TryCreate(settings.RepoApiListsUrl, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException($"{RepoApiListsUrlKey} must be a valid absolute URL.");
            }
        }

        if (isRawMode)
        {
            if (string.IsNullOrWhiteSpace(settings.RawListsUri))
            {
                throw new InvalidOperationException($"{RawListsUriKey} cannot be empty when fetch_method is raw_url.");
            }

            if (!settings.RawListsUri.Contains("{Program}", StringComparison.Ordinal)
                || !settings.RawListsUri.Contains("{Year}", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{RawListsUriKey} must include {{Program}} and {{Year}} placeholders.");
            }
        }
    }

    private static string GetRuntimeConfigPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "config.json");
    }

    private static string GetCurrentDirectoryConfigPath()
    {
        return Path.Combine(Environment.CurrentDirectory, "config.json");
    }

    private static void TryMigrateCurrentDirectoryConfigToRuntime()
    {
        var runtimeConfigPath = GetRuntimeConfigPath();
        if (File.Exists(runtimeConfigPath))
        {
            return;
        }

        var currentDirectoryConfigPath = GetCurrentDirectoryConfigPath();
        if (!File.Exists(currentDirectoryConfigPath))
        {
            return;
        }

        var runtimeDirectory = Path.GetDirectoryName(runtimeConfigPath);
        if (!string.IsNullOrWhiteSpace(runtimeDirectory))
        {
            Directory.CreateDirectory(runtimeDirectory);
        }

        File.Copy(currentDirectoryConfigPath, runtimeConfigPath, overwrite: true);
    }

    private static void WriteConfig(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content);
    }
}
