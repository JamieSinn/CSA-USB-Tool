using System.Collections.Generic;
using System.Threading;
using CSAUSBTool.CrossPlatform.Core;
using ReactiveUI;

namespace CSAUSBTool.CrossPlatform.Models;

public class ControlSystemSoftwareGroup : ReactiveObject
{
    public string Tag { get; set; }
    public string DisplayName { get; set; }
    public List<ControlSystemSoftware> Software { get; set; }

    public List<ControlSystemSoftware> SelectedSoftware { get; set; }

    public ControlSystemSoftwareGroup()
    {
    }
}