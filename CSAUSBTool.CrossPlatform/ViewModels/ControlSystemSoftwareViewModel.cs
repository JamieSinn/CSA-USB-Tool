using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSAUSBTool.CrossPlatform.Models;

namespace CSAUSBTool.CrossPlatform.ViewModels
{
    public class ControlSystemSoftwareViewModel
    {
        public double Progress { get; set; }
        public ControlSystemSoftware Software { get; set; }
    }

    public class DesignControlSystemSoftwareViewModel : ControlSystemSoftwareViewModel
    {
        public DesignControlSystemSoftwareViewModel()
        {
            Software = new ControlSystemSoftware()
            {
                Name = "FRC Driver Station",
                Tags = new List<string>() { "Driver Station", "FRC" },
                Description = "The FRC Driver Station is the software used to control your robot during a match.",
            };
        }
    }
}
