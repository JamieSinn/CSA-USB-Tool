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
    }
}