using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CSAUSBTool.CrossPlatform.Models;
using ReactiveUI;

namespace CSAUSBTool.CrossPlatform.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private const string DefaultRepoApiListsUrl = "https://api.github.com/repos/JamieSinn/CSA-USB-Tool/contents/Lists";

    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string> _jsonPathToUrl = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ControlSystemSoftware> _observedSoftwareItems = [];
    private CancellationTokenSource? _operationCts;
    private (string Title, string Message)? _pendingCompletionDialog;

    public ObservableCollection<string> JsonFiles { get; } = [];
    public ObservableCollection<string> TagOptions { get; } = ["All Tags"];
    public ObservableCollection<int> MaxParallelOptions { get; } = [1, 2, 3, 4, 5, 6];
    public ObservableCollection<ControlSystemSoftware> SoftwareItems { get; } = [];

    private string? _selectedJsonFile;
    public string? SelectedJsonFile
    {
        get => _selectedJsonFile;
        set => this.RaiseAndSetIfChanged(ref _selectedJsonFile, value);
    }

    private string _selectedTag = "All Tags";
    public string SelectedTag
    {
        get => _selectedTag;
        set => this.RaiseAndSetIfChanged(ref _selectedTag, value);
    }

    private int _maxParallelDownloads = 3;
    public int MaxParallelDownloads
    {
        get => _maxParallelDownloads;
        set => this.RaiseAndSetIfChanged(ref _maxParallelDownloads, value);
    }

    private string _downloadFolder = string.Empty;
    public string DownloadFolder
    {
        get => _downloadFolder;
        set
        {
            this.RaiseAndSetIfChanged(ref _downloadFolder, value);
            this.RaisePropertyChanged(nameof(HasDownloadFolder));
            this.RaisePropertyChanged(nameof(IsDownloadFolderPathValid));
            this.RaisePropertyChanged(nameof(CanDownload));
            this.RaisePropertyChanged(nameof(CanVerify));
            IsStep4Done = IsDownloadFolderPathValid;
            if (!IsBusy)
            {
                UpdateGuidanceHint();
            }
        }
    }

    public bool HasDownloadFolder => IsDownloadFolderPathValid;
    public bool IsDownloadFolderPathValid => IsValidDirectoryPath(DownloadFolder);

    private string _statusText = "Hint: Click Step 1 to fetch JSON files from the repository.";
    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    private bool _isStep1Done;
    public bool IsStep1Done
    {
        get => _isStep1Done;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isStep1Done, value);
            this.RaisePropertyChanged(nameof(Step1ButtonText));
        }
    }

    private bool _isStep2Done;
    public bool IsStep2Done
    {
        get => _isStep2Done;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isStep2Done, value);
            this.RaisePropertyChanged(nameof(Step2ButtonText));
        }
    }

    private bool _isStep4Done;
    public bool IsStep4Done
    {
        get => _isStep4Done;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isStep4Done, value);
            this.RaisePropertyChanged(nameof(Step4ButtonText));
        }
    }

    private bool _isStep5Done;
    public bool IsStep5Done
    {
        get => _isStep5Done;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isStep5Done, value);
            this.RaisePropertyChanged(nameof(Step5ButtonText));
        }
    }

    private bool _isStep6Done;
    public bool IsStep6Done
    {
        get => _isStep6Done;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isStep6Done, value);
            this.RaisePropertyChanged(nameof(Step6ButtonText));
        }
    }

    public string Step1ButtonText => IsStep1Done ? "✅ Step 1: Fetch JSON List" : "Step 1: Fetch JSON List";
    public string Step2ButtonText => IsStep2Done ? "✅ Step 2: Load Selected JSON" : "Step 2: Load Selected JSON";
    public bool IsStep3Done => SoftwareItems.Any(s => s.IsChecked && s.IsSelectable);
    public string Step3Text => IsStep3Done ? "✅ Step 3: Select by tag" : "Step 3: Select by tag";
    public string Step4ButtonText => IsStep4Done ? "✅ Step 4: Select Folder" : "Step 4: Select Folder";
    public string Step5ButtonText => IsStep5Done ? "✅ Step 5: Download Selected" : "Step 5: Download Selected";
    public string Step6ButtonText => IsStep6Done ? "✅ Step 6: Verify MD5" : "Step 6: Verify MD5";

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            this.RaiseAndSetIfChanged(ref _isBusy, value);
            this.RaisePropertyChanged(nameof(CanDownload));
            this.RaisePropertyChanged(nameof(CanVerify));
        }
    }

    public bool CanDownload => !IsBusy && HasDownloadFolder && SoftwareItems.Any(s => s.IsChecked && s.IsSelectable);
    public bool CanVerify => !IsBusy && HasDownloadFolder && SoftwareItems.Count > 0;

    public MainWindowViewModel()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CSA-USB-Tool-CrossPlatform/1.0");
    }

    public async Task FetchJsonListAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        StatusText = InProgress("Fetching JSON files...");
        JsonFiles.Clear();
        _jsonPathToUrl.Clear();
        IsStep1Done = false;
        IsStep2Done = false;
        IsStep5Done = false;
        IsStep6Done = false;

        try
        {
            var repoApiUrl = LoadRepoApiListsUrl();
            var results = new List<(string Path, string DownloadUrl)>();
            await CollectJsonFilesRecursiveAsync(repoApiUrl, results);

            foreach (var item in results.OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase))
            {
                JsonFiles.Add(item.Path);
                _jsonPathToUrl[item.Path] = item.DownloadUrl;
            }

            if (results.Count == 0)
            {
                StatusText = "Hint: No JSON files found. Check your config URL and try Step 1 again.";
                IsStep1Done = false;
                return;
            }

            var latest = results
                .OrderByDescending(x => ExtractYearFromPath(x.Path))
                .ThenBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
                .First();

            SelectedJsonFile = latest.Path;
            IsStep1Done = true;
            StatusText = $"Complete: Step 1 ✅ fetched JSON list. Auto-selected newest file: {latest.Path}.";
        }
        catch (Exception ex)
        {
            IsStep1Done = false;
            StatusText = "Error: Could not fetch JSON files. Check network/config and try Step 1 again.";
            throw new InvalidOperationException($"Failed to fetch JSON list: {ex.Message}", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task LoadSelectedJsonAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedJsonFile) || !_jsonPathToUrl.TryGetValue(SelectedJsonFile, out var downloadUrl))
        {
            throw new InvalidOperationException("Please select a JSON file first.");
        }

        IsBusy = true;
        IsStep2Done = false;
        IsStep5Done = false;
        IsStep6Done = false;
        try
        {
            await LoadJsonInternalAsync(SelectedJsonFile, downloadUrl);
            IsStep2Done = SoftwareItems.Count > 0;
        }
        catch (Exception ex)
        {
            IsStep2Done = false;
            StatusText = "Error: Could not load selected JSON. Try Step 2 again or choose another file.";
            throw new InvalidOperationException($"Failed to load selected JSON: {ex.Message}", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ToggleTagSelection(string selectedTag)
    {
        if (string.IsNullOrWhiteSpace(selectedTag) || selectedTag == "All Tags")
        {
            return;
        }

        foreach (var item in SoftwareItems.Where(s => s.IsSelectable && s.Tags.Contains(selectedTag, StringComparer.OrdinalIgnoreCase)))
        {
            item.IsChecked = !item.IsChecked;
        }

        this.RaisePropertyChanged(nameof(CanDownload));
        if (!IsBusy)
        {
            UpdateGuidanceHint();
        }
    }

    public void SelectAll()
    {
        foreach (var item in SoftwareItems.Where(s => s.IsSelectable))
        {
            item.IsChecked = true;
        }
        this.RaisePropertyChanged(nameof(CanDownload));
        if (!IsBusy)
        {
            UpdateGuidanceHint();
        }
    }

    public void DeselectAll()
    {
        foreach (var item in SoftwareItems)
        {
            item.IsChecked = false;
        }
        this.RaisePropertyChanged(nameof(CanDownload));
        if (!IsBusy)
        {
            UpdateGuidanceHint();
        }
    }

    public async Task DownloadSelectedAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!HasDownloadFolder)
        {
            throw new InvalidOperationException("Please select a download folder first.");
        }

        var selected = SoftwareItems.Where(s => s.IsChecked && s.IsSelectable).ToList();
        if (selected.Count == 0)
        {
            throw new InvalidOperationException("Please select at least one downloadable item.");
        }

        IsBusy = true;
        IsStep5Done = false;
        IsStep6Done = false;
        StatusText = InProgress($"Downloading {selected.Count} selected item(s)...");
        _operationCts = new CancellationTokenSource();
        var token = _operationCts.Token;

        foreach (var item in selected)
        {
            item.DownloadProgress = 0;
            item.StatusText = "Queued";
        }

        var successful = new HashSet<ControlSystemSoftware>();
        PreventSystemSleep();

        try
        {
            var semaphore = new SemaphoreSlim(MaxParallelDownloads);
            var tasks = selected.Select(async item =>
            {
                await semaphore.WaitAsync(token);
                try
                {
                    await DownloadItemWithRetryAsync(item, token);
                    successful.Add(item);
                }
                catch (OperationCanceledException)
                {
                    SetItemStatus(item, "Canceled");
                    throw;
                }
                catch (Exception ex)
                {
                    SetItemStatus(item, $"Failed: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                StatusText = "Aborted: Download cancelled.";
            }

            if (!token.IsCancellationRequested)
            {
                foreach (var item in successful)
                {
                    item.IsChecked = false;
                }
            }

            var failedCount = selected.Count - successful.Count;
            IsStep5Done = !token.IsCancellationRequested && failedCount == 0 && selected.Count > 0;
            StatusText = token.IsCancellationRequested
                ? $"Aborted: Download cancelled. Success: {successful.Count}, Failed: {failedCount}. Retry failed items if needed."
                : IsStep5Done
                    ? $"Complete: Step 5 ✅ download finished. Success: {successful.Count}, Failed: {failedCount}. Next: Step 6 Verify MD5."
                    : $"Complete: Download finished. Success: {successful.Count}, Failed: {failedCount}. Next: Step 6 Verify MD5.";
            _pendingCompletionDialog = (
                "Download Result",
                token.IsCancellationRequested
                    ? $"Aborted: Download cancelled.\nSuccess: {successful.Count}\nFailed: {failedCount}"
                    : IsStep5Done
                        ? $"Complete: Step 5 ✅ download finished.\nSuccess: {successful.Count}\nFailed: {failedCount}\n\nNext: Step 6 Verify MD5."
                        : $"Complete: Download finished.\nSuccess: {successful.Count}\nFailed: {failedCount}\n\nNext: Step 6 Verify MD5."
            );
        }
        finally
        {
            AllowSystemSleep();
            IsBusy = false;
            _operationCts?.Dispose();
            _operationCts = null;
            this.RaisePropertyChanged(nameof(CanDownload));
            this.RaisePropertyChanged(nameof(CanVerify));
        }
    }

    public async Task VerifyMd5Async()
    {
        if (IsBusy)
        {
            return;
        }

        if (!HasDownloadFolder)
        {
            throw new InvalidOperationException("Please select a download folder first.");
        }

        if (SoftwareItems.Count == 0)
        {
            throw new InvalidOperationException("Load a JSON file first.");
        }

        IsBusy = true;
        IsStep6Done = false;
        StatusText = InProgress("Verifying MD5 hashes...");
        _operationCts = new CancellationTokenSource();
        var token = _operationCts.Token;

        var ok = 0;
        var fail = 0;
        var missing = 0;
        var failedItems = new List<ControlSystemSoftware>();

        try
        {
            foreach (var item in SoftwareItems)
            {
                token.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(item.FileName) || string.IsNullOrWhiteSpace(item.Hash))
                {
                    SetItemStatus(item, "No MD5 metadata");
                    continue;
                }

                if (item.Hash.Length != 32)
                {
                    SetItemStatus(item, "Non-MD5 hash");
                    continue;
                }

                var filePath = Path.Combine(DownloadFolder, item.FileName);
                if (!File.Exists(filePath))
                {
                    SetItemStatus(item, "Missing file");
                    missing++;
                    failedItems.Add(item);
                    continue;
                }

                SetItemStatus(item, "Verifying");
                await Task.Run(() =>
                {
                    using var md5 = MD5.Create();
                    using var stream = File.OpenRead(filePath);
                    var hash = md5.ComputeHash(stream);
                    var actual = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
                    if (actual == item.Hash.ToLowerInvariant())
                    {
                        SetItemProgress(item, 100);
                        SetItemStatus(item, "MD5 OK");
                        Interlocked.Increment(ref ok);
                    }
                    else
                    {
                        SetItemStatus(item, "MD5 Mismatch");
                        Interlocked.Increment(ref fail);
                        lock (failedItems)
                        {
                            failedItems.Add(item);
                        }
                    }
                }, token);
            }

            foreach (var item in failedItems.Distinct())
            {
                item.IsChecked = item.IsSelectable;
            }

            IsStep6Done = fail == 0 && missing == 0;
            StatusText = IsStep6Done
                ? $"Complete: Step 6 ✅ verify finished. OK: {ok}, Fail: {fail}, Missing: {missing}."
                : $"Complete: Verify finished. OK: {ok}, Fail: {fail}, Missing: {missing}.";
            _pendingCompletionDialog = (
                "Verify Result",
                IsStep6Done
                    ? $"Complete: Step 6 ✅ verify finished.\nOK: {ok}\nFail: {fail}\nMissing: {missing}"
                    : $"Complete: Verify finished.\nOK: {ok}\nFail: {fail}\nMissing: {missing}"
            );
        }
        catch (OperationCanceledException)
        {
            IsStep6Done = false;
            StatusText = "Aborted: Verify cancelled.";
            _pendingCompletionDialog = ("Verify Result", "Aborted: Verify cancelled.");
        }
        finally
        {
            IsBusy = false;
            _operationCts?.Dispose();
            _operationCts = null;
            this.RaisePropertyChanged(nameof(CanDownload));
            this.RaisePropertyChanged(nameof(CanVerify));
        }
    }

    public void CancelCurrentOperation()
    {
        _operationCts?.Cancel();
    }

    public bool TryConsumeCompletionDialog(out string title, out string message)
    {
        if (_pendingCompletionDialog is null)
        {
            title = string.Empty;
            message = string.Empty;
            return false;
        }

        title = _pendingCompletionDialog.Value.Title;
        message = _pendingCompletionDialog.Value.Message;
        _pendingCompletionDialog = null;
        return true;
    }

    private async Task DownloadItemWithRetryAsync(ControlSystemSoftware item, CancellationToken token)
    {
        var maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                await DownloadItemAsync(item, token);
                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch when (attempt < maxAttempts)
            {
                SetItemStatus(item, $"Retry {attempt}/{maxAttempts}");
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), token);
            }
        }

        throw new InvalidOperationException("Download failed after retries.");
    }

    private async Task DownloadItemAsync(ControlSystemSoftware item, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(item.Uri))
        {
            throw new InvalidOperationException("No download URI.");
        }

        var safeFileName = SanitizeFileName(item.ResolveFileName());
        var finalPath = Path.Combine(DownloadFolder, safeFileName);
        var partPath = finalPath + ".part";

        SetItemStatus(item, "Downloading");
        SetItemProgress(item, 0);

        if (!Directory.Exists(DownloadFolder))
        {
            throw new InvalidOperationException("Selected download folder does not exist.");
        }

        var existingSize = File.Exists(partPath) ? new FileInfo(partPath).Length : 0L;

        using var request = new HttpRequestMessage(HttpMethod.Get, item.Uri);
        if (existingSize > 0)
        {
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingSize, null);
        }

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();

        if (existingSize > 0 && response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            File.Delete(partPath);
            existingSize = 0;
        }

        await using var input = await response.Content.ReadAsStreamAsync(token);
        await using var output = new FileStream(
            partPath,
            existingSize > 0 ? FileMode.Append : FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            1024 * 1024,
            useAsync: true);

        var contentLength = response.Content.Headers.ContentLength ?? 0;
        var totalExpected = existingSize + contentLength;
        var downloaded = existingSize;

        var buffer = new byte[1024 * 1024];
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var read = await input.ReadAsync(buffer, token);
            if (read == 0)
            {
                break;
            }

            await output.WriteAsync(buffer.AsMemory(0, read), token);
            downloaded += read;
            if (totalExpected > 0)
            {
                var percent = (downloaded * 100d) / totalExpected;
                SetItemProgress(item, percent);
            }
        }

        output.Close();
        if (File.Exists(finalPath))
        {
            File.Delete(finalPath);
        }
        File.Move(partPath, finalPath);

        var algo = ControlSystemSoftware.GetHashAlgorithmFromLength(item.Hash);
        if (!string.IsNullOrWhiteSpace(item.Hash) && algo != null)
        {
            SetItemStatus(item, $"Verifying {algo}");
            var actualHash = await Task.Run(() => ControlSystemSoftware.CalculateHash(finalPath, algo), token);
            if (!actualHash.Equals(item.Hash, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Hash mismatch.");
            }
        }

        SetItemProgress(item, 100);
        SetItemStatus(item, "Success");
    }

    private async Task CollectJsonFilesRecursiveAsync(string apiUrl, List<(string Path, string DownloadUrl)> results)
    {
        using var stream = await _httpClient.GetStreamAsync(apiUrl);
        using var doc = await JsonDocument.ParseAsync(stream);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var entry in doc.RootElement.EnumerateArray())
        {
            var type = entry.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;
            if (string.Equals(type, "file", StringComparison.OrdinalIgnoreCase))
            {
                var name = entry.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                var path = entry.TryGetProperty("path", out var pathProp) ? pathProp.GetString() : null;
                var downloadUrl = entry.TryGetProperty("download_url", out var downloadProp) ? downloadProp.GetString() : null;

                if (!string.IsNullOrWhiteSpace(name) && name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(downloadUrl))
                {
                    results.Add((path, downloadUrl));
                }
            }
            else if (string.Equals(type, "dir", StringComparison.OrdinalIgnoreCase))
            {
                var childUrl = entry.TryGetProperty("url", out var urlProp) ? urlProp.GetString() : null;
                if (!string.IsNullOrWhiteSpace(childUrl))
                {
                    await CollectJsonFilesRecursiveAsync(childUrl, results);
                }
            }
        }
    }

    private async Task LoadJsonInternalAsync(string selectedPath, string downloadUrl)
    {
        StatusText = InProgress($"Loading software from {selectedPath}...");

        await using var stream = await _httpClient.GetStreamAsync(downloadUrl);
        var season = await JsonSerializer.DeserializeAsync<SeasonSoftwareList>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        SoftwareItems.Clear();
        DetachSoftwareObservers();
        TagOptions.Clear();
        TagOptions.Add("All Tags");
        this.RaisePropertyChanged(nameof(IsStep3Done));
        this.RaisePropertyChanged(nameof(Step3Text));

        if (season?.Software == null || season.Software.Count == 0)
        {
            StatusText = "Hint: No software entries were found in this JSON. Select a different file.";
            this.RaisePropertyChanged(nameof(CanDownload));
            this.RaisePropertyChanged(nameof(CanVerify));
            return;
        }

        foreach (var software in season.Software)
        {
            software.DownloadProgress = 0;
            software.IsChecked = false;
            software.StatusText = "Pending";
            software.RefreshDisplayText();
            SoftwareItems.Add(software);
        }
        AttachSoftwareObservers();

        var allTags = SoftwareItems
            .SelectMany(s => s.Tags)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase);

        foreach (var tag in allTags)
        {
            TagOptions.Add(tag);
        }

        SelectedTag = "All Tags";
        StatusText = $"Complete: Step 2 ✅ loaded {SoftwareItems.Count} entries. Hint: Step 3 - select software with checkboxes.";
        this.RaisePropertyChanged(nameof(CanDownload));
        this.RaisePropertyChanged(nameof(CanVerify));
        this.RaisePropertyChanged(nameof(IsStep3Done));
        this.RaisePropertyChanged(nameof(Step3Text));
    }

    private void AttachSoftwareObservers()
    {
        foreach (var item in SoftwareItems)
        {
            if (_observedSoftwareItems.Contains(item))
            {
                continue;
            }
            item.PropertyChanged += SoftwareItemOnPropertyChanged;
            _observedSoftwareItems.Add(item);
        }
    }

    private void DetachSoftwareObservers()
    {
        foreach (var item in _observedSoftwareItems)
        {
            item.PropertyChanged -= SoftwareItemOnPropertyChanged;
        }
        _observedSoftwareItems.Clear();
    }

    private void SoftwareItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ControlSystemSoftware.IsChecked))
        {
            return;
        }

        this.RaisePropertyChanged(nameof(CanDownload));
        this.RaisePropertyChanged(nameof(IsStep3Done));
        this.RaisePropertyChanged(nameof(Step3Text));
        if (!IsBusy)
        {
            UpdateGuidanceHint();
        }
    }

    private void UpdateGuidanceHint()
    {
        if (IsBusy)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(DownloadFolder) && !IsDownloadFolderPathValid)
        {
            StatusText = "Error: Folder path is invalid or does not exist. Use an existing folder path or Step 4.";
            return;
        }

        if (SoftwareItems.Count == 0)
        {
            if (JsonFiles.Count == 0)
            {
                StatusText = "Hint: Click Step 1 to fetch JSON files from the repository.";
                return;
            }

            StatusText = string.IsNullOrWhiteSpace(SelectedJsonFile)
                ? "Hint: Choose a JSON file, then click Step 2 to load software."
                : "Hint: Click Step 2 to load software from the selected JSON.";
            return;
        }

        var selectedCount = SoftwareItems.Count(s => s.IsChecked && s.IsSelectable);
        var hasSuccessfulDownloads = SoftwareItems.Any(s => s.StatusText == "Success" || s.StatusText == "MD5 OK");

        if (selectedCount > 0 && !HasDownloadFolder)
        {
            StatusText = $"Hint: Step 4 - select a download folder for {selectedCount} selected item(s).";
            return;
        }

        if (HasDownloadFolder)
        {
            StatusText = selectedCount > 0
                ? "Hint: Step 5 - click Download Selected, or click Step 6 to Verify MD5 if files already exist."
                : hasSuccessfulDownloads
                    ? "Hint: You can click Step 6 to Verify MD5, or select more items and use Step 5."
                    : "Hint: Select software, then use Step 5 to download. You can also use Step 6 to verify existing files.";
            return;
        }

        StatusText = "Hint: Step 3 - select software with checkboxes. Then choose a folder in Step 4.";
    }

    private static bool IsValidDirectoryPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(path);
            return Directory.Exists(fullPath);
        }
        catch
        {
            return false;
        }
    }

    private static int ExtractYearFromPath(string path)
    {
        var file = Path.GetFileNameWithoutExtension(path);
        var matches = Regex.Matches(file, @"(19|20)\d{2}");
        var years = matches
            .Select(m => int.TryParse(m.Value, out var y) ? y : int.MinValue)
            .Where(y => y > 0);

        return years.DefaultIfEmpty(int.MinValue).Max();
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }

    private string LoadRepoApiListsUrl()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "config.json"),
            Path.Combine(Environment.CurrentDirectory, "config.json")
        };

        foreach (var path in candidates)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            using var stream = File.OpenRead(path);
            using var doc = JsonDocument.Parse(stream);
            if (doc.RootElement.TryGetProperty("repo_api_lists_url", out var value))
            {
                var url = value.GetString();
                if (!string.IsNullOrWhiteSpace(url))
                {
                    return url;
                }
            }
        }

        return DefaultRepoApiListsUrl;
    }

    private static void SetItemProgress(ControlSystemSoftware item, double value)
    {
        Dispatcher.UIThread.Post(() => item.DownloadProgress = value);
    }

    private static void SetItemStatus(ControlSystemSoftware item, string status)
    {
        Dispatcher.UIThread.Post(() => item.StatusText = status);
    }

    private static string InProgress(string details)
    {
        return $"In progress ⏳: {details}";
    }

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private const uint EsContinuous = 0x80000000;
    private const uint EsSystemRequired = 0x00000001;

    private static void PreventSystemSleep()
    {
        if (OperatingSystem.IsWindows())
        {
            SetThreadExecutionState(EsContinuous | EsSystemRequired);
        }
    }

    private static void AllowSystemSleep()
    {
        if (OperatingSystem.IsWindows())
        {
            SetThreadExecutionState(EsContinuous);
        }
    }
}


