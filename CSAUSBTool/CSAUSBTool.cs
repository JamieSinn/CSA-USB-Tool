using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSAUSBTool.Base;

namespace CSAUSBTool
{
    public partial class CSAUSBTool : Form
    {

        public string DownloadFolderPath { get; set; }
        private int DlCount { get; set; }
        public FIRSTSeason SelectedSeason { get; set; }

        private List<string> ValidSeasonsList { get; set; }

        private readonly Dictionary<string, FIRSTSeason> _seasons = new();

        private string _selectedYear;

        private Dictionary<string, ControlSystemsSoftware> _selectedSoftware = new();

        public CSAUSBTool(IReadOnlyList<string> args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            InitializeComponent();
            Text = @"CSA USB Tool v2022.1";
            string currentDir = Directory.GetCurrentDirectory();
            if (Directory.GetFiles(currentDir, "*.csa").Length > 0)
            {
                _seasons["FRC9999"] = new FRCSeason(9999, $"local:{Directory.GetFiles(currentDir, "*.csa")[0]}");
            }

            ValidSeasonsList = GetValidSeasons();

            ValidSeasonsList.ForEach(year =>
            {
                if (!Enum.TryParse(year[..3], true, out FIRSTProgram program))
                    return;

                _seasons[year] = new FIRSTSeason(int.Parse(year[3..]), program);
            });
            
            // Bind year objects to the selector.
            yearSelection.DataSource = new BindingSource(_seasons, null);
            yearSelection.DisplayMember = "Key";
            yearSelection.ValueMember = "Value";
            SelectedSeason = _seasons[ValidSeasonsList.ElementAt(0)];

            // Clear the selected software to ensure blank slate.
            _selectedSoftware.Clear();
            
            ResetSelectedSoftware();
            
            // Bind software for the year to the listbox.
            SelectedItems.DataSource = new BindingSource(_selectedSoftware, null);
            SelectedItems.DisplayMember = "Key";
            SelectedItems.ValueMember = "Value";

            downloadFolder.Text = $@"{Directory.GetCurrentDirectory()}\\{_selectedYear}\\";
        }

        private void yearSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(yearSelection.SelectedItem.ToString()))
                return;

            // Reset the selections
            var selected = (KeyValuePair<string, FIRSTSeason>) yearSelection.SelectedItem;
            _selectedYear = selected.Key;
            SelectedSeason = _seasons[_selectedYear];
            downloadFolder.Text = $@"{Directory.GetCurrentDirectory()}\\{_selectedYear}\\"; 
            _selectedSoftware.Clear();
            
            ResetSelectedSoftware();
            
            // Bind software again to ensure it gets updated.
            SelectedItems.DataSource = new BindingSource(_selectedSoftware, null);
            SelectedItems.DisplayMember = "Key";
            SelectedItems.ValueMember = "Value";

        }

        private void ResetSelectedSoftware()
        {
            toolStripProgressBar.Value = 0;
            foreach (var css in SelectedSeason.Software)
            {
                _selectedSoftware.Add(css.Name, css);
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
            var unitPct = 100/_selectedSoftware.Count;
            foreach (var soft in _selectedSoftware)
            {
                soft.Value.Download(DownloadFolderPath,
                    progress: delegate
                    {
                        if (toolStripProgressBar.Value == 100)
                            return;

                        DlCount++;
                        if (DlCount == _selectedSoftware.Count)
                            toolStripProgressBar.Value = 100;
                        else
                            toolStripProgressBar.Value += unitPct;

                        toolStripStatusLabel.Text = $@"{DlCount}/{_selectedSoftware.Count} Downloaded";

                    });
            }
        }

        private static List<string> GetValidSeasons()
        {
            using var client = new HttpClient();
            var data = client
                .GetStringAsync($"https://raw.githubusercontent.com/JamieSinn/CSA-USB-Tool/master/Years.txt")
                .Result; 
            var lines = data.Split('\n').ToList();
            return lines;
        }
        private void SelectedItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedSoftware.Clear();
            foreach (KeyValuePair<string, ControlSystemsSoftware> item in SelectedItems.SelectedItems)
            {
                _selectedSoftware.Add(item.Key, item.Value);
            }
        }

        private void CSAUSBTool_Load(object sender, EventArgs e)
        {

        }
    }
}