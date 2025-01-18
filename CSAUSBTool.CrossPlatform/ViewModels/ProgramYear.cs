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
        public List<ControlSystemSoftwareGroup> SoftwareGroups { get; set; } = [];

        public ProgramYear(int? year, string? program)
        {
            Year = year ?? DateTime.Now.Year;
            Program = program ?? "FRC";
            DownloadSoftwareLists();
        }

        public ProgramYear()
        {
            Year = DateTime.Now.Year;
            Program = "FRC";
            DownloadSoftwareLists();
        }

        public void DownloadSoftwareLists()
        {
            //TODO: Change to main branch when released.
            var downloadUrl =
                $"https://raw.githubusercontent.com/JamieSinn/CSA-USB-Tool/avalonia/Lists/{Program}{Year}.json";

            using var client = new HttpClient();
            try
            {
                var software = client.GetFromJsonAsync<SeasonSoftwareList>(downloadUrl).Result;
                if (software == null) return;
                SoftwareGroups = software.Groups;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}