using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CSAUSBTool.CrossPlatform.Models;


namespace CSAUSBTool.CrossPlatform.ViewModels
{
    public class ProgramYear
    {
        public int Year { get; set; }
        public string Program { get; set; }

        public List<ControlSystemSoftwareGroup> SoftwareGroups { get; set; } = new();
        public ProgramYear(int? year, string? program)
        {
            year ??= DateTime.Now.Year;
            program ??= "FRC";
            Year = (int) year;
            Program = program;

            DownloadSoftwareLists();
        }
        public ProgramYear()
        {
            Year = DateTime.Now.Year;
            Program = "FRC";
        }

        private void DownloadSoftwareLists()
        {
            var downloadUrl =
                $"https://raw.githubusercontent.com/JamieSinn/CSA-USB-Tool/avalonia/Lists/{Program}{Year}.json";

            using var client = new HttpClient();
            var software= client.GetFromJsonAsync<SeasonSoftwareList>(downloadUrl).Result;
            
                    //await client.GetStreamAsync(downloadUrl));
            SoftwareGroups = software.GetGroups();
        }
    }
}
