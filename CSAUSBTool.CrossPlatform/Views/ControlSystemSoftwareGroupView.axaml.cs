using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CSAUSBTool.CrossPlatform.Core;
using CSAUSBTool.CrossPlatform.Models;
using CSAUSBTool.CrossPlatform.ViewModels;

namespace CSAUSBTool.CrossPlatform.Views
{
    public partial class ControlSystemSoftwareGroupView : UserControl
    {
        public ControlSystemSoftwareGroupView()
        {
            InitializeComponent();
            SoftwareSelectionList.SelectionChanged += ListSelectionChanged;
        }

        public void ListSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            TestText.Text = "";
            if (SoftwareSelectionList.SelectedItems == null) return;
            foreach (var selectedItem in SoftwareSelectionList.SelectedItems)
            {
                if (selectedItem as ControlSystemSoftware is { } s)
                {
                    TestText.Text += string.Format("{0}{1}{1}", s.Name, Environment.NewLine);
                }
            }
        }

        private async void Download_OnClick(object? sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Trying to download....");

            var topLevel = TopLevel.GetTopLevel(this);
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select folder to download to...",
                AllowMultiple = false
            });

            if (SoftwareSelectionList.SelectedItems == null) return;
            foreach (var selectedItem in SoftwareSelectionList.SelectedItems)
            {
                if (selectedItem as ControlSystemSoftware is { } s)
                {
                    s.Download(folder[0].Path.ToString(),
                        new CancellationToken());
                }
            }
        }
    }
}