using SlideDOck.Models;
using SlideDOck.Commands;
using SlideDOck.Services;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System;

namespace SlideDOck.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private readonly ConfigurationService _configService;

        // Propriedade para o novo gerenciador
        public DockManagerViewModel DockManager { get; }

        // Comandos que permanecem no MainViewModel
        public ICommand ToggleDockCommand { get; }
        public ICommand CloseApplicationCommand { get; }

        public MainViewModel()
        {
            _configService = new ConfigurationService();
            DockManager = new DockManagerViewModel(this); // Inicializa o DockManager

            ToggleDockCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            CloseApplicationCommand = new RelayCommand(_ => CloseApplication());
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

        public void SaveConfiguration()
        {
            try
            {
                Debug.WriteLine("Salvando configuração...");
                _configService.SaveMenuGroups(DockManager.MenuGroups);
                Debug.WriteLine($"Configuração salva com {DockManager.MenuGroups.Count} grupos");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao salvar configuração: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                Debug.WriteLine("Iniciando carregamento da configuração...");
                var config = _configService.LoadConfiguration();
                Debug.WriteLine($"Configuração carregada com {config.MenuGroups.Count} grupos");

                DockManager.MenuGroups.Clear();

                foreach (var groupData in config.MenuGroups)
                {
                    Debug.WriteLine($"Carregando grupo: {groupData.Name} com {groupData.AppIcons.Count} aplicativos");
                    var group = new MenuGroup
                    {
                        Name = groupData.Name,
                        IsExpanded = groupData.IsExpanded
                    };

                    foreach (var appData in groupData.AppIcons)
                    {
                        Debug.WriteLine($"Carregando app: {appData.Name}, {appData.ExecutablePath}");
                        var appIcon = new AppIcon
                        {
                            Name = appData.Name,
                            ExecutablePath = appData.ExecutablePath
                        };
                        group.AppIcons.Add(appIcon);
                    }

                    var groupViewModel = new MenuGroupViewModel(group, this);
                    DockManager.MenuGroups.Add(groupViewModel);
                    Debug.WriteLine($"Grupo '{groupData.Name}' adicionado com {groupViewModel.AppIcons.Count} aplicativos");
                }

                if (DockManager.MenuGroups.Count == 0)
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

        private void InitializeSampleData()
        {
            var group1 = new MenuGroup { Name = "Desenvolvimento", IsExpanded = true };
            var group2 = new MenuGroup { Name = "Utilitários", IsExpanded = false };
            var group1ViewModel = new MenuGroupViewModel(group1, this);
            var group2ViewModel = new MenuGroupViewModel(group2, this);
            DockManager.MenuGroups.Add(group1ViewModel);
            DockManager.MenuGroups.Add(group2ViewModel);
            SaveConfiguration();
        }

        private void CloseApplication()
        {
            Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}