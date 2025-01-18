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
        private readonly ProgramYear _year;

        public ProgramYearView(ProgramYear year)
        {
            InitializeComponent();
            Tabs.SelectionChanged += TabChanged;
            _year = year;
            DataContext = _year;
            
        }

        public ProgramYearView() : this(new ProgramYear())
        {
        }

        private void TabChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTab = Tabs.SelectedItem as ControlSystemSoftwareGroup;
        }
    }
}