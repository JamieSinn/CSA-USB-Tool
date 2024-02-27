using System.Collections.Generic;
using ReactiveUI;

namespace CSAUSBTool.CrossPlatform.ViewModels;

public class ControlSystemSoftwareGroup : ReactiveObject
{
    public string Tag { get; set; }
    public string DisplayName { get; set; }
    public List<ControlSystemSoftware> Software { get; set; }

    public ControlSystemSoftwareGroup()

    {

    }
}