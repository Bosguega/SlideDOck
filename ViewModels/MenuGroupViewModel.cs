using Microsoft.Win32;
using SlideDOck.Commands;
using SlideDOck.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SlideDOck.ViewModels
{
    public class MenuGroupViewModel : INotifyPropertyChanged
    {
        private readonly MenuGroup _model;
        private readonly MainViewModel _mainViewModel;
        public ICommand ToggleExpandCommand { get; }
        public ICommand AddAppCommand { get; }
        public ICommand RemoveGroupCommand { get; }

        public MenuGroupViewModel(MenuGroup model, MainViewModel mainViewModel)
        {
            _model = model;
            _mainViewModel = mainViewModel;
            ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            AddAppCommand = new RelayCommand(_ => AddAppFromDialog());
            RemoveGroupCommand = new RelayCommand(_ => _mainViewModel.MenuGroups.Remove(this));
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

        public void AddAppIcon(AppIcon appIcon)
        {
            var appIconViewModel = new AppIconViewModel(appIcon);
            appIconViewModel.RemoveRequested += (sender, e) => RemoveApp(appIconViewModel);
            appIconViewModel.OpenFolderRequested += (sender, e) => OpenAppFolder(appIconViewModel);
            AppIcons.Add(appIconViewModel);
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

                AddAppIcon(appIcon);
            }
        }

        public void RemoveApp(AppIconViewModel appViewModel)
        {
            if (appViewModel != null && AppIcons.Contains(appViewModel))
            {
                AppIcons.Remove(appViewModel);
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}