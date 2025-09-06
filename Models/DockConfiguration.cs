using System.Collections.Generic;

namespace SlideDOck.Models
{
    public class DockConfiguration
    {
        public List<MenuGroupData> MenuGroups { get; set; } = new List<MenuGroupData>();
    }

    public class MenuGroupData
    {
        public string Name { get; set; }
        public bool IsExpanded { get; set; }
        public List<AppIconData> AppIcons { get; set; } = new List<AppIconData>();
    }

    public class AppIconData
    {
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
    }
}