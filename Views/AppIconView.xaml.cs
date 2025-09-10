using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using SlideDock.ViewModels;
using System;
using System.Diagnostics;

namespace SlideDock.Views
{
    public partial class AppIconView : UserControl
    {
        private Point _startPoint;
        private bool _isDragging = false;

        public AppIconView()
        {
            InitializeComponent();
        }

        private void AppIcon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = false;
            Debug.WriteLine($"AppIcon_PreviewMouseLeftButtonDown: _startPoint = {_startPoint}");

            ((FrameworkElement)sender).CaptureMouse();
        }

        private void AppIcon_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((FrameworkElement)sender).ReleaseMouseCapture();

            if (!_isDragging)
            {
                // Handle app launch click
                if (this.DataContext is AppIconViewModel appIcon && appIcon.LaunchAppCommand?.CanExecute(null) == true)
                {
                    appIcon.LaunchAppCommand.Execute(null);
                    Debug.WriteLine($"App launched: {appIcon.Name}");
                }
            }

            _isDragging = false;
            Debug.WriteLine("AppIcon_PreviewMouseLeftButtonUp: Mouse capture released");
        }

        private void AppIcon_MouseMove(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("AppIcon_MouseMove acionado.");

            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = _startPoint - mousePos;

                Debug.WriteLine($"  _startPoint = {_startPoint}, mousePos = {mousePos}, diff = {diff}");
                Debug.WriteLine($"  Condição de arrasto: (Abs(diff.X) > 10 = {Math.Abs(diff.X) > 10}) || (Abs(diff.Y) > 10 = {Math.Abs(diff.Y) > 10})");

                if (Math.Abs(diff.X) > 10 || Math.Abs(diff.Y) > 10)
                {
                    Debug.WriteLine("  *** Condição de arrasto ATINGIDA! ***");
                    _isDragging = true;

                    AppIconViewModel appIconToDrag = this.DataContext as AppIconViewModel;
                    Debug.WriteLine($"DataContext do AppIconView: {this.DataContext?.GetType().Name ?? "NULL"}, Valor: {this.DataContext}");
                    Debug.WriteLine($"AppIconToDrag: {appIconToDrag?.Name ?? "NULL"}");

                    MenuGroupView parentMenuGroupView = FindParent<MenuGroupView>(this);
                    Debug.WriteLine($"Parent MenuGroupView (objeto): {parentMenuGroupView?.GetType().Name ?? "NULL"}");
                    MenuGroupViewModel sourceGroup = parentMenuGroupView?.DataContext as MenuGroupViewModel;
                    Debug.WriteLine($"SourceGroup: {sourceGroup?.Name ?? "NULL"}");

                    if (appIconToDrag != null && sourceGroup != null)
                    {
                        Debug.WriteLine($"Iniciando arrasto: App='{appIconToDrag.Name}' do grupo='{sourceGroup.Name}'");

                        ((FrameworkElement)sender).ReleaseMouseCapture();

                        DataObject dragData = new DataObject("SlideDockAppIcon", new AppIconDragData
                        {
                            AppIcon = appIconToDrag,
                            SourceGroup = sourceGroup
                        });

                        try
                        {
                            DragDropEffects result = DragDrop.DoDragDrop(this, dragData, DragDropEffects.Move);
                            Debug.WriteLine($"Drag operation completed with result: {result}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Drag operation failed: {ex.Message}");
                        }
                        finally
                        {
                            _isDragging = false;
                        }

                        e.Handled = true;
                    }
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

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
    }

    public class AppIconDragData
    {
        public AppIconViewModel AppIcon { get; set; }
        public MenuGroupViewModel SourceGroup { get; set; }
    }
}
