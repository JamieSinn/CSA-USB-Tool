using System;
using Avalonia.Controls;
using CSAUSBTool.CrossPlatform.Models;
using CSAUSBTool.CrossPlatform.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace CSAUSBTool.CrossPlatform.Views
{
    public partial class ProgramYearView : UserControl
    {
        public ProgramYearView()
        {
            InitializeComponent();

            DataContext = new ProgramYear(null, null);
            Tabs.SelectionChanged += TabChanged;
            Download.Click += Download_Click;
        }

        private async void Download_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                { Title = "Select a folder to download to", AllowMultiple = false, });
            
        }

        private void TabChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTab = Tabs.SelectedItem as ControlSystemSoftwareGroup;
        }
    }
}