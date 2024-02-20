using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CSAUSBTool.Base
{
    public enum FIRSTProgram
    {
        FRC,
        FTC
    }

    public class FIRSTSeason
    {
        public int Year { get; set; }
        public List<ControlSystemsSoftware> Software { get; set; }
        public FIRSTProgram Program { get; set; }

        public FIRSTSeason(int year, FIRSTProgram program, string uri = "")
        {
            Year = year;
            Program = program;
            if (uri.Equals(""))
                uri = $"https://raw.githubusercontent.com/JamieSinn/CSA-USB-Tool/master/{Program}Software{Year}.csv";
            Software = GetWebList(uri);
        }

        public List<ControlSystemsSoftware> GetWebList(string uri)
        {
            if (uri.StartsWith("local:"))
            {
                return GetLocalList(uri.Replace("local:", ""));
            }

            var client = new RestClient(uri);
            var req = new RestRequest();
            client.DownloadDataAsync(req);
            using (var client = new WebClient())
            {
                Console.WriteLine($"{Program}{Year}:{uri}");
                var data = client.DownloadString(new Uri(uri));
                var lines = data.Split('\n').ToList();
                return GetFromCsv(lines);
            }
        }

        public List<ControlSystemsSoftware> FromLocal(string uri)
        {
            var lines = new List<string>();
            using (var file = new StreamReader(uri))
            {
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return GetFromCsv(lines);
        }

        private static List<ControlSystemsSoftware> FromCsv(List<string> lines)
        {
            var ret = new List<ControlSystemsSoftware>();
            lines.ForEach(line =>
            {
                if (line.Equals("") || line.StartsWith("#")) return;
                var args = line.Split(',');
                ret.Add(new ControlSystemsSoftware() { });
            });

            return ret;
        }

        private static List<ControlSystemsSoftware> FromJson(string json)
        {
        }

        public override string ToString()
        {
            return $"{Program} - {Year}";
        }
    }

    public class FRCSeason : FIRSTSeason
    {
        public FRCSeason(int year, string uri = "") : base(year, FIRSTProgram.FRC, uri)
        {
        }
    }

    public class FTCSeason : FIRSTSeason
    {
        public FTCSeason(int year, string uri = "") : base(year, FIRSTProgram.FTC, uri)
        {
        }
    }
}