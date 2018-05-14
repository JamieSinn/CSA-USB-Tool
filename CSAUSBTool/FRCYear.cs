using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiscUtils.Iso9660;

namespace CSAUSBTool
{
    public class FRCYear
    {
        public int Year;
        public List<ControlSystemsSoftware> Software;

        public FRCYear(int year, List<ControlSystemsSoftware> software)
        {
            Year = year;
            Software = software;
        }

        public void Download(string path, DownloadProgressChangedEventHandler progress, ToolStripLabel textProgress,
            bool async)
        {
            textProgress.Text = @"Downloading Software...";
            foreach (var soft in Software)
            {
                Console.Out.WriteLine(soft.FileName + " is being downloaded...");
                if (File.Exists(path + @"\" + soft.FileName))
                {
                    if (soft.IsValid(path))
                    {
                        Console.Out.WriteLine(soft.FileName + " already exists in target directory, skipping.");
                        continue;
                    }

                    Console.Out.WriteLine("MD5 Hash for " + soft.FileName +
                                          " was not equal to the given hash. Redownloading.");
                }

                soft.Download(path, progress, async);
            }
        }

        public void BuildISO(string sourcepath, string outputPath, ToolStripProgressBar progress)
        {
            progress.ProgressBar.Value = 0;
            CDBuilder builder = new CDBuilder
            {
                UseJoliet = true,
                VolumeIdentifier = "CSA_USB_" + Year
            };

            foreach (var soft in Software)
            {
                // Skip NI LabView due to ISO file format not supporting large files
                if (soft.FileName.Contains("NI_FRC")) continue;
                progress.ProgressBar.Value += 100 / Software.Count;
                builder.AddFile(soft.FileName, sourcepath + @"\" + soft.FileName);
            }

            builder.Build(outputPath + @"\" + builder.VolumeIdentifier + ".iso");
            progress.ProgressBar.Value = 100;
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

            using (WebClient client = new WebClient())
            {
                string data = client.DownloadString(new Uri(uri));
                List<string> lines = data.Split('\n').ToList();
                return getFromCSV(lines);
            }
        }

        public static List<ControlSystemsSoftware> GetLocalList(string uri)
        {

            StreamReader file = new StreamReader(uri);
            string line;
            List<string> lines = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
               lines.Add(line);
            }
            return getFromCSV(lines);
        }

        private static List<ControlSystemsSoftware> getFromCSV(List<string> lines)
        {
            List<ControlSystemsSoftware> ret = new List<ControlSystemsSoftware>();

            foreach (string line in lines)
            {
                if (line.Equals("") || line.StartsWith("#")) continue;
                string[] args = line.Split(',');
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