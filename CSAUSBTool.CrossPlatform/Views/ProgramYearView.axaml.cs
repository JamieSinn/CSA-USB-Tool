using System;
using Avalonia.Controls;
using CSAUSBTool.CrossPlatform.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CSAUSBTool.CrossPlatform.ViewModels;

namespace CSAUSBTool.CrossPlatform.Views
{
    public partial class ProgramYearView : UserControl
    {
        private readonly ProgramYearViewModel _yearViewModel;

        public ProgramYearView(ProgramYearViewModel yearViewModel)
        {
            InitializeComponent();
            _yearViewModel = yearViewModel;
            
            
            DataContext = _yearViewModel;
        }

        public ProgramYearView() : this(new ProgramYearViewModel())
        {
        }
    }
}