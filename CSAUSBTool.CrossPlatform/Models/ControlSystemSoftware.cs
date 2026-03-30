using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CSAUSBTool.CrossPlatform.Core;
using ReactiveUI;

namespace CSAUSBTool.CrossPlatform.Models
{
    public class ControlSystemSoftware : ReactiveObject
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("FileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("Tags")]
        public List<string> Tags { get; set; } = [];

        [JsonPropertyName("Uri")]
        public string? Uri { get; set; }

        [JsonPropertyName("Hash")]
        public string? Hash { get; set; }

        [JsonPropertyName("Platform")]
        public string? Platform { get; set; }

        private double _downloadProgress;
        public double DownloadProgress
        {
            get => _downloadProgress;
            set => this.RaiseAndSetIfChanged(ref _downloadProgress, value);
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

        private string _statusText = "Pending";
        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        private string _displayText = string.Empty;
        public string DisplayText
        {
            get => _displayText;
            set => this.RaiseAndSetIfChanged(ref _displayText, value);
        }

        [JsonIgnore]
        public bool IsSelectable => !string.IsNullOrWhiteSpace(Uri);

        public void RefreshDisplayText()
        {
            var tags = Tags.Count > 0 ? $" [{string.Join(", ", Tags)}]" : string.Empty;
            DisplayText = IsSelectable ? $"{Name}{tags}" : $"{Name}{tags} (no download URI)";
        }

        public string ResolveFileName()
        {
            if (!string.IsNullOrWhiteSpace(FileName))
            {
                return FileName!;
            }

            if (!string.IsNullOrWhiteSpace(Uri))
            {
                var parsed = new global::System.Uri(Uri);
                var fromUrl = global::System.Uri.UnescapeDataString(Path.GetFileName(parsed.LocalPath));
                if (!string.IsNullOrWhiteSpace(fromUrl))
                {
                    return fromUrl;
                }
            }

            return $"{Name}.bin";
        }

        // Legacy compatibility for older views still calling Download().
        public async Task Download(string outputPath, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(Uri))
            {
                throw new InvalidOperationException("No download URI.");
            }

            var resolvedName = ResolveFileName();
            var outputFile = Path.Combine(outputPath, resolvedName);
            using var client = new HttpClientDownloadWithProgress(Uri, outputFile);
            client.ProgressChanged += (_, _, p) => DownloadProgress = p ?? 0;
            await client.StartDownload(token);
        }

        public static string CalculateHash(string filePath, string algorithmName)
        {
            using var stream = File.OpenRead(filePath);

            byte[] hash = algorithmName switch
            {
                "MD5" => MD5.HashData(stream),
                "SHA1" => SHA1.HashData(stream),
                "SHA256" => SHA256.HashData(stream),
                _ => throw new InvalidOperationException($"Invalid hash algorithm: {algorithmName}")
            };

            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        public static string? GetHashAlgorithmFromLength(string? hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                return null;
            }

            return hash.Length switch
            {
                32 => "MD5",
                40 => "SHA1",
                64 => "SHA256",
                _ => null
            };
        }
    }

    public class DesignControlSystemSoftware : ControlSystemSoftware
    {
        public DesignControlSystemSoftware()
        {
            Name = "FRC Driver Station";
            Tags = ["Driver Station", "FRC"];
            Description = "The FRC Driver Station is the software used to control your robot during a match.";
            Uri = "https://example.com/driverstation.exe";
            RefreshDisplayText();
        }
    }
}
