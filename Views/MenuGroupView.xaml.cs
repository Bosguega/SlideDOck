using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SlideDock.Services;
using SlideDock.ViewModels;
using SlideDock.Views;
using System.Diagnostics;

namespace SlideDock.Views
{
    public partial class MenuGroupView : UserControl
    {
        private readonly IFileInteractionService _fileInteractionService;

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
                        // Certifique-se de que o AppIconView tem acesso ao MainViewModel
                        // Para mover entre grupos, precisamos do DockManagerViewModel (que está no MainViewModel)
                        MainViewModel mainViewModel = FindParent<ExpandedDockView>(this)?.DataContext as MainViewModel;
                        if (mainViewModel != null)
                        {
                            mainViewModel.DockManager.MoveAppIconBetweenGroups(dragData.AppIcon, dragData.SourceGroup, targetGroup);
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
            e.Handled = true; // Marca o evento como manipulado para evitar que ele borbulhe.
        }

        private void MenuGroup_DragEnter(object sender, DragEventArgs e)
        {
            Debug.WriteLine($"DragEnter no grupo: {(this.DataContext as MenuGroupViewModel)?.Name}");
            // A lógica principal de validação e efeitos será movida para MenuGroup_DragOver
            e.Handled = true;
        }

        private void MenuGroup_DragOver(object sender, DragEventArgs e)
        {
            Debug.WriteLine($"DragOver no grupo: {(this.DataContext as MenuGroupViewModel)?.Name}");
            if (e.Data.GetDataPresent("SlideDockAppIcon"))
            {
                Debug.WriteLine($"DragOver: SlideDockAppIcon presente. Grupo atual: {(this.DataContext as MenuGroupViewModel)?.Name}");
                AppIconDragData dragData = e.Data.GetData("SlideDockAppIcon") as AppIconDragData;
                if (dragData != null && dragData.SourceGroup != (this.DataContext as MenuGroupViewModel))
                {
                    e.Effects = DragDropEffects.Move;
                    Debug.WriteLine($"DragOver: Permitindo Move. Effects={e.Effects}");
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                    Debug.WriteLine($"DragOver: Não permitindo Move (mesmo grupo). Effects={e.Effects}");
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
                Debug.WriteLine($"DragOver: Nenhum tipo de dado reconhecido. Effects={e.Effects}");
            }
            e.Handled = true; // Essencial para que o Drop funcione
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
    }
}