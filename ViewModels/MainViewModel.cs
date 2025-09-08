using SlideDOck.Models;
using SlideDOck.Commands;
using SlideDOck.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System;
using System.Windows.Media.Imaging;
using SlideDOck.Utils;
using System.Diagnostics;
using System.Linq;

namespace SlideDOck.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private readonly ConfigurationService _configService;
        public ICommand ToggleDockCommand { get; }
        public ICommand AddMenuGroupCommand { get; }
        public ICommand AddAppFromDialogCommand { get; }
        public ICommand RemoveMenuGroupCommand { get; }
        public ICommand RemoveAppCommand { get; }
        public ICommand OpenAppFolderCommand { get; }
        // --- Added for refactoring ---
        public ICommand CloseApplicationCommand { get; }
        // -----------------------------

        public MainViewModel()
        {
            _configService = new ConfigurationService();
            ToggleDockCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            AddMenuGroupCommand = new RelayCommand(_ => AddNewMenuGroup());
            AddAppFromDialogCommand = new RelayCommand(_ => AddAppFromFileDialog());
            RemoveMenuGroupCommand = new RelayCommand(param => RemoveMenuGroup(param as MenuGroupViewModel));
            RemoveAppCommand = new RelayCommand(param => RemoveApp(param as AppIconViewModel));
            OpenAppFolderCommand = new RelayCommand(param => OpenAppFolder(param as AppIconViewModel));
            // --- Added for refactoring ---
            CloseApplicationCommand = new RelayCommand(_ => CloseApplication());
            // -----------------------------
            LoadConfiguration();
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
            Debug.WriteLine("Novo grupo adicionado");
            SaveConfiguration(); // Salva imediatamente
        }

        public void RemoveMenuGroup(MenuGroupViewModel group)
        {
            if (group != null && MenuGroups.Contains(group))
            {
                MenuGroups.Remove(group);
                Debug.WriteLine("Grupo removido");
                SaveConfiguration(); // Salva imediatamente
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
            if (MenuGroups.Count > 0)
            {
                var appIcon = new AppIcon
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    ExecutablePath = filePath
                };
                // Adiciona ao primeiro grupo usando o método AddAppIcon do ViewModel
                MenuGroups[0].AddAppIcon(appIcon);
                Debug.WriteLine($"App adicionado ao grupo: {appIcon.Name}");
                SaveConfiguration(); // Salva imediatamente
            }
        }

        private void RemoveApp(AppIconViewModel appViewModel)
        {
            if (appViewModel != null)
            {
                foreach (var group in MenuGroups)
                {
                    if (group.AppIcons.Contains(appViewModel))
                    {
                        group.RemoveApp(appViewModel);
                        Debug.WriteLine("App removido");
                        // SaveConfiguration já é chamado dentro de RemoveApp
                        break;
                    }
                }
            }
        }

        private void OpenAppFolder(AppIconViewModel appViewModel)
        {
            if (appViewModel != null && !string.IsNullOrEmpty(appViewModel.ExecutablePath))
            {
                try
                {
                    string folderPath = Path.GetDirectoryName(appViewModel.ExecutablePath);
                    if (Directory.Exists(folderPath))
                    {
                        Process.Start("explorer.exe", folderPath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir pasta: {ex.Message}");
                }
            }
        }

        public void AddAppFromFile(string filePath)
        {
            if (MenuGroups.Count == 0)
            {
                AddNewMenuGroup();
            }
            if (MenuGroups.Count > 0)
            {
                var appIcon = new AppIcon
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    ExecutablePath = filePath
                };
                // Adiciona ao primeiro grupo usando o método AddAppIcon do ViewModel
                MenuGroups[0].AddAppIcon(appIcon);
                Debug.WriteLine($"App adicionado via arquivo: {appIcon.Name}");
                SaveConfiguration(); // Salva imediatamente
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                Debug.WriteLine("Iniciando carregamento da configuração...");
                var config = _configService.LoadConfiguration();
                Debug.WriteLine($"Configuração carregada com {config.MenuGroups.Count} grupos");
                // Limpa os grupos existentes
                MenuGroups.Clear();
                // Carrega os grupos salvos
                foreach (var groupData in config.MenuGroups)
                {
                    Debug.WriteLine($"Carregando grupo: {groupData.Name} com {groupData.AppIcons.Count} aplicativos");
                    var group = new MenuGroup
                    {
                        Name = groupData.Name,
                        IsExpanded = groupData.IsExpanded
                    };
                    // Carrega os apps do grupo
                    foreach (var appData in groupData.AppIcons)
                    {
                        Debug.WriteLine($"Carregando app: {appData.Name}, {appData.ExecutablePath}");
                        var appIcon = new AppIcon
                        {
                            Name = appData.Name,
                            ExecutablePath = appData.ExecutablePath
                        };
                        // Adiciona diretamente ao modelo
                        group.AppIcons.Add(appIcon);
                    }
                    // Cria o ViewModel após o modelo estar completamente preenchido
                    var groupViewModel = new MenuGroupViewModel(group, this);
                    MenuGroups.Add(groupViewModel);
                    Debug.WriteLine($"Grupo '{groupData.Name}' adicionado com {groupViewModel.AppIcons.Count} aplicativos no ViewModel");
                }
                // Se não houver grupos, carrega os dados de exemplo
                if (MenuGroups.Count == 0)
                {
                    Debug.WriteLine("Nenhum grupo encontrado, inicializando com dados de exemplo");
                    InitializeSampleData();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar configuração: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                InitializeSampleData();
            }
        }

        public void SaveConfiguration()
        {
            try
            {
                Debug.WriteLine("Salvando configuração...");
                _configService.SaveMenuGroups(MenuGroups);
                Debug.WriteLine($"Configuração salva com {MenuGroups.Count} grupos");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao salvar configuração: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        private void InitializeSampleData()
        {
            // Exemplo de dados iniciais
            var group1 = new MenuGroup { Name = "Desenvolvimento", IsExpanded = true };
            var group2 = new MenuGroup { Name = "Utilitários", IsExpanded = false };
            var group1ViewModel = new MenuGroupViewModel(group1, this);
            var group2ViewModel = new MenuGroupViewModel(group2, this);
            MenuGroups.Add(group1ViewModel);
            MenuGroups.Add(group2ViewModel);
            // Salva os dados iniciais
            SaveConfiguration();
        }

        // --- Added for refactoring ---
        private void CloseApplication()
        {
            // Get the current application instance and shut it down
            Application.Current.Shutdown();
        }
        // -----------------------------

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}