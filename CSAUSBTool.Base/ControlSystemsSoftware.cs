using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Octokit;

namespace CSAUSBTool.Base
{
    public class ControlSystemsSoftware
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public List<string> Tags { get; set; }
        public string Uri { get; set; }
        public string? Hash { get; set; }
        public string Platform { get; set; }

        private Dictionary<string, string> DownloadableAssets { get; set; }

        private GitHubClient github = new(new ProductHeaderValue("CSAUSBTool"));

        public ControlSystemsSoftware()
        {

        }

        private async Task ParseGitHubRelease()
        {
            var rx = new Regex(@"github://({<owner>})/({<repo>})/({<release>})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(Uri);
            foreach (Match match in matches)
            {
                Release gitHubRelease;
                var collection = match.Groups;
                var owner = collection["owner"].Value;
                var repo = collection["repo"].Value;
                var tag = collection["release"].Value;
                if (tag.Equals("latest"))
                {
                    gitHubRelease = await github.Repository.Release.GetLatest(owner, repo);
                }
                else
                {
                    gitHubRelease = await github.Repository.Release.Get(owner, repo, tag);
                }

                foreach (var asset in gitHubRelease.Assets)
                {
                    DownloadableAssets.Add(asset.Name, asset.BrowserDownloadUrl);
                }
            }
        }
        public async void Download(string path, AsyncCompletedEventHandler progress)
        {
            foreach (var (filename, uri) in DownloadableAssets)
            {
                if (filename.Equals("")) continue;

                if (!File.Exists($"{path}\\{filename}") || !IsValid(path)) continue;

                if (uri.StartsWith("file://"))
                {
                    CopyLocal(uri.Replace("file://", ""), path);
                    progress.Invoke(null, null);
                    return;
                }
                await DownloadHttp(uri, path, progress);
                progress.Invoke(null, null);
                return;

            }
        }

        private async Task DownloadHttp(string sourceUri, string destinationPath, AsyncCompletedEventHandler handler)
        {
            var noCancel = new CancellationTokenSource();

            using var client = new HttpClientDownloadWithProgress(sourceUri,
                $"{destinationPath}\\{FileName}");
            client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
            {
                if (progressPercentage == null) return;
                if ((int) progressPercentage == 100)
                {
                    handler?.Invoke(null, null);
                }
            };

            await client.StartDownload(noCancel.Token);
        }

        public void CopyLocal(string sourcePath, string destPath)
        {
            File.Copy(sourcePath, $"{destPath}\\{FileName}");
        }

        public bool IsValid(string path)
        {
            var calc = CalculateMd5($"{path}\\{FileName}");
            Console.Out.WriteLine(FileName + " provided md5: " + Hash + " calculated md5: " + calc);
            return Hash == calc;
        }

        private static string CalculateMd5(string filepath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filepath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}