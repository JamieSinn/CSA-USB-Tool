using System;

namespace CSAUSBTool.CrossPlatform.Core;

public class AppSettings
{
    public string FetchMethod { get; set; } = "github_api";
    public string Program { get; set; } = "FRC";
    public string OtherProgram { get; set; } = string.Empty;
    public string YearMode { get; set; } = "largest_year";
    public string Step3ViewMode { get; set; } = "tag_view";
    public int ManualYear { get; set; } = DateTime.Now.Year;
    public bool RawSystemYearFallbackToPrevious { get; set; } = true;
    public bool AutoFetchOnStartup { get; set; } = true;
    public int AutoFetchDelaySeconds { get; set; } = 1;
    public bool DefaultVerifyAfterDownload { get; set; } = true;
    public bool LockVerifyAfterDownload { get; set; }
    public int DefaultMaxParallelDownloads { get; set; } = 3;
    public bool LockMaxParallelDownloads { get; set; }
    public string FileExistsBehavior { get; set; } = "redownload_replace";
    public bool HideSetting { get; set; }
    public string RepoApiListsUrl { get; set; } = string.Empty;
    public string RawListsUri { get; set; } = string.Empty;
}
