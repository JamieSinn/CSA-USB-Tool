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

        }
    }
}