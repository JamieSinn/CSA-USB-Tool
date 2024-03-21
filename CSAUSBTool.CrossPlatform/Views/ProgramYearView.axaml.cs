using System;
using Avalonia.Controls;
using CSAUSBTool.CrossPlatform.Models;
using CSAUSBTool.CrossPlatform.ViewModels;
using System.Collections.Generic;

namespace CSAUSBTool.CrossPlatform.Views
{
    public partial class ProgramYearView : UserControl
    {
        public ProgramYearView()
        {
            InitializeComponent();

            DataContext = new ProgramYear(null, null);
            Tabs.SelectionChanged += new EventHandler<SelectionChangedEventArgs>(TabChanged);
        }

        private void TabChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTab = Tabs.SelectedItem as ControlSystemSoftwareGroup;
        }

    }
}