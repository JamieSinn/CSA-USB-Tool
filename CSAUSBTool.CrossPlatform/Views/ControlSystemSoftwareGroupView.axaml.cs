using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using CSAUSBTool.CrossPlatform.Models;

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
            Details.Text = "";
            if (SoftwareSelectionList.SelectedItems == null) return;
            foreach (var selectedItem in SoftwareSelectionList.SelectedItems)
            {
                var s = selectedItem as ControlSystemSoftware;
                Details.Text += $"{s.Name}\n{s.Description}\n{s.Platform}\n\n";
            }
        }
    }
}
