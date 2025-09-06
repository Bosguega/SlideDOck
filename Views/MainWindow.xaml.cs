using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.Diagnostics;

namespace SlideDOck.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Left = 0;
            Top = 0;
            Height = SystemParameters.WorkArea.Height;
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
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

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
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

        private void AppIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Implementar drag de ícones existentes se necessário
            }
        }

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
            var viewModel = (ViewModels.MainViewModel)DataContext;
            if (viewModel.MenuGroups.Count == 0)
            {
                viewModel.AddMenuGroupCommand.Execute(null);
            }

            // Adicionar ao primeiro grupo (ou implementar lógica para escolher grupo)
            if (viewModel.MenuGroups.Count > 0)
            {
                var appIcon = new Models.AppIcon
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    ExecutablePath = filePath,
                    IconPath = GetIconFromFile(filePath)
                };

                viewModel.MenuGroups[0].AddAppIcon(appIcon);
            }
        }

        private string GetIconFromFile(string filePath)
        {
            // Implementar extração de ícone real
            // Por enquanto retorna ícone padrão
            return "pack://application:,,,/Resources/default_app.png";
        }
    }
}