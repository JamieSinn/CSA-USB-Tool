using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CSAUSBTool.CrossPlatform.ViewModels
{
    public class MenuItemViewModel : ViewModelBase
    {
        public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }
        public required string Header { get; set; }
        public ICommand? Command { get; set; }


        
    }
}