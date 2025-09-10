using SlideDock.Services;
using SlideDock.ViewModels;
using SlideDock.Views;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void MenuGroup_Drop(object sender, DragEventArgs e)
        {
            Debug.WriteLine($"Drop no grupo: {(this.DataContext as MenuGroupViewModel)?.Name}");
            if (this.DataContext is MenuGroupViewModel targetGroup)
            {
                if (e.Data.GetDataPresent("SlideDockAppIcon"))
                {
                    Debug.WriteLine($"Drop de SlideDockAppIcon detectado no grupo: {targetGroup.Name}");
                    AppIconDragData dragData = e.Data.GetData("SlideDockAppIcon") as AppIconDragData;
                    if (dragData != null && dragData.AppIcon != null && dragData.SourceGroup != null)
                    {
                        Debug.WriteLine($"AppIcon: {dragData.AppIcon.Name}, Origem: {dragData.SourceGroup.Name}, Destino: {targetGroup.Name}");

                        if (dragData.SourceGroup == targetGroup)
                        {
                            // Reordering within the same group
                            int newIndex = GetDropIndex(e.GetPosition(this));
                            targetGroup.ReorderAppIcon(dragData.AppIcon, newIndex);
                            Debug.WriteLine($"Reordenando dentro do mesmo grupo. Nova posição: {newIndex}");
                        }
                        else
                        {
                            // Moving between different groups
                            MainViewModel mainViewModel = FindParent<ExpandedDockView>(this)?.DataContext as MainViewModel;
                            if (mainViewModel != null)
                            {
                                mainViewModel.DockManager.MoveAppIconBetweenGroups(dragData.AppIcon, dragData.SourceGroup, targetGroup);
                            }
                        }
                    }
                }
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    Debug.WriteLine($"Drop de arquivo externo detectado no grupo: {targetGroup.Name}");
                    string[] files = _fileInteractionService.GetDroppedFiles(e);
                    foreach (string file in files)
                    {
                        targetGroup.AddAppIcon(file);
                    }
                }
            }
            e.Handled = true;
            HideDropIndicator();
        }

        private void MenuGroup_DragEnter(object sender, DragEventArgs e)
        {
            Debug.WriteLine($"DragEnter no grupo: {(this.DataContext as MenuGroupViewModel)?.Name}");
            e.Handled = true;
        }

        private void MenuGroup_DragOver(object sender, DragEventArgs e)
        {
            Debug.WriteLine($"DragOver no grupo: {(this.DataContext as MenuGroupViewModel)?.Name}");
            if (e.Data.GetDataPresent("SlideDockAppIcon"))
            {
                Debug.WriteLine($"DragOver: SlideDockAppIcon presente. Grupo atual: {(this.DataContext as MenuGroupViewModel)?.Name}");
                AppIconDragData dragData = e.Data.GetData("SlideDockAppIcon") as AppIconDragData;
                if (dragData != null)
                {
                    if (dragData.SourceGroup == (this.DataContext as MenuGroupViewModel))
                    {
                        e.Effects = DragDropEffects.Move;
                        ShowDropIndicator(e.GetPosition(this));
                        Debug.WriteLine($"DragOver: Permitindo reordenação no mesmo grupo. Effects={e.Effects}");
                    }
                    else if (dragData.SourceGroup != (this.DataContext as MenuGroupViewModel))
                    {
                        e.Effects = DragDropEffects.Move;
                        Debug.WriteLine($"DragOver: Permitindo Move entre grupos. Effects={e.Effects}");
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                        Debug.WriteLine($"DragOver: Não permitindo Move. Effects={e.Effects}");
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Debug.WriteLine($"DragOver: FileDrop presente. Grupo atual: {(this.DataContext as MenuGroupViewModel)?.Name}");
                string[] files = _fileInteractionService.GetDroppedFiles(e);
                e.Effects = files.Length > 0 ? DragDropEffects.Copy : DragDropEffects.None;
                Debug.WriteLine($"DragOver: Permitindo Copy (arquivo externo). Effects={e.Effects}");
            }
            else
            {
                e.Effects = DragDropEffects.None;
                HideDropIndicator();
                Debug.WriteLine($"DragOver: Nenhum tipo de dado reconhecido. Effects={e.Effects}");
            }
            e.Handled = true;
        }

        private void MenuGroup_DragLeave(object sender, DragEventArgs e)
        {
            HideDropIndicator();
            e.Handled = true;
        }

        private int GetDropIndex(Point dropPosition)
        {
            if (this.DataContext is MenuGroupViewModel groupViewModel)
            {
                var itemsControl = FindChild<ItemsControl>(this);
                if (itemsControl != null && itemsControl.Items.Count > 0)
                {
                    // Find the closest item based on actual visual positions
                    double closestDistance = double.MaxValue;
                    int closestIndex = 0;
                    bool insertAfter = false;

                    for (int i = 0; i < itemsControl.Items.Count; i++)
                    {
                        var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                        if (container != null)
                        {
                            // Get the actual position and size of the container
                            var containerPosition = container.TranslatePoint(new Point(0, 0), itemsControl);
                            var containerBounds = new Rect(containerPosition, container.RenderSize);

                            // Calculate distance from drop point to container center
                            var containerCenter = new Point(
                                containerBounds.X + containerBounds.Width / 2,
                                containerBounds.Y + containerBounds.Height / 2
                            );

                            double distance = Math.Sqrt(
                                Math.Pow(dropPosition.X - containerCenter.X, 2) +
                                Math.Pow(dropPosition.Y - containerCenter.Y, 2)
                            );

                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestIndex = i;

                                // Determine if we should insert before or after this item
                                // Check if drop point is in the right half of the item
                                insertAfter = dropPosition.X > containerCenter.X;
                            }
                        }
                    }

                    // Return the appropriate index
                    return insertAfter ? Math.Min(closestIndex + 1, groupViewModel.AppIcons.Count) : closestIndex;
                }
            }
            return 0;
        }

        // Helper para encontrar um filho visual de um determinado tipo
        private static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var childOfChild = FindChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        // Helper para encontrar um pai visual de um determinado tipo
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private void ShowDropIndicator(Point position)
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

                // Add to a Canvas or Grid for positioning
                var parent = this.Content as Panel;
                if (parent != null)
                {
                    parent.Children.Add(_dropIndicator);
                }
            }

            // Position the indicator
            if (_dropIndicator is Border indicator)
            {
                Canvas.SetLeft(indicator, position.X);
                Canvas.SetTop(indicator, position.Y);
                indicator.Visibility = Visibility.Visible;
            }
        }

        private void HideDropIndicator()
        {
            if (_dropIndicator != null)
            {
                _dropIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}
