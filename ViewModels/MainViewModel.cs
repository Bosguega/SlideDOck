// Arquivo: ViewModels\MainViewModel.cs
using SlideDock.Models;
using SlideDock.Commands;
using SlideDock.Services;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Collections.Specialized;
using System.Diagnostics;
using System;
using System.Linq; // Adicionado para possíveis usos futuros

namespace SlideDock.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private readonly ConfigurationService _configService;
        private readonly IFileInteractionService _fileInteractionService;
        private readonly IDialogService _dialogService;
        private DockPosition _dockPosition;

        public DockManagerViewModel DockManager { get; }

        public ICommand ToggleDockCommand { get; }
        public ICommand CloseApplicationCommand { get; }
        public ICommand DropFilesCommand { get; }
        public ICommand DragOverFilesCommand { get; }

        public MainViewModel()
        {
            _configService = new ConfigurationService(new SampleDataProvider());
            _fileInteractionService = new FileInteractionService();
            _dialogService = new DialogService();

            // Passa os serviços injetados para o DockManagerViewModel
            DockManager = new DockManagerViewModel(this, _fileInteractionService, _dialogService);

            ToggleDockCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            CloseApplicationCommand = new RelayCommand(_ => CloseApplication());
            DropFilesCommand = new RelayCommand(OnDropFiles);
            DragOverFilesCommand = new RelayCommand(OnDragOverFiles);
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

        public DockPosition DockPosition
        {
            get => _dockPosition;
            set
            {
                _dockPosition = value;
                OnPropertyChanged();
            }
        }

        public void ToggleDockSide()
        {
            DockPosition = (DockPosition == DockPosition.Left) ? DockPosition.Right : DockPosition.Left;
            Debug.WriteLine($"Posição da dock alterada para: {DockPosition}");
        }

        public void SaveConfiguration()
        {
            try
            {
                Debug.WriteLine("Salvando configuração...");
                var config = new DockConfiguration
                {
                    IsExpanded = this.IsExpanded,
                    DockPosition = this.DockPosition
                };

                foreach (var groupViewModel in DockManager.MenuGroups)
                {
                    var groupData = new MenuGroupData
                    {
                        Name = groupViewModel.Name,
                        IsExpanded = groupViewModel.IsExpanded
                    };

                    foreach (var appIconViewModel in groupViewModel.AppIcons)
                    {
                        // Certifique-se de salvar o ItemType também
                        groupData.AppIcons.Add(new AppIconData
                        {
                            Name = appIconViewModel.Name,
                            ExecutablePath = appIconViewModel.ExecutablePath,
                            ItemType = appIconViewModel.ItemType // Salva o tipo
                        });
                    }

                    config.MenuGroups.Add(groupData);
                }

                _configService.SaveConfiguration(config); // Salva a configuração completa
                Debug.WriteLine($"Configuração salva com {DockManager.MenuGroups.Count} grupos ");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao salvar configuração: {ex.Message} ");
                Debug.WriteLine($"StackTrace: {ex.StackTrace} ");
            }
        }


        private void LoadConfiguration()
        {
            try
            {
                Debug.WriteLine("Iniciando carregamento da configuração... ");
                var config = _configService.LoadConfiguration();
                Debug.WriteLine($"Configuração carregada com {config.MenuGroups.Count} grupos ");

                // Load IsExpanded and DockPosition
                IsExpanded = config.IsExpanded;
                DockPosition = config.DockPosition;

                DockManager.MenuGroups.Clear();

                foreach (var groupData in config.MenuGroups)
                {
                    Debug.WriteLine($"Carregando grupo: {groupData.Name} com {groupData.AppIcons.Count} aplicativos ");
                    var group = new MenuGroup
                    {
                        Name = groupData.Name,
                        IsExpanded = groupData.IsExpanded
                    };

                    foreach (var appData in groupData.AppIcons)
                    {
                        Debug.WriteLine($"Carregando app: {appData.Name}, {appData.ExecutablePath} ");
                        var appIcon = new AppIcon
                        {
                            Name = appData.Name,
                            ExecutablePath = appData.ExecutablePath,
                            ItemType = appData.ItemType // Carrega o tipo salvo
                        };
                        group.AppIcons.Add(appIcon);
                    }

                    // Passa os serviços injetados (se estiver usando a versão atualizada)
                    // var groupViewModel = new MenuGroupViewModel(group, this, _dialogService, _fileInteractionService);
                    // Para compatibilidade com o código original, assumindo que não precisa injetar serviços aqui:
                    var groupViewModel = new MenuGroupViewModel(group, this, _dialogService, _fileInteractionService);
                    DockManager.MenuGroups.Add(groupViewModel);
                    Debug.WriteLine($"Grupo '{groupData.Name}' adicionado com {groupViewModel.AppIcons.Count} aplicativos ");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar configuração: {ex.Message} ");
                Debug.WriteLine($"StackTrace: {ex.StackTrace} ");
            }
        }

        private void CloseApplication()
        {
            Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Drag and Drop Handlers

        private void OnDropFiles(object? parameter)
        {
            if (parameter is DragEventArgs e)
            {
                string[] files = _fileInteractionService.GetDroppedFiles(e);
                foreach (string file in files)
                {
                    AddAppFromFile(file);
                }
            }
        }

        private void OnDragOverFiles(object? parameter)
        {
            if (parameter is DragEventArgs e)
            {
                string[] files = _fileInteractionService.GetDroppedFiles(e);
                e.Effects = files.Length > 0 ? DragDropEffects.Copy : DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void AddAppFromFile(string filePath)
        {
            DockManager.AddAppFromFile(filePath);
        }

        #endregion
    }
}