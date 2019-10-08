using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

        public FIRSTSeason(int year, FIRSTProgram program)
        {
            Year = year;
            Program = program;
            Software = GetWebList();
        }

        public List<ControlSystemsSoftware> GetWebList()
        {
            return GetWebList($"https://raw.githubusercontent.com/JamieSinn/CSA-USB-Tool/master/{Program}Software{Year}.csv");
        }

        public List<ControlSystemsSoftware> GetWebList(string uri)
        {
            if (uri.StartsWith("local:"))
            {
                return GetLocalList(uri.Replace("local:", ""));
            }

            using (var client = new WebClient())
            {
                Console.WriteLine($"{Program}{Year}:{uri}");
                var data = client.DownloadString(new Uri(uri));
                var lines = data.Split('\n').ToList();
                return GetFromCsv(lines);
            }
        }

        public List<ControlSystemsSoftware> GetLocalList(string uri)
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

        private List<ControlSystemsSoftware> GetFromCsv(List<string> lines)
        {
            var ret = new List<ControlSystemsSoftware>();
            lines.ForEach(line =>
            {
                if (line.Equals("") || line.StartsWith("#")) return;
                var args = line.Split(',');
                ret.Add(new ControlSystemsSoftware(args[0], args[1], args[2], args[3], bool.Parse(args[4])));
            });

            return ret;
        }

        public override string ToString()
        {
            return $"{Program} - {Year}";
        }
    }
}
