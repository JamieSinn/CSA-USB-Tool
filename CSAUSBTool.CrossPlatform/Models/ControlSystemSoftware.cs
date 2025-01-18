using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSAUSBTool.CrossPlatform.Core;

namespace CSAUSBTool.CrossPlatform.Models
{
    public class ControlSystemSoftware
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string Uri { get; set; }
        public string? Hash { get; set; }
        public string Platform { get; set; }
        public double DownloadProgress { get; set; }

        public ControlSystemSoftware()
        {
            
        }

        public async Task Download(string outputPath, CancellationToken token)
        {
            if (Uri == null)
            {
                throw new ArgumentNullException("Uri", "must not be null");
            }
            FileName ??= Uri.Split('/').Last();
            using var client = new HttpClientDownloadWithProgress(Uri, new Uri(new Uri(outputPath), FileName).AbsolutePath);
            //client.ProgressChanged += _8kbBuffer;
            client.ProgressChanged += UpdateProgress;
            await client.StartDownload(token);
        }

        private void UpdateProgress(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
        {
            Debug.WriteLine($"Downloaded {totalBytesDownloaded} of {totalFileSize} - {progressPercentage}%");
            DownloadProgress = progressPercentage ?? 0;
        }
    }
}
