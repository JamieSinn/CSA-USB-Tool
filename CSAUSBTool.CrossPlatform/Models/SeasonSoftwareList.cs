﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAUSBTool.CrossPlatform.Models
{
    public class SeasonSoftwareList
    {
        public int Year { get; set; }
        public string Program { get; set; }
        public List<ControlSystemSoftware> Software { get; set; } = new();


        public List<ControlSystemSoftwareGroup> GetGroups()
        {
            var groups = new Dictionary<string, ControlSystemSoftwareGroup>();
            Software.ForEach(s =>
            {
                // Yes, I know this is inefficient.
                s.Tags.ForEach(tag =>
                {
                    if (!groups.ContainsKey(tag))
                    {
                        groups.Add(tag, new ControlSystemSoftwareGroup(){DisplayName =tag, Tag=tag});
                    }

                    groups[tag].Software.Add(s);
                });
            });
            return new List<ControlSystemSoftwareGroup>(groups.Values);
        }
    }
}
