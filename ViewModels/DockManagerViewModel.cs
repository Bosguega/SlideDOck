// Arquivo: ViewModels\DockManagerViewModel.cs
using SlideDock.Models;
using SlideDock.Commands;
using SlideDock.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Linq; // Adicionado para FirstOrDefault

namespace SlideDock.ViewModels
{
    public class DockManagerViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        // Serviço já injetado
        private readonly IFileInteractionService _fileInteractionService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<MenuGroupViewModel> MenuGroups { get; } = [];

        public ICommand AddMenuGroupCommand { get; }
        public ICommand RemoveMenuGroupCommand { get; }
        public ICommand AddAppFromDialogCommand { get; }
        public ICommand ToggleDockSideCommand { get; }

        public DockManagerViewModel(MainViewModel mainViewModel,
                                   IFileInteractionService fileInteractionService,
                                   IDialogService dialogService)
        {
            _mainViewModel = mainViewModel;
            // Atribuir os serviços injetados
            _fileInteractionService = fileInteractionService;
            _dialogService = dialogService;

            AddMenuGroupCommand = new RelayCommand(_ => AddNewMenuGroup());
            RemoveMenuGroupCommand = new RelayCommand(param => RemoveMenuGroup(param as MenuGroupViewModel));
            AddAppFromDialogCommand = new RelayCommand(_ => AddAppFromDialog());
            ToggleDockSideCommand = new RelayCommand(_ => _mainViewModel.ToggleDockSide());
        }

        private void AddNewMenuGroup()
        {
            var newGroup = new MenuGroup { Name = "Novo Grupo", IsExpanded = true };
            // Passar os serviços injetados para o novo MenuGroupViewModel
            var viewModel = new MenuGroupViewModel(newGroup, _mainViewModel, _dialogService, _fileInteractionService);
            MenuGroups.Add(viewModel);
            Debug.WriteLine("Novo grupo adicionado");
            _mainViewModel.SaveConfiguration();
        }

        public void RemoveMenuGroup(MenuGroupViewModel? group)
        {
            if (group != null && MenuGroups.Contains(group))
            {
                string message = $"Deseja remover o grupo '{group.Name}' e todos os seus itens?";
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
                MessageBox.Show("Adicione um grupo primeiro!", "SlideDock", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Usa o serviço injetado para selecionar o arquivo
            string filePath = _fileInteractionService.SelectExecutableFile();
            if (!string.IsNullOrEmpty(filePath))
            {
                AddAppToSelectedGroup(filePath);
            }
        }

        private async void AddAppToSelectedGroup(string filePath)
        {
            if (MenuGroups.Count > 0)
            {
                await MenuGroups[0].AddAppIconAsync(filePath); // Corrigido para usar o método assíncrono
                Debug.WriteLine($"Item adicionado ao grupo via diálogo: {Path.GetFileName(filePath)}");
                _mainViewModel.SaveConfiguration();
            }
        }

        // Método chamado ao arrastar arquivos/pastas para a janela principal
        public async void AddAppFromFile(string filePath)
        {
            if (MenuGroups.Count == 0)
            {
                AddNewMenuGroup();
            }

            if (MenuGroups.Count > 0)
            {
                await MenuGroups[0].AddAppIconAsync(filePath); // Corrigido para usar o método assíncrono
                Debug.WriteLine($"Item adicionado via arquivo/drag: {Path.GetFileName(filePath)}");
                _mainViewModel.SaveConfiguration();
            }
        }

        // Método para mover ícones entre grupos via drag & drop
        public async void MoveAppIconBetweenGroups(AppIconViewModel appIcon, MenuGroupViewModel sourceGroup, MenuGroupViewModel targetGroup)
        {
            if (appIcon == null || sourceGroup == null || targetGroup == null || sourceGroup == targetGroup) return;

            Debug.WriteLine($"Movendo App '{appIcon.Name}' de '{sourceGroup.Name}' para '{targetGroup.Name}'");

            sourceGroup.RemoveAppIcon(appIcon);

            await targetGroup.AddAppIconAsync(appIcon.ExecutablePath); // Corrigido para usar o método assíncrono

            _mainViewModel.SaveConfiguration();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}