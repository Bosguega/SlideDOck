using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SlideDock.ViewModels;

namespace SlideDock.Views
{
    public partial class AppIconView : UserControl
    {
        private Point _startPoint;
        private bool _isDragging;

        public AppIconView()
        {
            InitializeComponent();
        }

        #region Eventos do Mouse

        private void AppIcon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = false;
            ((FrameworkElement)sender).CaptureMouse();
        }

        private void AppIcon_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((FrameworkElement)sender).ReleaseMouseCapture();

            if (!_isDragging && this.DataContext is AppIconViewModel appIcon && appIcon.LaunchAppCommand?.CanExecute(null) == true)
            {
                appIcon.LaunchAppCommand.Execute(null);
            }

            _isDragging = false;
        }

        private void AppIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _isDragging) return;

            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (Math.Abs(diff.X) > 10 || Math.Abs(diff.Y) > 10)
            {
                _isDragging = true;

                if (this.DataContext is AppIconViewModel appIconToDrag &&
                    FindParent<MenuGroupView>(this)?.DataContext is MenuGroupViewModel sourceGroup)
                {
                    ((FrameworkElement)sender).ReleaseMouseCapture();

                    var dragData = new DataObject("SlideDockAppIcon", new AppIconDragData
                    {
                        AppIcon = appIconToDrag,
                        SourceGroup = sourceGroup
                    });

                    try
                    {
                        DragDrop.DoDragDrop(this, dragData, DragDropEffects.Move);
                    }
                    finally
                    {
                        _isDragging = false;
                    }

                    e.Handled = true;
                }
            }
        }

        private void AppIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _isDragging = false;
                ((FrameworkElement)sender).ReleaseMouseCapture();
            }
        }

        #endregion

        #region Helpers Visuais

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }

        #endregion
    }

    public class AppIconDragData
    {
        public AppIconViewModel AppIcon { get; set; }
        public MenuGroupViewModel SourceGroup { get; set; }
    }
}
