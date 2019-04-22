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
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.SelectedItems = new System.Windows.Forms.ListBox();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // yearSelection
            // 
            this.yearSelection.FormattingEnabled = true;
            this.yearSelection.Items.AddRange(new object[] {
            "2018",
            "2017"});
            this.yearSelection.Location = new System.Drawing.Point(669, 24);
            this.yearSelection.Margin = new System.Windows.Forms.Padding(6);
            this.yearSelection.Name = "yearSelection";
            this.yearSelection.Size = new System.Drawing.Size(238, 33);
            this.yearSelection.TabIndex = 1;
            this.yearSelection.SelectedIndexChanged += new System.EventHandler(this.yearSelection_SelectedIndexChanged);
            // 
            // yearLabel
            // 
            this.yearLabel.AutoSize = true;
            this.yearLabel.Location = new System.Drawing.Point(417, 29);
            this.yearLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.yearLabel.Name = "yearLabel";
            this.yearLabel.Size = new System.Drawing.Size(244, 25);
            this.yearLabel.TabIndex = 2;
            this.yearLabel.Text = "Select Competition Year";
            // 
            // downloadButton
            // 
            this.downloadButton.Location = new System.Drawing.Point(422, 166);
            this.downloadButton.Margin = new System.Windows.Forms.Padding(6);
            this.downloadButton.Name = "downloadButton";
            this.downloadButton.Size = new System.Drawing.Size(696, 44);
            this.downloadButton.TabIndex = 5;
            this.downloadButton.Text = "Download";
            this.downloadButton.UseVisualStyleBackColor = true;
            this.downloadButton.Click += new System.EventHandler(this.downloadButton_Click);
            // 
            // downloadFolder
            // 
            this.downloadFolder.Location = new System.Drawing.Point(596, 90);
            this.downloadFolder.Margin = new System.Windows.Forms.Padding(6);
            this.downloadFolder.Name = "downloadFolder";
            this.downloadFolder.Size = new System.Drawing.Size(324, 31);
            this.downloadFolder.TabIndex = 6;
            this.downloadFolder.TextChanged += new System.EventHandler(this.downloadFolder_TextChanged);
            // 
            // downloadBrowse
            // 
            this.downloadBrowse.Location = new System.Drawing.Point(936, 86);
            this.downloadBrowse.Margin = new System.Windows.Forms.Padding(6);
            this.downloadBrowse.Name = "downloadBrowse";
            this.downloadBrowse.Size = new System.Drawing.Size(176, 44);
            this.downloadBrowse.TabIndex = 7;
            this.downloadBrowse.Text = "Browse";
            this.downloadBrowse.UseVisualStyleBackColor = true;
            this.downloadBrowse.Click += new System.EventHandler(this.downloadBrowse_Click);
            // 
            // downloadFolderLabel
            // 
            this.downloadFolderLabel.AutoSize = true;
            this.downloadFolderLabel.Location = new System.Drawing.Point(416, 96);
            this.downloadFolderLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.downloadFolderLabel.Name = "downloadFolderLabel";
            this.downloadFolderLabel.Size = new System.Drawing.Size(174, 25);
            this.downloadFolderLabel.TabIndex = 8;
            this.downloadFolderLabel.Text = "Download Folder";
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.SystemColors.Control;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 636);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 28, 0);
            this.statusStrip.Size = new System.Drawing.Size(1118, 37);
            this.statusStrip.TabIndex = 15;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(200, 31);
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(54, 32);
            this.toolStripStatusLabel.Text = "Idle";
            // 
            // SelectedItems
            // 
            this.SelectedItems.FormattingEnabled = true;
            this.SelectedItems.ItemHeight = 25;
            this.SelectedItems.Location = new System.Drawing.Point(12, 12);
            this.SelectedItems.Name = "SelectedItems";
            this.SelectedItems.ScrollAlwaysVisible = true;
            this.SelectedItems.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.SelectedItems.Size = new System.Drawing.Size(383, 604);
            this.SelectedItems.TabIndex = 17;
            this.SelectedItems.SelectedIndexChanged += new System.EventHandler(this.SelectedItems_SelectedIndexChanged);
            // 
            // CSAUSBTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1118, 673);
            this.Controls.Add(this.SelectedItems);
            this.Controls.Add(this.downloadFolderLabel);
            this.Controls.Add(this.downloadBrowse);
            this.Controls.Add(this.downloadFolder);
            this.Controls.Add(this.downloadButton);
            this.Controls.Add(this.yearLabel);
            this.Controls.Add(this.yearSelection);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
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
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ListBox SelectedItems;
        public System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
    }
}

