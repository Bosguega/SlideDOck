using System.Windows;

namespace SlideDock.Models
{
    public class DragStartInfo
    {
        public object? ViewModel { get; set; }
        public DependencyObject? DragSource { get; set; }
    }
}
