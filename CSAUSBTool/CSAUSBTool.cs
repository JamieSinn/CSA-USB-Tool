using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSAUSBTool
{
    public partial class CSAUSBTool : Form
    {
        private int selectedYear = DateTime.Today.Year;
        public string downloadFolderPath { get; set; }
        private string isoFolderPath { get; set;  }
        private bool downloaded { get; set; }
        public FRCYear selectedFrc { get; set; }
        private Dictionary<int, FRCYear> competitions = new Dictionary<int, FRCYear>();

        public CSAUSBTool(string[] args) 
        {
            InitializeComponent();
            
            if (args.Length >= 1)
            {
                Console.Out.WriteLine(args[0]);
                competitions[9999] = new FRCYear(9999, FRCYear.GetWebList(args[0]));
            }
            competitions[2018] = new FRCYear(2018, FRCYear.GetWebList(2018));
            competitions[2017] = new FRCYear(2017, FRCYear.GetWebList(2017));
            buildISOButton.Enabled = false;
            yearSelection.DataSource = new BindingSource(competitions, null);
            yearSelection.DisplayMember = "Key";
            yearSelection.ValueMember = "Value";
            selectedFrc = competitions[selectedYear];
          
            downloadFolder.Text = Directory.GetCurrentDirectory() + @"\" + selectedYear + @"\";
        }

        private void yearSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (yearSelection.SelectedItem.ToString().Equals(""))
                return;
            selectedYear = int.Parse((string) yearSelection.SelectedItem);
            selectedFrc = competitions[selectedYear];
            downloadFolder.Text = Directory.GetCurrentDirectory() + @"\" + selectedYear + @"\";
        }

        private void downloadFolder_TextChanged(object sender, EventArgs e)
        {
            downloadFolderPath = downloadFolder.Text;
        }

        private void isoFolder_TextChanged(object sender, EventArgs e)
        {
            isoFolderPath = isoFolder.Text;
        }

        private void downloadBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string folder = folderBrowserDialog.SelectedPath;
                downloadFolder.Text = folder;
                downloadFolderPath = folder;
            }
        }

        private void isoBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string folder = folderBrowserDialog.SelectedPath;
                isoFolder.Text = folder;
                isoFolderPath = folder;
            }
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            buildISOButton.Enabled = true;
            Directory.CreateDirectory(downloadFolderPath);
            selectedFrc.Download(downloadFolderPath, 
                (dlSender, dlEvent) => toolStripProgressBar.Value = dlEvent.ProgressPercentage, 
                toolStripStatusLabel,
                downloadAsyncCheckbox.Checked);
        }

        private void buildISOButton_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(isoFolderPath);
            toolStripStatusLabel.Text = @"Building ISO Image...";
            selectedFrc.BuildISO(downloadFolderPath, isoFolderPath, toolStripProgressBar);
            toolStripStatusLabel.Text = @"Idle";
        }
    }
}