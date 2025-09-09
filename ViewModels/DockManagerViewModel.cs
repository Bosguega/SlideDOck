using SlideDOck.Models;
using SlideDOck.Commands;
using SlideDOck.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System.IO;

namespace SlideDOck.ViewModels
{
    public class DockManagerViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IFileInteractionService _fileInteractionService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<MenuGroupViewModel> MenuGroups { get; } = new();

        public ICommand AddMenuGroupCommand { get; }
        public ICommand RemoveMenuGroupCommand { get; }
        public ICommand AddAppFromDialogCommand { get; }

        public DockManagerViewModel(MainViewModel mainViewModel,
                                   IFileInteractionService fileInteractionService,
                                   IDialogService dialogService)
        {
            _mainViewModel = mainViewModel;
            _fileInteractionService = fileInteractionService;
            _dialogService = dialogService;

            AddMenuGroupCommand = new RelayCommand(_ => AddNewMenuGroup());
            RemoveMenuGroupCommand = new RelayCommand(param => RemoveMenuGroup(param as MenuGroupViewModel));
            AddAppFromDialogCommand = new RelayCommand(_ => AddAppFromDialog());
        }

        private void AddNewMenuGroup()
        {
            var newGroup = new MenuGroup { Name = "Novo Grupo", IsExpanded = true };
            var viewModel = new MenuGroupViewModel(newGroup, _mainViewModel, _dialogService);
            MenuGroups.Add(viewModel);
            Debug.WriteLine("Novo grupo adicionado");
            _mainViewModel.SaveConfiguration();
        }
        public void RemoveMenuGroup(MenuGroupViewModel group)
        {
            if (group != null && MenuGroups.Contains(group))
            {
                string message = $"Deseja remover o grupo '{group.Name}' e todos os seus aplicativos?";
                string title = "Confirmar Remoção";

                if (_dialogService.ShowConfirmationDialog(message, title))
                {
                    MenuGroups.Remove(group);
                    Debug.WriteLine($"Grupo removido: {group.Name}");
                    _mainViewModel.SaveConfiguration();
                }
            }
        }
        private void AddAppFromDialog()
        {
            if (MenuGroups.Count == 0)
            {
                MessageBox.Show("Adicione um grupo primeiro!", "SlideDOck", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string filePath = _fileInteractionService.SelectExecutableFile();
            if (!string.IsNullOrEmpty(filePath))
            {
                AddAppToSelectedGroup(filePath);
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