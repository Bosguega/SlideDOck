using System.Windows;
using SlideDock.ViewModels;

namespace SlideDock.Models
{
    public class AppIconDragData
    {
        public AppIconViewModel AppIcon { get; set; }
        public MenuGroupViewModel SourceGroup { get; set; }
        public Point InitialMousePosition { get; set; }
    }
}
