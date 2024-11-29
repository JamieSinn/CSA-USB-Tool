using System;
using System.Collections.Generic;
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
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string Uri { get; set; }
        public string? Hash { get; set; }
        public string Platform { get; set; }

        public ControlSystemSoftware()
        {
        }

        public async Task Download(string outputPath, HttpClientDownloadWithProgress.ProgressChangedHandler _8kbBuffer, CancellationToken token)
        {
            using var client = new HttpClientDownloadWithProgress(this.Uri, outputPath);
            client.ProgressChanged += _8kbBuffer;
            await client.StartDownload(token);
        }
    }
}
