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
using System.Threading.Tasks;
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
        private IDragDropUIService? _dragDropUIService;

        public ObservableCollection<AppIconViewModel> AppIcons { get; } = new();

        public ICommand ToggleExpandCommand { get; }
        public ICommand AddAppCommand { get; }
        public ICommand RemoveGroupCommand { get; }
        public ICommand AddFolderCommand { get; }
        public ICommand DropCommand { get; }
        public ICommand DragOverCommand { get; }

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
            AddAppCommand = new AsyncRelayCommand(AddAppFromFileDialogAsync);
            RemoveGroupCommand = new RelayCommand(_ => _mainViewModel.DockManager.RemoveMenuGroup(this));
            AddFolderCommand = new AsyncRelayCommand(AddFolderFromDialogAsync);
            DropCommand = new RelayCommand(OnDrop);
            DragOverCommand = new RelayCommand(OnDragOver);

            SyncAppIconsFromModel();
        }

        #region Properties

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

        #endregion

        #region Initialization

        private void SyncAppIconsFromModel()
        {
            foreach (var vm in AppIcons)
            {
                vm.RemoveRequested -= (s, e) => RemoveApp(vm);
                vm.OpenFolderRequested -= async (s, e) => await OpenAppFolderAsync(vm);
            }

            AppIcons.Clear();

            foreach (var appIcon in _model.AppIcons)
            {
                var vm = new AppIconViewModel(appIcon, _dialogService, this);
                vm.RemoveRequested += (s, e) => RemoveApp(vm);
                vm.OpenFolderRequested += async (s, e) => await OpenAppFolderAsync(vm);
                AppIcons.Add(vm);
            }
        }

        #endregion

        #region Add / Remove Items

        public async Task AddAppIconAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            DockItemType type;
            string name;

            if (Directory.Exists(filePath))
            {
                type = DockItemType.Folder;
                name = Path.GetFileName(filePath);
            }
            else
            {
                type = Path.GetExtension(filePath).ToLowerInvariant() == ".exe"
                    ? DockItemType.Application
                    : DockItemType.File;
                name = Path.GetFileNameWithoutExtension(filePath);
            }

            var appIcon = new AppIcon
            {
                Name = name,
                ExecutablePath = filePath,
                ItemType = type
            };

            _model.AppIcons.Add(appIcon);

            var vm = new AppIconViewModel(appIcon, _dialogService, this);
            vm.RemoveRequested += (s, e) => RemoveApp(vm);
            vm.OpenFolderRequested += async (s, e) => await OpenAppFolderAsync(vm);
            AppIcons.Add(vm);

            _mainViewModel.SaveConfiguration();
        }

        private async Task AddAppFromFileDialogAsync()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Todos os arquivos (*.*)|*.*",
                Title = "Selecione um arquivo"
            };

            if (dialog.ShowDialog() == true)
                await AddAppIconAsync(dialog.FileName);
        }

        private async Task AddFolderFromDialogAsync()
        {
            string? folder = _fileInteractionService.SelectFolder();
            if (!string.IsNullOrEmpty(folder))
                await AddAppIconAsync(folder);
        }

        public void RemoveApp(AppIconViewModel vm)
        {
            if (vm == null || !AppIcons.Contains(vm)) return;

            if (_dialogService.ShowConfirmationDialog(
                $"Deseja remover o item '{vm.Name}'?",
                "Confirmar Remoção"))
            {
                RemoveAppIcon(vm);
            }
        }

        public void RemoveAppIcon(AppIconViewModel vm)
        {
            if (vm == null) return;

            AppIcons.Remove(vm);

            var modelItem = _model.AppIcons.FirstOrDefault(a => a.ExecutablePath == vm.ExecutablePath);
            if (modelItem != null)
                _model.AppIcons.Remove(modelItem);

            _mainViewModel.SaveConfiguration();
        }

        #endregion

        #region Item Actions

        public async Task OpenAppFolderAsync(AppIconViewModel vm)
        {
            if (vm == null || string.IsNullOrEmpty(vm.ExecutablePath)) return;

            await Task.Run(() =>
            {
                try
                {
                    string? path = null;
                    if (vm.ItemType == DockItemType.Folder && Directory.Exists(vm.ExecutablePath))
                        path = vm.ExecutablePath;
                    else if ((vm.ItemType == DockItemType.Application || vm.ItemType == DockItemType.File) &&
                             File.Exists(vm.ExecutablePath))
                        path = Path.GetDirectoryName(vm.ExecutablePath);

                    if (!string.IsNullOrEmpty(path))
                        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                        MessageBox.Show($"Erro ao abrir a pasta: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error));
                }
            });
        }

        #endregion

        #region Reorder

        public void ReorderAppIcon(AppIconViewModel vm, int newIndex)
        {
            if (vm == null || !AppIcons.Contains(vm)) return;

            int currentIndex = AppIcons.IndexOf(vm);
            if (currentIndex == newIndex) return;

            newIndex = Math.Clamp(newIndex, 0, AppIcons.Count - 1);

            AppIcons.RemoveAt(currentIndex);
            AppIcons.Insert(newIndex, vm);

            SyncModelFromViewModel();
            _mainViewModel.SaveConfiguration();
        }

        private void SyncModelFromViewModel()
        {
            _model.AppIcons.Clear();
            foreach (var vm in AppIcons)
            {
                _model.AppIcons.Add(new AppIcon
                {
                    Name = vm.Name,
                    ExecutablePath = vm.ExecutablePath,
                    ItemType = vm.ItemType
                });
            }
        }

        #endregion

        #region Drag & Drop

        private void OnDrop(object parameter)
        {
            if (parameter is not DragEventArgs e) return;

            if (e.Data.GetDataPresent("SlideDockAppIcon"))
                HandleDragBetweenGroupsOrReorder(e);
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                HandleExternalFileDropAsync(e).ConfigureAwait(false);

            _dragDropUIService?.HideDropIndicator();
        }

        private void OnDragOver(object parameter)
        {
            if (parameter is not DragEventArgs e) return;

            if (e.Data.GetDataPresent("SlideDockAppIcon"))
            {
                e.Effects = DragDropEffects.Move;
                _dragDropUIService?.ShowDropIndicator(e, e.Source as DependencyObject);
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                HandleDragOverExternalFiles(e);
                _dragDropUIService?.HideDropIndicator();
            }
            else
            {
                e.Effects = DragDropEffects.None;
                _dragDropUIService?.HideDropIndicator();
            }
        }

        private void HandleDragBetweenGroupsOrReorder(DragEventArgs e)
        {
            if (e.Data.GetData("SlideDockAppIcon") is not AppIconDragData dragData) return;

            if (dragData.SourceGroup == this)
            {
                int index = _dragDropUIService?.GetDropIndex(e, this, e.Source as DependencyObject) ?? -1;
                ReorderAppIcon(dragData.AppIcon, index);
            }
            else
            {
                _mainViewModel.DockManager.MoveAppIconBetweenGroups(dragData.AppIcon, dragData.SourceGroup, this);
            }
        }

        private async Task HandleExternalFileDropAsync(DragEventArgs e)
        {
            foreach (string file in _fileInteractionService.GetDroppedFiles(e))
                await AddAppIconAsync(file);
        }

        private void HandleDragOverExternalFiles(DragEventArgs e)
        {
            string[] files = _fileInteractionService.GetDroppedFiles(e);
            e.Effects = files.Length > 0 ? DragDropEffects.Copy : DragDropEffects.None;
        }

        public void SetDragDropUIService(IDragDropUIService service)
        {
            _dragDropUIService = service ?? throw new ArgumentNullException(nameof(service));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
