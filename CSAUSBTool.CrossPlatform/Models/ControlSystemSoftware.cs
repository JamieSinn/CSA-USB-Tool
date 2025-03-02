﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSAUSBTool.CrossPlatform.Core;
using ReactiveUI;

namespace CSAUSBTool.CrossPlatform.Models
{
    public class ControlSystemSoftware : ReactiveObject
    {
        public string Name { get; set; }
        public string? FileName { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string Uri { get; set; }
        public string? Hash { get; set; }
        public string Platform { get; set; }

        private double _DownloadProgress;
        public double DownloadProgress
        {
            get => _DownloadProgress;
            set => this.RaiseAndSetIfChanged(ref _DownloadProgress, value);
        }

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

            var outputUri = new Uri(new Uri(outputPath), FileName);

            try
            {
                await using var existingFile = File.OpenRead(outputUri.AbsolutePath);

                if (existingFile is { Length: > 0 })
                {
                    if (Hash != null)
                    {
                        var currentHash = CalculateMD5(existingFile);
                        if (currentHash == Hash)
                        {
                            DownloadProgress = 100;
                            return;
                        }

                        File.Delete(outputUri.AbsolutePath);
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                // Silently catch this - this is ignored.
            }
            

            using var client =
                new HttpClientDownloadWithProgress(Uri, outputUri.AbsolutePath);
            //client.ProgressChanged += _8kbBuffer;
            client.ProgressChanged += UpdateProgress;
            await client.StartDownload(token);
        }

        private void UpdateProgress(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
        {
            Debug.WriteLine($"Downloaded {totalBytesDownloaded} of {totalFileSize} - {progressPercentage}%");
            DownloadProgress = progressPercentage ?? 0;
        }
        private string CalculateMD5(FileStream stream)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}