using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DynamicData.Tests;
using ReactiveUI;

namespace CSAUSBTool.CrossPlatform.Models
{
    public class SeasonSoftwareList : ReactiveObject
    {
        public int Year { get; set; }
        public string Program { get; set; }
        [JsonPropertyName("Software")]
        public List<ControlSystemSoftware> Software { get; set; } = new();

        public List<ControlSystemSoftwareGroup> Groups => GetGroups();

        public SeasonSoftwareList()
        {

        }

        public SeasonSoftwareList(List<ControlSystemSoftware> software)
        {
            Software = software;
        }

        private List<ControlSystemSoftwareGroup> GetGroups()
        {
            var groups = new Dictionary<string, ControlSystemSoftwareGroup>();
            Software.ForEach(s =>
            {
                // Yes, I know this is inefficient.
                s.Tags.ForEach(tag =>
                {
                    if (!groups.ContainsKey(tag))
                    {
                        groups.Add(tag, new ControlSystemSoftwareGroup(){DisplayName =tag, Tag=tag, SelectedSoftware = [], Software = [] });
                    }
                    groups[tag].Software.Add(s);
                });
            });
            return [..groups.Values];
        }
    }
}
