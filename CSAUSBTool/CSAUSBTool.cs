using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using CSAUSBTool.Base;

namespace CSAUSBTool
{
    public partial class CSAUSBTool : Form
    {
        private string SelectedYear;
        public string DownloadFolderPath { get; set; }
        private int DlCount { get; set; }
        public FIRSTSeason SelectedSeason { get; set; }
        private readonly Dictionary<string, FIRSTSeason> seasons = new Dictionary<string, FIRSTSeason>();

        private Dictionary<string, ControlSystemsSoftware> SelectedSoftware =
            new Dictionary<string, ControlSystemsSoftware>();

        public CSAUSBTool(IReadOnlyList<string> args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            InitializeComponent();

            if (args.Count >= 1)
            {
                Console.Out.WriteLine(args[0]);
                seasons["FRC9999"] = new FIRSTSeason(9999, FIRSTProgram.FRC);
            }


            ValidSeasons().ForEach(year =>
            {
                if (!Enum.TryParse(year.Substring(0, 3), true, out FIRSTProgram program)) return;
                seasons[year] = new FIRSTSeason(int.Parse(year.Substring(3)), program);
            });

            // Bind year objects to the selector.
            yearSelection.DataSource = new BindingSource(seasons, null);
            yearSelection.DisplayMember = "Key";
            yearSelection.ValueMember = "Value";
            SelectedSeason = seasons[ValidSeasons().ElementAt(0)];

            // Clear the selected software to ensure blank slate.
            SelectedSoftware.Clear();
            
            ResetSelectedSoftware();
            
            // Bind software for the year to the listbox.
            SelectedItems.DataSource = new BindingSource(SelectedSoftware, null);
            SelectedItems.DisplayMember = "Key";
            SelectedItems.ValueMember = "Value";

            downloadFolder.Text = $@"{Directory.GetCurrentDirectory()}\\{SelectedYear}\\";
        }

        private void yearSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (yearSelection.SelectedItem.ToString().Equals(""))
                return;

            // Reset the selections
            var selected = (KeyValuePair<string, FIRSTSeason>) yearSelection.SelectedItem;
            SelectedYear = selected.Key;
            SelectedSeason = seasons[SelectedYear];
            downloadFolder.Text = $@"{Directory.GetCurrentDirectory()}\\{SelectedYear}\\"; 
            SelectedSoftware.Clear();
            
            ResetSelectedSoftware();
            
            // Bind software again to ensure it gets updated.
            SelectedItems.DataSource = new BindingSource(SelectedSoftware, null);
            SelectedItems.DisplayMember = "Key";
            SelectedItems.ValueMember = "Value";

        }

        private void ResetSelectedSoftware()
        {
            toolStripProgressBar.Value = 0;
            foreach (var css in SelectedSeason.Software)
            {
                SelectedSoftware.Add(css.Name, css);
            }
        }

        private void downloadFolder_TextChanged(object sender, EventArgs e)
        {
            DownloadFolderPath = downloadFolder.Text;
        }

        private void downloadBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK) return;
            var folder = folderBrowserDialog.SelectedPath;
            downloadFolder.Text = folder;
            DownloadFolderPath = folder;
        }

 

        private void downloadButton_Click(object sender, EventArgs e)
        {
            toolStripProgressBar.Value = 0;
            DlCount = 0;
            Directory.CreateDirectory(DownloadFolderPath);
            var unitPct = 100/SelectedSoftware.Count;
            foreach (var soft in SelectedSoftware)
            {
                soft.Value.Download(DownloadFolderPath,
                    progress: delegate
                    {
                        if (toolStripProgressBar.Value == 100)
                            return;

                        DlCount++;
                        if (DlCount == SelectedSoftware.Count)
                            toolStripProgressBar.Value = 100;
                        else
                            toolStripProgressBar.Value += unitPct;

                        toolStripStatusLabel.Text = $@"{DlCount}/{SelectedSoftware.Count} Downloaded";

                    });
            }
        }

        private static List<string> ValidSeasons()
        {
            var years = new List<string>();
            using (var client = new WebClient())
            {
                var data = client.DownloadString(new Uri("https://raw.githubusercontent.com/JamieSinn/CSA-USB-Tool/master/Years.txt"));
                var lines = data.Split('\n').ToList();
                lines.ForEach(line => years.Add(line));
            }
            return years;
        }
        private void SelectedItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedSoftware.Clear();
            foreach (KeyValuePair<string, ControlSystemsSoftware> item in SelectedItems.SelectedItems)
            {
                SelectedSoftware.Add(item.Key, item.Value);
            }
        }

        private void CSAUSBTool_Load(object sender, EventArgs e)
        {

        }
    }
}