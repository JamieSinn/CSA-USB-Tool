using System.Collections.Generic;
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
public class DesignControlSystemSoftwareGroup : ControlSystemSoftwareGroup
{
    public DesignControlSystemSoftwareGroup()
    {
        Tag = "DesignTag";
        DisplayName = "Design Display Name";
        Software =
        [
            new ControlSystemSoftware
            {
                Name = "FRC Driver Station",
                FileName = "File1.exe",
                Description = "The FRC Driver Station is the software used to control your robot during a match.",
                Tags = ["Driver Station", "FRC"],
                Uri = "http://example.com/file1",
                Hash = null,
                Platform = "Platform1"
            },
            new ControlSystemSoftware
            {
                Name = "Software2",
                FileName = "File2.exe",
                Description = "Description2",
                Tags = ["DesignTag", "Tag4"],
                Uri = "http://example.com/file2",
                Hash = null,
                Platform = "Platform2"
            }
        ];
        SelectedSoftware = [];
    }
}
