using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiscUtils.Iso9660;

namespace CSAUSBTool
{
    public class FrcSeason
    {
        public int Year;
        public List<ControlSystemsSoftware> Software;

        public FrcSeason(int year, List<ControlSystemsSoftware> software)
        {
            Year = year;
            Software = software;
        }
        public static List<ControlSystemsSoftware> GetWebList(int year)
        {
            return GetWebList(
                "https://raw.githubusercontent.com/JamieSinn/CSA-USB-Tool/master/Software" + year + ".csv");
        }

        public static List<ControlSystemsSoftware> GetWebList(string uri)
        {
            if (uri.StartsWith("local:"))
            {
                return GetLocalList(uri.Replace("local:", ""));
            }

            using (var client = new WebClient())
            {
                var data = client.DownloadString(new Uri(uri));
                var lines = data.Split('\n').ToList();
                return GetFromCsv(lines);
            }
        }

        public static List<ControlSystemsSoftware> GetLocalList(string uri)
        {

            var file = new StreamReader(uri);
            string line;
            var lines = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
               lines.Add(line);
            }
            return GetFromCsv(lines);
        }

        private static List<ControlSystemsSoftware> GetFromCsv(List<string> lines)
        {
            List<ControlSystemsSoftware> ret = new List<ControlSystemsSoftware>();

            foreach (var line in lines)
            {
                if (line.Equals("") || line.StartsWith("#")) continue;
                var args = line.Split(',');
                ret.Add(new ControlSystemsSoftware(args[0], args[1], args[2], args[3], bool.Parse(args[4])));
            }

            return ret;
        }

        public override string ToString()
        {
            return Year + "";
        }
       
    }
}