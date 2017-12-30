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

        public CSAUSBTool()
        {
            InitializeComponent();
            competitions[2017] = new FRCYear(2017,
                new List<ControlSystemsSoftware>
                {
                    new ControlSystemsSoftware("NI-Update",
                        "FRCUpdateSuite_2017.2.0.zip",
                        "http://ftp.ni.com/support/softlib/first/frc/FileAttachments/FRCUpdateSuite_2017.2.0.zip",
                        "4b773561c66cd3e4fb06315d6e570884",
                        true),
                    new ControlSystemsSoftware("NI-LabVIEW",
                        "NI_FRC2017.zip",
                        "http://ftp.ni.com/support/softlib/labview/labview_frc/NI_FRC2017.zip",
                        "6f3d492145ddb84527ed493690020ed9",
                        true),
                    new ControlSystemsSoftware("C++Toolchain",
                        "FRC-2017-Windows-Toolchain-4.9.3.msi",
                        "http://first.wpi.edu/FRC/roborio/toolchains/FRC-2017-Windows-Toolchain-4.9.3.msi",
                        "48d500157900ec7532e417e118880a8d",
                        false),
                    new ControlSystemsSoftware("CTRE-Libraries",
                        "CTRE Toolsuite v4.4.1.12.zip",
                        "http://www.ctr-electronics.com/downloads/installers/CTRE%20Toolsuite%20v4.4.1.12.zip",
                        "e29adad28add73220792b0c919bffbb4",
                        true),
                    new ControlSystemsSoftware("EclipseJava",
                        "eclipse-java-oxygen-2-win32.zip",
                        "http://mirror.csclub.uwaterloo.ca/eclipse/technology/epp/downloads/release/oxygen/2/eclipse-java-oxygen-2-win32.zip",
                        "e7661f45ebd097d4b6b7ad18d5f08799",
                        true),
                    new ControlSystemsSoftware("EclipseC++",
                        "eclipse-cpp-oxygen-2-win32.zip",
                        "http://mirror.csclub.uwaterloo.ca/eclipse/technology/epp/downloads/release/oxygen/2/eclipse-cpp-oxygen-2-win32.zip",
                        "f6dca87d054379c63e76ed160b7efaf6",
                        true)
                });
            competitions[2018] = new FRCYear(2018,
                new List<ControlSystemsSoftware>
                {
                    new ControlSystemsSoftware("NI-Update",
                        "",
                        "",
                        "",
                        true),
                    new ControlSystemsSoftware("NI-LabVIEW",
                        "",
                        "",
                        "",
                        true),
                    new ControlSystemsSoftware("C++Toolchain",
                        "",
                        "",
                        "",
                        false),
                    new ControlSystemsSoftware("CTRE-Libraries",
                        "",
                        "",
                        "",
                        true),
                    new ControlSystemsSoftware("EclipseJava",
                        "eclipse-java-oxygen-2-win32.zip",
                        "http://mirror.csclub.uwaterloo.ca/eclipse/technology/epp/downloads/release/oxygen/2/eclipse-java-oxygen-2-win32.zip",
                        "e7661f45ebd097d4b6b7ad18d5f08799",
                        true),
                    new ControlSystemsSoftware("EclipseC++",
                        "eclipse-cpp-oxygen-2-win32.zip",
                        "http://mirror.csclub.uwaterloo.ca/eclipse/technology/epp/downloads/release/oxygen/2/eclipse-cpp-oxygen-2-win32.zip",
                        "f6dca87d054379c63e76ed160b7efaf6",
                        true)
                });
            buildISOButton.Enabled = false;
            yearSelection.SelectedIndex = 0;
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