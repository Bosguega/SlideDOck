using System.Windows;

namespace SlideDock.Services
{
    public interface IDragDropUIService
    {
        int GetDropIndex(DragEventArgs e, object dataContext, DependencyObject dropTarget);
        void ShowDropIndicator(DragEventArgs e, DependencyObject dropTarget);
        void HideDropIndicator();
    }
}
