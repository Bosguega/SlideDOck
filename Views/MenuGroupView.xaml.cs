using SlideDock.Services;
using SlideDock.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SlideDock.Views
{
    public partial class MenuGroupView : UserControl
    {
        private readonly IFileInteractionService _fileInteractionService;
        private FrameworkElement _dropIndicator;

        public MenuGroupView()
        {
            InitializeComponent();
            _fileInteractionService = new FileInteractionService();
        }

        #region Drag & Drop

        private void MenuGroup_Drop(object sender, DragEventArgs e)
        {
            if (this.DataContext is not MenuGroupViewModel targetGroup) return;

            if (e.Data.GetDataPresent("SlideDockAppIcon"))
                HandleDragBetweenGroupsOrReorder(targetGroup, e);
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                HandleExternalFileDrop(targetGroup, e);

            e.Handled = true;
            HideDropIndicator();
        }

        private void MenuGroup_DragEnter(object sender, DragEventArgs e) => e.Handled = true;

        private void MenuGroup_DragOver(object sender, DragEventArgs e)
        {
            if (this.DataContext is not MenuGroupViewModel targetGroup) return;

            if (e.Data.GetDataPresent("SlideDockAppIcon"))
                HandleDragOverAppIcon(targetGroup, e);
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                HandleDragOverExternalFiles(targetGroup, e);
            else
            {
                e.Effects = DragDropEffects.None;
                HideDropIndicator();
            }

            e.Handled = true;
        }

        private void MenuGroup_DragLeave(object sender, DragEventArgs e)
        {
            HideDropIndicator();
            e.Handled = true;
        }

        #endregion

        #region Módulos de Drag

        private void HandleDragBetweenGroupsOrReorder(MenuGroupViewModel targetGroup, DragEventArgs e)
        {
            if (e.Data.GetData("SlideDockAppIcon") is not AppIconDragData dragData) return;

            if (dragData.SourceGroup == targetGroup)
            {
                int newIndex = GetDropIndex(e.GetPosition(this));
                targetGroup.ReorderAppIcon(dragData.AppIcon, newIndex);
            }
            else
            {
                var mainVM = FindParent<ExpandedDockView>(this)?.DataContext as MainViewModel;
                mainVM?.DockManager.MoveAppIconBetweenGroups(dragData.AppIcon, dragData.SourceGroup, targetGroup);
            }
        }

        private void HandleExternalFileDrop(MenuGroupViewModel targetGroup, DragEventArgs e)
        {
            string[] files = _fileInteractionService.GetDroppedFiles(e);
            foreach (string file in files)
                targetGroup.AddAppIcon(file);
        }

        private void HandleDragOverAppIcon(MenuGroupViewModel targetGroup, DragEventArgs e)
        {
            if (e.Data.GetData("SlideDockAppIcon") is not AppIconDragData dragData) return;

            e.Effects = dragData.SourceGroup == targetGroup ? DragDropEffects.Move : DragDropEffects.Move;
            if (dragData.SourceGroup == targetGroup)
                ShowDropIndicator(e.GetPosition(this));
        }

        private void HandleDragOverExternalFiles(MenuGroupViewModel targetGroup, DragEventArgs e)
        {
            string[] files = _fileInteractionService.GetDroppedFiles(e);
            e.Effects = files.Length > 0 ? DragDropEffects.Copy : DragDropEffects.None;
        }

        #endregion

        #region Helpers

        private int GetDropIndex(Point dropPosition)
        {
            if (this.DataContext is not MenuGroupViewModel groupViewModel) return 0;

            var itemsControl = FindChild<ItemsControl>(this);
            if (itemsControl == null || itemsControl.Items.Count == 0) return 0;

            double closestDistance = double.MaxValue;
            int closestIndex = 0;
            bool insertAfter = false;

            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                if (itemsControl.ItemContainerGenerator.ContainerFromIndex(i) is not FrameworkElement container) continue;

                var containerPosition = container.TranslatePoint(new Point(0, 0), itemsControl);
                var containerBounds = new Rect(containerPosition, container.RenderSize);
                var containerCenter = new Point(containerBounds.X + containerBounds.Width / 2,
                                                containerBounds.Y + containerBounds.Height / 2);
                double distance = (dropPosition - containerCenter).Length;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                    insertAfter = dropPosition.X > containerCenter.X;
                }
            }

            return insertAfter ? Math.Min(closestIndex + 1, groupViewModel.AppIcons.Count) : closestIndex;
        }

        private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result) return result;
                var childOfChild = FindChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            return parent is T tParent ? tParent : FindParent<T>(parent);
        }

        private void ShowDropIndicator(Point position)
        {
            if (_dropIndicator == null)
            {
                _dropIndicator = new Border
                {
                    Background = System.Windows.Media.Brushes.Red,
                    Width = 2,
                    Height = 50,
                    Opacity = 0.7
                };
                if (this.Content is Panel parent) parent.Children.Add(_dropIndicator);
            }

            if (_dropIndicator is Border indicator)
            {
                Canvas.SetLeft(indicator, position.X);
                Canvas.SetTop(indicator, position.Y);
                indicator.Visibility = Visibility.Visible;
            }
        }

        private void HideDropIndicator()
        {
            if (_dropIndicator != null) _dropIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}
