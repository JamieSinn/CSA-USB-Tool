using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAUSBTool.CrossPlatform.ViewModels
{
    public class ProgramYear
    {
        public int Year { get; set; }
        public string Program { get; set; } = "FRC";

        public List<ControlSystemSoftwareGroup> ControlSystemSoftwareGroups { get; set; } = new();
        public ProgramYear()
        {

        }
    }
}
