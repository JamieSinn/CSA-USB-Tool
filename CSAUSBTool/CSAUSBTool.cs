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
        private int _selectedYear = DateTime.Today.Year;
        public string DownloadFolderPath { get; set; }
        private int DlCount { get; set; }
        public FrcSeason SelectedFrc { get; set; }
        private readonly Dictionary<int, FrcSeason> _competitions = new Dictionary<int, FrcSeason>();

        private Dictionary<string, ControlSystemsSoftware> _selectedSoftwares =
            new Dictionary<string, ControlSystemsSoftware>();

        public CSAUSBTool(IReadOnlyList<string> args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            InitializeComponent();

            if (args.Count >= 1)
            {
                Console.Out.WriteLine(args[0]);
                _competitions[9999] = new FrcSeason(9999, FrcSeason.GetWebList(args[0]));
            }

            ValidYears().ForEach(year => _competitions[year] = new FrcSeason(year));

            // Bind year objects to the selector.
            yearSelection.DataSource = new BindingSource(_competitions, null);
            yearSelection.DisplayMember = "Key";
            yearSelection.ValueMember = "Value";
            SelectedFrc = _competitions[_selectedYear];

            // Clear the selected software to ensure blank slate.
            _selectedSoftwares.Clear();
            
            ResetSelectedSoftware();
            
            // Bind software for the year to the listbox.
            SelectedItems.DataSource = new BindingSource(_selectedSoftwares, null);
            SelectedItems.DisplayMember = "Key";
            SelectedItems.ValueMember = "Value";

            downloadFolder.Text = Directory.GetCurrentDirectory() + @"\" + _selectedYear + @"\";
        }

        private void yearSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (yearSelection.SelectedItem.ToString().Equals(""))
                return;

            // Reset the selections
            var selected = (KeyValuePair<int, FrcSeason>) yearSelection.SelectedItem;
            _selectedYear = selected.Key;
            SelectedFrc = _competitions[_selectedYear];
            downloadFolder.Text = Directory.GetCurrentDirectory() + @"\" + _selectedYear + @"\";
            _selectedSoftwares.Clear();
            
            ResetSelectedSoftware();
            
            // Bind software again to ensure it gets updated.
            SelectedItems.DataSource = new BindingSource(_selectedSoftwares, null);
            SelectedItems.DisplayMember = "Key";
            SelectedItems.ValueMember = "Value";

        }

        private void ResetSelectedSoftware()
        {
            toolStripProgressBar.Value = 0;
            foreach (var css in SelectedFrc.Software)
            {
                _selectedSoftwares.Add(css.Name, css);
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
            var unitPct = 100/_selectedSoftwares.Count;
            foreach (var soft in _selectedSoftwares)
            {
                soft.Value.Download(DownloadFolderPath,
                    progress: delegate
                    {
                        if (toolStripProgressBar.Value == 100)
                        {
                            return;
                        }

                        DlCount++;
                        if (DlCount == _selectedSoftwares.Count)
                        {
                            toolStripProgressBar.Value = 100;
                        }
                        else
                        {
                            toolStripProgressBar.Value += unitPct;
                        }

                        toolStripStatusLabel.Text =  DlCount + @"/" + _selectedSoftwares.Count + @" Downloaded";
                    });
            }
        }

        private static List<int> ValidYears()
        {
            var years = new List<int>();
            using (var client = new WebClient())
            {
                var data = client.DownloadString(new Uri("https://raw.githubusercontent.com/JamieSinn/CSA-USB-Tool/master/Years.txt"));
                var lines = data.Split('\n').ToList();
                lines.ForEach(line => years.Add(int.Parse(line)));
            }

            return years;
        }
        private void SelectedItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedSoftwares.Clear();
            foreach (KeyValuePair<string, ControlSystemsSoftware> item in SelectedItems.SelectedItems)
            {
                _selectedSoftwares.Add(item.Key, item.Value);
            }
        }
    }
}