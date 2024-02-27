using Avalonia.Controls;
using CSAUSBTool.CrossPlatform.ViewModels;
using System.Collections.Generic;

namespace CSAUSBTool.CrossPlatform.Views
{
    public partial class ProgramYearView : UserControl
    {
        public ProgramYearView()
        {
            InitializeComponent();
            
            DataContext = new ProgramYear()
            {
                Program = "FRC",
                Year = 2024,
                ControlSystemSoftwareGroups = new List<ControlSystemSoftwareGroup>
                {
                    new()
                    {
                        Tag = "FTA",
                        DisplayName = "FTA Tools",
                        Software = new List<ControlSystemSoftware>
                        {
                            new()
                            {
                                Description = "First item",
                                Name = "Software Item Name 1",
                                Tags = new List<string> {"FTA", "CSA"}
                            },
                            new()
                            {
                                Description = "Second item",
                                Name = "Software Item Name 2",
                                Tags = new List<string> {"FTA"}
                            }
                        }
                    },
                    new()
                    {
                        Tag = "CSA",
                        DisplayName = "CSA Tools",
                        Software = new List<ControlSystemSoftware>
                        {
                            new()
                            {
                                Description = "First item",
                                Name = "Software Item Name 1",
                                Tags = new List<string> {"FTA", "CSA"}
                            },
                            new()
                            {
                                Description = "Second item",
                                Name = "Software Item Name 2",
                                Tags = new List<string> {"CSA"}
                            }
                        }
                    },
                    new()
                    {
                        Tag = "Team",
                        DisplayName = "Team Tools",
                        Software = new List<ControlSystemSoftware>
                        {
                            new()
                            {
                                Description = "First item",
                                Name = "Software Item Name 1",
                                Tags = new List<string> {"FTA", "CSA"}
                            },
                            new()
                            {
                                Description = "Second item",
                                Name = "Software Item Name 2",
                                Tags = new List<string> {"CSA"}
                            }
                        }
                    }
                }
            };
            
        }
    }
}