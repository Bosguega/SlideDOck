using SlideDOck.Models;
using SlideDOck.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Diagnostics;

namespace SlideDOck.ViewModels
{
    public class DockManagerViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        public ObservableCollection<MenuGroupViewModel> MenuGroups { get; } = new();

        public ICommand AddMenuGroupCommand { get; }
        public ICommand RemoveMenuGroupCommand { get; }
        public ICommand AddAppFromDialogCommand { get; }

        public DockManagerViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            AddMenuGroupCommand = new RelayCommand(_ => AddNewMenuGroup());
            RemoveMenuGroupCommand = new RelayCommand(param => RemoveMenuGroup(param as MenuGroupViewModel));
            AddAppFromDialogCommand = new RelayCommand(_ => AddAppFromFileDialog());
        }

        private void AddNewMenuGroup()
        {
            var newGroup = new MenuGroup { Name = "Novo Grupo", IsExpanded = true };
            var viewModel = new MenuGroupViewModel(newGroup, _mainViewModel);
            MenuGroups.Add(viewModel);
            Debug.WriteLine("Novo grupo adicionado");
            _mainViewModel.SaveConfiguration();
        }

        public void RemoveMenuGroup(MenuGroupViewModel group)
        {
            if (group != null && MenuGroups.Contains(group))
            {
                MenuGroups.Remove(group);
                Debug.WriteLine("Grupo removido");
                _mainViewModel.SaveConfiguration();
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
                MenuGroups[0].AddAppIcon(appIcon);
                Debug.WriteLine($"App adicionado ao grupo: {appIcon.Name}");
                _mainViewModel.SaveConfiguration();
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
                MenuGroups[0].AddAppIcon(appIcon);
                Debug.WriteLine($"App adicionado via arquivo: {appIcon.Name}");
                _mainViewModel.SaveConfiguration();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}