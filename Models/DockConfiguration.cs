using System.Collections.Generic;
using SlideDock.Models;

namespace SlideDock.Models
{
    public class DockConfiguration
    {
        public bool IsExpanded { get; set; }
        public DockPosition DockPosition { get; set; }
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
        // Nova propriedade para persistência
        public DockItemType ItemType { get; set; } = DockItemType.File; // Valor padrão para compatibilidade com configs antigas
    }
}