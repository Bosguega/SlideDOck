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

        public ICommand ToggleExpandCommand { get; }
        public ICommand AddAppCommand { get; }
        public ICommand RemoveGroupCommand { get; }

        public MenuGroupViewModel(MenuGroup model,
                                 MainViewModel mainViewModel,
                                 IDialogService dialogService)
        {
            _model = model;
            _mainViewModel = mainViewModel;
            _dialogService = dialogService;

            ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            AddAppCommand = new RelayCommand(_ => AddAppFromDialog());
            RemoveGroupCommand = new RelayCommand(_ => _mainViewModel.DockManager.RemoveMenuGroup(this));

            SyncAppIconsFromModel();

            Debug.WriteLine($"MenuGroupViewModel criado para grupo '{model.Name}' com {model.AppIcons.Count} aplicativos no modelo");
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

        public ObservableCollection<AppIconViewModel> AppIcons { get; } = new ObservableCollection<AppIconViewModel>();

        private void SyncAppIconsFromModel()
        {
            Debug.WriteLine($"Sincronizando {_model.AppIcons.Count} aplicativos do modelo para o ViewModel");
            AppIcons.Clear();
            foreach (var appIcon in _model.AppIcons)
            {
                var appIconViewModel = new AppIconViewModel(appIcon);
                appIconViewModel.RemoveRequested += (sender, e) => RemoveApp(appIconViewModel);
                appIconViewModel.OpenFolderRequested += (sender, e) => OpenAppFolder(appIconViewModel);
                AppIcons.Add(appIconViewModel);
                Debug.WriteLine($"App sincronizado: {appIcon.Name}");
            }
            Debug.WriteLine($"Sincronização concluída. ViewModel agora tem {AppIcons.Count} aplicativos");
        }

        public void AddAppIcon(string filePath)
        {
            var appIcon = new AppIcon
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                ExecutablePath = filePath
            };

            // Adiciona ao modelo primeiro
            _model.AppIcons.Add(appIcon);

            // Depois cria o ViewModel e adiciona à coleção
            var appIconViewModel = new AppIconViewModel(appIcon);
            appIconViewModel.RemoveRequested += (sender, e) => RemoveApp(appIconViewModel);
            appIconViewModel.OpenFolderRequested += (sender, e) => OpenAppFolder(appIconViewModel);
            AppIcons.Add(appIconViewModel);

            Debug.WriteLine($"App adicionado ao grupo: {appIcon.Name}. Total no modelo: {_model.AppIcons.Count}, Total no ViewModel: {AppIcons.Count}");
            _mainViewModel.SaveConfiguration();
        }

        private void AddAppFromDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executáveis (*.exe)|*.exe|Todos os arquivos (*.*)|*.*",
                Title = "Selecione um aplicativo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var appIcon = new AppIcon
                {
                    Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName),
                    ExecutablePath = openFileDialog.FileName
                };

                AddAppIcon(openFileDialog.FileName);
            }
        }

        public void RemoveApp(AppIconViewModel appViewModel)
        {
            if (appViewModel != null && AppIcons.Contains(appViewModel))
            {
                string message = $"Deseja remover o aplicativo '{appViewModel.Name}'?";
                string title = "Confirmar Remoção";

                if (_dialogService.ShowConfirmationDialog(message, title))
                {
                    RemoveAppIcon(appViewModel);
                }
            }
        }

        public void RemoveAppIcon(AppIconViewModel appViewModel)
        {
            if (appViewModel == null) return;

            // Remove do ViewModel
            AppIcons.Remove(appViewModel);

            // Encontra e remove do modelo
            var modelToRemove = _model.AppIcons.FirstOrDefault(a => a.ExecutablePath == appViewModel.ExecutablePath);
            if (modelToRemove != null)
            {
                _model.AppIcons.Remove(modelToRemove);
                Debug.WriteLine($"App removido do grupo: {appViewModel.Name}. Restantes no modelo: {_model.AppIcons.Count}");
            }
            _mainViewModel.SaveConfiguration();
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}