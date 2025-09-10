using SlideDock.Models;
using SlideDock.Commands;
using SlideDock.Services;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System;

namespace SlideDock.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private readonly ConfigurationService _configService;
        private readonly IFileInteractionService _fileInteractionService;
        private readonly IDialogService _dialogService;

        public DockManagerViewModel DockManager { get; }

        public ICommand ToggleDockCommand { get; }
        public ICommand CloseApplicationCommand { get; }

        public MainViewModel()
        {
            _configService = new ConfigurationService(new SampleDataProvider());
            _fileInteractionService = new FileInteractionService();
            _dialogService = new DialogService();

            DockManager = new DockManagerViewModel(this, _fileInteractionService, _dialogService);

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

                    var groupViewModel = new MenuGroupViewModel(group, this, _dialogService);
                    DockManager.MenuGroups.Add(groupViewModel);
                    Debug.WriteLine($"Grupo '{groupData.Name}' adicionado com {groupViewModel.AppIcons.Count} aplicativos");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar configuração: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
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