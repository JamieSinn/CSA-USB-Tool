using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CSAUSBTool
{
    public class ControlSystemsSoftware
    {
        public string Name { get; }
        private Uri Uri { get; }
        private string Hash { get; }
        public string FileName { get; }
        private bool Unzip { get; }

        public ControlSystemsSoftware(string name, string fileName, string url, string hash, bool unzip)
        {
            if (url == "")
                return;

            Name = name;
            FileName = fileName;
            Uri = new Uri(url);
            Hash = hash;
            Unzip = unzip;
        }

        public async void Download(string path, AsyncCompletedEventHandler progress)
        {
            if (FileName == "") return;
            if (File.Exists(path + @"\" + FileName) && IsValid(path))
            {
                progress?.Invoke(null, null);
                return;
            }

            if (Uri.ToString().StartsWith("local:"))
            {
                CopyLocal(Uri.ToString().Replace("local:", ""), path);
                progress?.Invoke(null, null);
                return;
            }

            await DownloadHttp(path, progress);
        }

        private async Task DownloadHttp(string path, AsyncCompletedEventHandler handler)
        {
            var noCancel = new CancellationTokenSource();

            using (var client = new HttpClientDownloadWithProgress(Uri.ToString(),
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