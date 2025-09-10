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

        public AppIconView()
        {
            InitializeComponent();
        }

        private void AppIcon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void AppIcon_MouseMove(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("AppIcon_MouseMove acionado.");
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                AppIconViewModel appIconToDrag = this.DataContext as AppIconViewModel;
                Debug.WriteLine($"AppIconToDrag: {appIconToDrag?.Name ?? "NULL"}");
                MenuGroupViewModel sourceGroup = FindParent<MenuGroupView>(this)?.DataContext as MenuGroupViewModel;
                Debug.WriteLine($"SourceGroup: {sourceGroup?.Name ?? "NULL"}");

                if (appIconToDrag != null && sourceGroup != null)
                {
                    Debug.WriteLine($"Iniciando arrasto: App='{appIconToDrag.Name}' do grupo='{sourceGroup.Name}'");
                    DataObject dragData = new DataObject("SlideDockAppIcon", new AppIconDragData
                    {
                        AppIcon = appIconToDrag,
                        SourceGroup = sourceGroup
                    });

                    DragDrop.DoDragDrop(this, dragData, DragDropEffects.Move);
                    e.Handled = true; // Adicionado para garantir que o evento seja manipulado
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            Debug.WriteLine($"FindParent: Buscando pai do tipo {typeof(T).Name} para {child.GetType().Name}");
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
            {
                Debug.WriteLine($"FindParent: Nenhum pai encontrado para {child.GetType().Name}");
                return null;
            }

            Debug.WriteLine($"FindParent: Pai encontrado: {parentObject.GetType().Name}");
            if (parentObject is T parent)
            {
                Debug.WriteLine($"FindParent: Pai do tipo {typeof(T).Name} encontrado: {parent.GetType().Name}");
                return parent;
            }
            else
            {
                Debug.WriteLine($"FindParent: Pai {parentObject.GetType().Name} não é do tipo {typeof(T).Name}, subindo...");
                return FindParent<T>(parentObject);
            }
        }
    }

    public class AppIconDragData
    {
        public AppIconViewModel AppIcon { get; set; }
        public MenuGroupViewModel SourceGroup { get; set; }
    }
}