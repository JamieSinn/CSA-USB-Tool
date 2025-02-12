using System.Globalization;
using System.Text.Json;
using Avalonia.Controls.Shapes;
using CSAUSBTool.CrossPlatform.Models;
using CsvHelper;

namespace CSV_to_JSON
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: CSV-to-JSON <input.csv> <output.json>");
                return;
            }

            using var reader = new StreamReader(args[0]);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<LegacyCSVRecord>();
            var jsonObj = records.Select(r => new ControlSystemSoftware()
            {
                Name = r.FriendlyName,
                FileName = r.FileName,
                Tags = [],
                Uri = r.URL,
                Hash = r.MD5,
                Platform = "Windows",
            }).ToList();

            var json = JsonSerializer.Serialize(jsonObj);
            File.WriteAllText(args[1], json);
            Console.WriteLine($"Content written to {args[1]}");
        }
    }

    internal class LegacyCSVRecord
    {
        public string FriendlyName { get; set; }
        public string FileName { get; set; }
        public string URL { get; set; }
        public string MD5 { get; set; }
        public bool isZipped { get; set; }
    }
}