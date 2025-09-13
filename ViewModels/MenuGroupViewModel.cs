using Microsoft.Win32;
using SlideDock.Commands;
using SlideDock.Models;
using SlideDock.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SlideDock.ViewModels
{
    public class MenuGroupViewModel : INotifyPropertyChanged
    {
        private readonly MenuGroup _model;
        private readonly MainViewModel _mainViewModel;
        private readonly IDialogService _dialogService;
        private readonly IFileInteractionService _fileInteractionService;

        public ObservableCollection<AppIconViewModel> AppIcons { get; } = [];

        public ICommand ToggleExpandCommand { get; }
        public ICommand AddAppCommand { get; }
        public ICommand RemoveGroupCommand { get; }
        public ICommand AddFolderCommand { get; }

        public MenuGroupViewModel(MenuGroup model,
                                  MainViewModel mainViewModel,
                                  IDialogService dialogService,
                                  IFileInteractionService fileInteractionService)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _fileInteractionService = fileInteractionService ?? throw new ArgumentNullException(nameof(fileInteractionService));

            ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            AddAppCommand = new RelayCommand(_ => AddAppFromFileDialog());
            RemoveGroupCommand = new RelayCommand(_ => _mainViewModel.DockManager.RemoveMenuGroup(this));
            AddFolderCommand = new RelayCommand(_ => AddFolderFromDialog());

            SyncAppIconsFromModel();
        }

        public string Name
        {
            get => _model.Name;
            set
            {
                _model.Name = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpanded
        {
            get => _model.IsExpanded;
            set
            {
                _model.IsExpanded = value;
                OnPropertyChanged();
            }
        }

        #region Adição de itens

        public void AddAppIcon(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            DockItemType itemType;
            string itemName;

            if (Directory.Exists(filePath))
            {
                itemType = DockItemType.Folder;
                itemName = Path.GetFileName(filePath);
            }
            else if (File.Exists(filePath))
            {
                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                itemType = extension == ".exe" ? DockItemType.Application : DockItemType.File;
                itemName = Path.GetFileNameWithoutExtension(filePath);
            }
            else
            {
                itemType = DockItemType.File;
                itemName = Path.GetFileNameWithoutExtension(filePath);
            }

            var appIcon = new AppIcon
            {
                Name = itemName,
                ExecutablePath = filePath,
                ItemType = itemType
            };

            _model.AppIcons.Add(appIcon);

            var appIconViewModel = new AppIconViewModel(appIcon, _dialogService);
            appIconViewModel.RemoveRequested += (s, e) => RemoveApp(appIconViewModel);
            appIconViewModel.OpenFolderRequested += (s, e) => OpenAppFolder(appIconViewModel);
            AppIcons.Add(appIconViewModel);

            _mainViewModel.SaveConfiguration();
        }

        private void AddAppFromFileDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Todos os arquivos (*.*)|*.*",
                Title = "Selecione um arquivo"
            };

            if (openFileDialog.ShowDialog() == true)
                AddAppIcon(openFileDialog.FileName);
        }

        private void AddFolderFromDialog()
        {
            string? folderPath = _fileInteractionService?.SelectFolder();
            if (!string.IsNullOrEmpty(folderPath))
                AddAppIcon(folderPath);
        }

        #endregion

        #region Remoção de itens

        public void RemoveApp(AppIconViewModel appViewModel)
        {
            if (appViewModel == null || !AppIcons.Contains(appViewModel)) return;

            if (_dialogService.ShowConfirmationDialog(
                $"Deseja remover o item '{appViewModel.Name}'?",
                "Confirmar Remoção"))
            {
                RemoveAppIcon(appViewModel);
            }
        }

        public void RemoveAppIcon(AppIconViewModel appViewModel)
        {
            if (appViewModel == null) return;

            AppIcons.Remove(appViewModel);

            var modelToRemove = _model.AppIcons.FirstOrDefault(a => a.ExecutablePath == appViewModel.ExecutablePath);
            if (modelToRemove != null)
                _model.AppIcons.Remove(modelToRemove);

            _mainViewModel.SaveConfiguration();
        }

        #endregion

        #region Ações nos itens

        private void OpenAppFolder(AppIconViewModel appViewModel)
        {
            if (appViewModel == null || string.IsNullOrEmpty(appViewModel.ExecutablePath))
                return;

            try
            {
                string? pathToOpen = null;

                if (appViewModel.ItemType == DockItemType.Folder && Directory.Exists(appViewModel.ExecutablePath))
                    pathToOpen = appViewModel.ExecutablePath;
                else if ((appViewModel.ItemType == DockItemType.Application || appViewModel.ItemType == DockItemType.File) &&
                         File.Exists(appViewModel.ExecutablePath))
                    pathToOpen = Path.GetDirectoryName(appViewModel.ExecutablePath);

                if (!string.IsNullOrEmpty(pathToOpen))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = pathToOpen,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir local: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Reordenação

        public void ReorderAppIcon(AppIconViewModel appIcon, int newIndex)
        {
            if (appIcon == null || !AppIcons.Contains(appIcon)) return;

            int currentIndex = AppIcons.IndexOf(appIcon);
            if (currentIndex == newIndex) return;

            newIndex = Math.Max(0, Math.Min(newIndex, AppIcons.Count - 1));
            AppIcons.RemoveAt(currentIndex);
            AppIcons.Insert(newIndex, appIcon);

            SyncModelFromViewModel();
            _mainViewModel.SaveConfiguration();
        }

        private void SyncModelFromViewModel()
        {
            _model.AppIcons.Clear();
            foreach (var appIconViewModel in AppIcons)
            {
                _model.AppIcons.Add(new AppIcon
                {
                    Name = appIconViewModel.Name,
                    ExecutablePath = appIconViewModel.ExecutablePath,
                    ItemType = appIconViewModel.ItemType
                });
            }
        }

        #endregion

        #region Inicialização

        private void SyncAppIconsFromModel()
        {
            AppIcons.Clear();
            foreach (var appIcon in _model.AppIcons)
            {
                var appIconViewModel = new AppIconViewModel(appIcon, _dialogService);
                appIconViewModel.RemoveRequested += (s, e) => RemoveApp(appIconViewModel);
                appIconViewModel.OpenFolderRequested += (s, e) => OpenAppFolder(appIconViewModel);
                AppIcons.Add(appIconViewModel);
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
