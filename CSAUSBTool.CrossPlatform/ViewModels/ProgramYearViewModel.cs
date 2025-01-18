﻿using CSAUSBTool.CrossPlatform.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;


namespace CSAUSBTool.CrossPlatform.ViewModels
{
    public class ProgramYearViewModel
    {
        public int Year { get; set; }
        public string Program { get; set; }
        public List<ControlSystemSoftwareGroup> SoftwareGroups { get; set; } = [];

        public ProgramYearViewModel(int? year, string? program)
        {
            Year = year ?? DateTime.Now.Year;
            Program = program ?? "FRC";
            DownloadSoftwareLists();
        }

        public ProgramYearViewModel()
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
                // Ignore errors that are generally 404's from mismatched years/programs for legacy compat.
            }
        }
    }
}