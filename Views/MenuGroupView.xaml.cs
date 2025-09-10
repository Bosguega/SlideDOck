using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SlideDock.Views
{
    public partial class MenuGroupView : UserControl
    {
        public MenuGroupView()
        {
            InitializeComponent();
        }

        private void AppIcons_Drop(object sender, DragEventArgs e)
        {
            var parentDockView = FindVisualParent<ExpandedDockView>(this);
            if (parentDockView != null)
            {
                parentDockView.MainWindow_Drop(sender, e);
            }
        }

        private void AppIcons_DragEnter(object sender, DragEventArgs e)
        {
            var parentDockView = FindVisualParent<ExpandedDockView>(this);
            if (parentDockView != null)
            {
                parentDockView.MainWindow_DragEnter(sender, e);
            }
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }
    }
}