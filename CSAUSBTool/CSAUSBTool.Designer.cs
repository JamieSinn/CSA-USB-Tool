namespace CSAUSBTool
{
    partial class CSAUSBTool
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CSAUSBTool));
            this.yearSelection = new System.Windows.Forms.ComboBox();
            this.yearLabel = new System.Windows.Forms.Label();
            this.downloadButton = new System.Windows.Forms.Button();
            this.downloadFolder = new System.Windows.Forms.TextBox();
            this.downloadBrowse = new System.Windows.Forms.Button();
            this.downloadFolderLabel = new System.Windows.Forms.Label();
            this.isoLabel = new System.Windows.Forms.Label();
            this.isoFolder = new System.Windows.Forms.TextBox();
            this.isoBrowse = new System.Windows.Forms.Button();
            this.buildISOButton = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.downloadAsyncCheckbox = new System.Windows.Forms.CheckBox();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // yearSelection
            // 
            this.yearSelection.FormattingEnabled = true;
            this.yearSelection.Items.AddRange(new object[] {
            "2018",
            "2017"});
            this.yearSelection.Location = new System.Drawing.Point(138, 6);
            this.yearSelection.Name = "yearSelection";
            this.yearSelection.Size = new System.Drawing.Size(121, 21);
            this.yearSelection.TabIndex = 1;
            this.yearSelection.SelectedIndexChanged += new System.EventHandler(this.yearSelection_SelectedIndexChanged);
            // 
            // yearLabel
            // 
            this.yearLabel.AutoSize = true;
            this.yearLabel.Location = new System.Drawing.Point(12, 9);
            this.yearLabel.Name = "yearLabel";
            this.yearLabel.Size = new System.Drawing.Size(120, 13);
            this.yearLabel.TabIndex = 2;
            this.yearLabel.Text = "Select Competition Year";
            // 
            // downloadButton
            // 
            this.downloadButton.Location = new System.Drawing.Point(12, 127);
            this.downloadButton.Name = "downloadButton";
            this.downloadButton.Size = new System.Drawing.Size(348, 23);
            this.downloadButton.TabIndex = 5;
            this.downloadButton.Text = "Download";
            this.downloadButton.UseVisualStyleBackColor = true;
            this.downloadButton.Click += new System.EventHandler(this.downloadButton_Click);
            // 
            // downloadFolder
            // 
            this.downloadFolder.Location = new System.Drawing.Point(102, 35);
            this.downloadFolder.Name = "downloadFolder";
            this.downloadFolder.Size = new System.Drawing.Size(164, 20);
            this.downloadFolder.TabIndex = 6;
            this.downloadFolder.TextChanged += new System.EventHandler(this.downloadFolder_TextChanged);
            // 
            // downloadBrowse
            // 
            this.downloadBrowse.Location = new System.Drawing.Point(272, 33);
            this.downloadBrowse.Name = "downloadBrowse";
            this.downloadBrowse.Size = new System.Drawing.Size(88, 23);
            this.downloadBrowse.TabIndex = 7;
            this.downloadBrowse.Text = "Browse";
            this.downloadBrowse.UseVisualStyleBackColor = true;
            this.downloadBrowse.Click += new System.EventHandler(this.downloadBrowse_Click);
            // 
            // downloadFolderLabel
            // 
            this.downloadFolderLabel.AutoSize = true;
            this.downloadFolderLabel.Location = new System.Drawing.Point(12, 38);
            this.downloadFolderLabel.Name = "downloadFolderLabel";
            this.downloadFolderLabel.Size = new System.Drawing.Size(87, 13);
            this.downloadFolderLabel.TabIndex = 8;
            this.downloadFolderLabel.Text = "Download Folder";
            // 
            // isoLabel
            // 
            this.isoLabel.AutoSize = true;
            this.isoLabel.Location = new System.Drawing.Point(12, 71);
            this.isoLabel.Name = "isoLabel";
            this.isoLabel.Size = new System.Drawing.Size(92, 13);
            this.isoLabel.TabIndex = 9;
            this.isoLabel.Text = "ISO Output Folder";
            // 
            // isoFolder
            // 
            this.isoFolder.Location = new System.Drawing.Point(102, 68);
            this.isoFolder.Name = "isoFolder";
            this.isoFolder.Size = new System.Drawing.Size(164, 20);
            this.isoFolder.TabIndex = 10;
            this.isoFolder.TextChanged += new System.EventHandler(this.isoFolder_TextChanged);
            // 
            // isoBrowse
            // 
            this.isoBrowse.Location = new System.Drawing.Point(272, 66);
            this.isoBrowse.Name = "isoBrowse";
            this.isoBrowse.Size = new System.Drawing.Size(88, 23);
            this.isoBrowse.TabIndex = 11;
            this.isoBrowse.Text = "Browse";
            this.isoBrowse.UseVisualStyleBackColor = true;
            this.isoBrowse.Click += new System.EventHandler(this.isoBrowse_Click);
            // 
            // buildISOButton
            // 
            this.buildISOButton.Location = new System.Drawing.Point(12, 156);
            this.buildISOButton.Name = "buildISOButton";
            this.buildISOButton.Size = new System.Drawing.Size(348, 23);
            this.buildISOButton.TabIndex = 12;
            this.buildISOButton.Text = "Build ISO Image";
            this.buildISOButton.UseVisualStyleBackColor = true;
            this.buildISOButton.Click += new System.EventHandler(this.buildISOButton_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 183);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(372, 22);
            this.statusStrip.TabIndex = 15;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(26, 17);
            this.toolStripStatusLabel.Text = "Idle";
            // 
            // downloadAsyncCheckbox
            // 
            this.downloadAsyncCheckbox.AutoSize = true;
            this.downloadAsyncCheckbox.Checked = true;
            this.downloadAsyncCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.downloadAsyncCheckbox.Location = new System.Drawing.Point(15, 104);
            this.downloadAsyncCheckbox.Name = "downloadAsyncCheckbox";
            this.downloadAsyncCheckbox.Size = new System.Drawing.Size(159, 17);
            this.downloadAsyncCheckbox.TabIndex = 16;
            this.downloadAsyncCheckbox.Text = "Download Async (No Unzip)";
            this.downloadAsyncCheckbox.UseVisualStyleBackColor = true;
            // 
            // CSAUSBTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 205);
            this.Controls.Add(this.downloadAsyncCheckbox);
            this.Controls.Add(this.buildISOButton);
            this.Controls.Add(this.isoBrowse);
            this.Controls.Add(this.isoFolder);
            this.Controls.Add(this.isoLabel);
            this.Controls.Add(this.downloadFolderLabel);
            this.Controls.Add(this.downloadBrowse);
            this.Controls.Add(this.downloadFolder);
            this.Controls.Add(this.downloadButton);
            this.Controls.Add(this.yearLabel);
            this.Controls.Add(this.yearSelection);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CSAUSBTool";
            this.Text = "CSA USB Tool";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox yearSelection;
        private System.Windows.Forms.Label yearLabel;
        private System.Windows.Forms.Button downloadButton;
        private System.Windows.Forms.TextBox downloadFolder;
        private System.Windows.Forms.Button downloadBrowse;
        private System.Windows.Forms.Label downloadFolderLabel;
        private System.Windows.Forms.Label isoLabel;
        private System.Windows.Forms.TextBox isoFolder;
        private System.Windows.Forms.Button isoBrowse;
        private System.Windows.Forms.Button buildISOButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.CheckBox downloadAsyncCheckbox;
    }
}

