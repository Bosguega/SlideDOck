using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Linq;
using System.Windows.Media;
using SlideDock.Services;

namespace SlideDock.Views
{
    public partial class ExpandedDockView : UserControl
    {
        private readonly IFileInteractionService _fileInteractionService;

        public ExpandedDockView()
        {
            InitializeComponent();
            _fileInteractionService = new FileInteractionService(); // Injeção manual
        }

        public void MainWindow_Drop(object sender, DragEventArgs e)
        {
            string[] files = _fileInteractionService.GetDroppedFiles(e);
            foreach (string file in files)
            {
                AddAppFromFile(file);
            }
        }

        public void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            string[] files = _fileInteractionService.GetDroppedFiles(e);
            e.Effects = files.Length > 0 ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void AddAppFromFile(string filePath)
        {
            if (this.DataContext is ViewModels.MainViewModel mainViewModel)
            {
                mainViewModel.DockManager.AddAppFromFile(filePath);
            }
        }
    }
}