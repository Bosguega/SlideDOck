using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace SlideDOck.Views
{
    public partial class ExpandedDockView : UserControl
    {
        public ExpandedDockView()
        {
            InitializeComponent();
        }

        // --- Changed access modifier to public ---
        public void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".exe")
                    {
                        AddAppFromFile(file);
                    }
                }
            }
        }

        // --- Changed access modifier to public ---
        public void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Any(f => Path.GetExtension(f).ToLower() == ".exe"))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        // --- End of changes ---

        private void AppIcons_Drop(object sender, DragEventArgs e)
        {
            MainWindow_Drop(sender, e);
        }

        private void AppIcons_DragEnter(object sender, DragEventArgs e)
        {
            MainWindow_DragEnter(sender, e);
        }

        private void AddAppFromFile(string filePath)
        {
            if (this.DataContext is ViewModels.MainViewModel mainViewModel)
            {
                mainViewModel.DockManager.AddAppFromFile(filePath);
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