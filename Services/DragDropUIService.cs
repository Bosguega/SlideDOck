using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SlideDock.ViewModels;
using SlideDock.Views;

namespace SlideDock.Services
{
    public class DragDropUIService : IDragDropUIService
    {
        private readonly MenuGroupView _menuGroupView;
        private FrameworkElement? _dropIndicator;

        public DragDropUIService(MenuGroupView menuGroupView)
        {
            _menuGroupView = menuGroupView ?? throw new ArgumentNullException(nameof(menuGroupView));
        }

        public int GetDropIndex(DragEventArgs e, object dataContext, DependencyObject dropTarget)
        {
            if (dataContext is not MenuGroupViewModel groupViewModel) return 0;

            var itemsControl = FindChild<ItemsControl>(_menuGroupView);
            if (itemsControl == null || itemsControl.Items.Count == 0) return 0;

            double closestDistance = double.MaxValue;
            int closestIndex = 0;
            bool insertAfter = false;

            Point dropPosition = e.GetPosition(itemsControl);

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

        public void ShowDropIndicator(DragEventArgs e, DependencyObject dropTarget)
        {
            if (_dropIndicator == null)
            {
                _dropIndicator = new Border
                {
                    Background = Brushes.Red,
                    Width = 2,
                    Height = 50,
                    Opacity = 0.7
                };
                if (_menuGroupView.Content is Panel parent) parent.Children.Add(_dropIndicator);
            }

            if (_dropIndicator is Border indicator)
            {
                Point position = e.GetPosition(_menuGroupView);
                Canvas.SetLeft(indicator, position.X);
                Canvas.SetTop(indicator, position.Y);
                indicator.Visibility = Visibility.Visible;
            }
        }

        public void HideDropIndicator()
        {
            if (_dropIndicator != null) _dropIndicator.Visibility = Visibility.Collapsed;
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
    }
}
