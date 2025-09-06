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

            // Adiciona handler para o evento de fechamento
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Salva a configuração antes de fechar
            if (DataContext is ViewModels.MainViewModel mainViewModel)
            {
                System.Diagnostics.Debug.WriteLine("Salvando configuração ao fechar janela...");
                // O salvamento já é feito automaticamente nos métodos
            }
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
            if (viewModel != null)
            {
                viewModel.AddAppFromFile(filePath);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Salva antes de fechar
            System.Diagnostics.Debug.WriteLine("Botão fechar clicado, salvando...");
            Close();
        }
    }
}