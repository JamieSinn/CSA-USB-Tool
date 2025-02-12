using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSAUSBTool.CrossPlatform.Core
{
    /// <summary>
    /// Borrowed from WPILib - https://github.com/wpilibsuite
    /// </summary>
    public class HttpClientDownloadWithProgress(string downloadUrl, string destinationFilePath) : IDisposable
    {
        private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromDays(1) };

        public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

        public event ProgressChangedHandler ProgressChanged;

        public async Task<bool> StartDownload(CancellationToken token)
        {
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, token);
            return await DownloadFileFromHttpResponseMessage(response, token);
        }

        private async Task<bool> DownloadFileFromHttpResponseMessage(HttpResponseMessage response, CancellationToken token)
        {
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;

            await using var contentStream = await response.Content.ReadAsStreamAsync(token);
            return await ProcessContentStream(totalBytes, contentStream, token);
        }

        private async Task<bool> ProcessContentStream(long? totalDownloadSize, Stream contentStream, CancellationToken token)
        {
            var totalBytesRead = 0L;
            var readCount = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;

            await using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                do
                {
                    var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0)
                    {
                        isMoreToRead = false;
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                        continue;
                    }

                    await fileStream.WriteAsync(buffer, 0, bytesRead, token);

                    totalBytesRead += bytesRead;
                    readCount += 1;

                    if (readCount % 100 == 0)
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                }
                while (isMoreToRead && !token.IsCancellationRequested);
            }

            if (!token.IsCancellationRequested) return true;

            try
            {
                File.Delete(destinationFilePath);
            }
            catch (IOException)
            {   

            }
            return false;
        }

        private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
        {
            double? progressPercentage = null;
            if (totalDownloadSize.HasValue)
                progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

            ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
