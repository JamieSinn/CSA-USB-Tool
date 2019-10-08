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
        public string Name { get; }
        private string Url { get; set; }
        private string Hash { get; }
        public string FileName { get; set; }
        private bool Unzip { get; }
        private GitHubClient github;

        public ControlSystemsSoftware(string name, string fileName, string url, string hash, bool unzip)
        {
            if (url == "")
                return;

            Name = name;
            FileName = fileName;
            Url = url;
            Hash = hash;
            Unzip = unzip;
            github = new GitHubClient(new ProductHeaderValue("CSAUSBTool"));
        }

        public async void Download(string path, AsyncCompletedEventHandler progress)
        {
            if (FileName == "") return;
            //Org, repo, file Using match index to
            Regex rx = new Regex(@"({<owner>})({<repo>})({<release>})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = rx.Matches(Url);
            // Use github latest
            foreach (Match match in matches)
            {
                GroupCollection collection = match.Groups;
                var owner = collection["owner"].Value;
                var repo = collection["repo"].Value;
                var releasematch = collection["release"].Value;

                var release = await github.Repository.Release.GetLatest(owner, repo);
                foreach (var asset in release.Assets)
                {
                    if(!asset.Name.Contains(releasematch) || matches.Count == 0)
                        continue;

                    FileName = asset.Name;
                    Url = asset.BrowserDownloadUrl;
                }

            }

            if (File.Exists(path + @"\" + FileName) && IsValid(path))
            {
                progress?.Invoke(null, null);
                return;
            }

            if (Url.StartsWith("local:"))
            {
                CopyLocal(Url.ToString().Replace("local:", ""), path);
                progress?.Invoke(null, null);
                return;
            }

            await DownloadHttp(path, progress);
        }

        private async Task DownloadHttp(string path, AsyncCompletedEventHandler handler)
        {
            var noCancel = new CancellationTokenSource();

            using (var client = new HttpClientDownloadWithProgress(Url,
                path + @"\" + FileName))
            {
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
        }

        public void CopyLocal(string sourcePath, string destPath)
        {
            File.Copy(sourcePath, destPath + @"\" + FileName);
        }

        public bool IsValid(string path)
        {
            var calc = CalculateMd5(path + @"\" + FileName);
            Console.Out.WriteLine(FileName + " provided md5: " + Hash + " calculated md5: " + calc);
            return Hash == calc;
        }

        public void UnzipFile(string path)
        {
            if (!Unzip) return;
            ZipFile.ExtractToDirectory(path + @"\" + FileName, FileName);
        }

        private static string CalculateMd5(string filepath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filepath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}