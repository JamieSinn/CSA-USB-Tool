using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAUSBTool.CrossPlatform.ViewModels
{
    public class ControlSystemSoftware
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public string Uri { get; set; }
        public string Platform { get; set; }

        public ControlSystemSoftware()
        {

        }
    }
}
