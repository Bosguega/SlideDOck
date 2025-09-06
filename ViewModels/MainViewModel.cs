using Microsoft.Win32;
using SlideDOck.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SlideDOck.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded;
        public ICommand ToggleDockCommand { get; }
        public ICommand AddMenuGroupCommand { get; }
        public ICommand AddAppFromDialogCommand { get; }
        public ICommand RemoveMenuGroupCommand { get; }

        public MainViewModel()
        {
            ToggleDockCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            AddMenuGroupCommand = new RelayCommand(_ => AddNewMenuGroup());
            AddAppFromDialogCommand = new RelayCommand(_ => AddAppFromFileDialog());
            RemoveMenuGroupCommand = new RelayCommand(param => RemoveMenuGroup(param as MenuGroupViewModel));
            InitializeSampleData();
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MenuGroupViewModel> MenuGroups { get; } = new ObservableCollection<MenuGroupViewModel>();

        private void AddNewMenuGroup()
        {
            var newGroup = new MenuGroup { Name = "Novo Grupo", IsExpanded = true };
            var viewModel = new MenuGroupViewModel(newGroup, this);
            MenuGroups.Add(viewModel);
        }

        public void RemoveMenuGroup(MenuGroupViewModel group)
        {
            if (group != null && MenuGroups.Contains(group))
            {
                MenuGroups.Remove(group);
            }
        }

        private void AddAppFromFileDialog()
        {
            if (MenuGroups.Count == 0)
            {
                MessageBox.Show("Adicione um grupo primeiro!", "SlideDOck", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executáveis (*.exe)|*.exe|Todos os arquivos (*.*)|*.*",
                Title = "Selecione um aplicativo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AddAppToSelectedGroup(openFileDialog.FileName);
            }
        }

        private void AddAppToSelectedGroup(string filePath)
        {
            // Para simplificar, adiciona ao primeiro grupo
            // Na prática, você pode querer mostrar um diálogo para escolher o grupo
            if (MenuGroups.Count > 0)
            {
                var appIcon = new AppIcon
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    ExecutablePath = filePath,
                    IconPath = GetIconPathForFile(filePath)
                };

                MenuGroups[0].AddAppIcon(appIcon);
            }
        }

        private string GetIconPathForFile(string filePath)
        {
            try
            {
                // Tenta extrair o ícone real do arquivo
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SlideDOck", "Icons");
                string iconPath = Utils.IconExtractor.ExtractIconToFile(filePath, appDataPath);

                if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                {
                    return iconPath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao extrair ícone: {ex.Message}");
            }

            // Retorna ícone padrão se falhar
            return "pack://application:,,,/Resources/default_app.png";
        }

        private string ExtractIconFromFile(string filePath)
        {
            try
            {
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SlideDOck", "Icons");
                return Utils.IconExtractor.ExtractIconToFile(filePath, appDataPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao extrair ícone: {ex.Message}");
                return "pack://application:,,,/Resources/default_app.png";
            }
        }

        private void InitializeSampleData()
        {
            // Exemplo de dados iniciais
            var group1 = new MenuGroup { Name = "Desenvolvimento", IsExpanded = true };
            var group2 = new MenuGroup { Name = "Utilitários", IsExpanded = false };

            var app1 = new AppIcon
            {
                Name = "Visual Studio",
                IconPath = "pack://application:,,,/Resources/vs_icon.png",
                ExecutablePath = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe"
            };

            var app2 = new AppIcon
            {
                Name = "Notepad++",
                IconPath = "pack://application:,,,/Resources/notepad_icon.png",
                ExecutablePath = @"C:\Program Files\Notepad++\notepad++.exe"
            };

            var group1ViewModel = new MenuGroupViewModel(group1, this);
            var group2ViewModel = new MenuGroupViewModel(group2, this);

            group1ViewModel.AddAppIcon(app1);
            group2ViewModel.AddAppIcon(app2);

            MenuGroups.Add(group1ViewModel);
            MenuGroups.Add(group2ViewModel);
        }

        public void AddAppFromFile(string filePath)
        {
            if (MenuGroups.Count == 0)
            {
                AddNewMenuGroup();
            }

            // Adicionar ao primeiro grupo (ou implementar lógica para escolher grupo)
            if (MenuGroups.Count > 0)
            {
                var appIcon = new AppIcon
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    ExecutablePath = filePath,
                    IconPath = GetIconPathForFile(filePath)
                };

                MenuGroups[0].AddAppIcon(appIcon);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}