// Arquivo: ViewModels\MenuGroupViewModel.cs
using Microsoft.Win32; // Para OpenFileDialog
using SlideDock.Commands;
using SlideDock.Models; // Para MenuGroup, DockItemType
using SlideDock.Services; // Para IDialogService, IFileInteractionService
using System; // Para ArgumentNullException
using System.Collections.ObjectModel; // Para ObservableCollection
using System.ComponentModel; // Para INotifyPropertyChanged
using System.Diagnostics; // Para Debug
using System.IO; // Para Path, Directory, File
using System.Linq; // Para FirstOrDefault
using System.Windows; // Para MessageBox (fallback)
using System.Windows.Input; // Para ICommand

namespace SlideDock.ViewModels
{
    public class MenuGroupViewModel : INotifyPropertyChanged
    {
        private readonly MenuGroup _model;
        private readonly MainViewModel _mainViewModel;
        // Campo para o serviço injetado
        private readonly IDialogService _dialogService;
        // Serviço injetado para interação com arquivos
        private readonly IFileInteractionService _fileInteractionService;

        // Comandos existentes
        public ICommand ToggleExpandCommand { get; }
        public ICommand AddAppCommand { get; } // Agora chama AddAppFromFileDialog
        public ICommand RemoveGroupCommand { get; }

        // Novo comando para adicionar pastas
        public ICommand AddFolderCommand { get; }

        /// <summary>
        /// Construtor que recebe o modelo, o MainViewModel e os serviços.
        /// </summary>
        /// <param name="model">O modelo de dados MenuGroup.</param>
        /// <param name="mainViewModel">Referência ao MainViewModel.</param>
        /// <param name="dialogService">Serviço para exibir diálogos.</param>
        /// <param name="fileInteractionService">Serviço para interação com arquivos.</param>
        public MenuGroupViewModel(MenuGroup model,
                                 MainViewModel mainViewModel,
                                 IDialogService dialogService,
                                 IFileInteractionService fileInteractionService)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            // Atribui o serviço injetado
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            // Atribui o serviço de interação com arquivos injetado
            _fileInteractionService = fileInteractionService ?? throw new ArgumentNullException(nameof(fileInteractionService));

            // Inicialização dos comandos existentes
            ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            // AddAppCommand agora chama AddAppFromFileDialog para clareza
            AddAppCommand = new RelayCommand(_ => AddAppFromFileDialog());
            RemoveGroupCommand = new RelayCommand(_ => _mainViewModel.DockManager.RemoveMenuGroup(this));

            // Inicialização do novo comando
            AddFolderCommand = new RelayCommand(_ => AddFolderFromDialog());

            // Sincroniza os AppIcons do modelo para o ViewModel
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

        // Coleção de AppIconViewModels
        public ObservableCollection<AppIconViewModel> AppIcons { get; } = new ObservableCollection<AppIconViewModel>();

        /// <summary>
        /// Sincroniza os AppIcons do modelo (MenuGroup) para o ViewModel (ObservableCollection<AppIconViewModel>).
        /// </summary>
        private void SyncAppIconsFromModel()
        {
            Debug.WriteLine($"Sincronizando {_model.AppIcons.Count} aplicativos do modelo para o ViewModel");
            AppIcons.Clear();
            foreach (var appIcon in _model.AppIcons)
            {
                // Passa o IDialogService para o construtor do AppIconViewModel
                var appIconViewModel = new AppIconViewModel(appIcon, _dialogService);
                appIconViewModel.RemoveRequested += (sender, e) => RemoveApp(appIconViewModel);
                // Este evento agora serve para "Abrir Local"
                appIconViewModel.OpenFolderRequested += (sender, e) => OpenAppFolder(appIconViewModel);
                AppIcons.Add(appIconViewModel);
                Debug.WriteLine($"App sincronizado: {appIcon.Name}");
            }
            Debug.WriteLine($"Sincronização concluída. ViewModel agora tem {AppIcons.Count} aplicativos");
        }

        /// <summary>
        /// Adiciona um novo AppIcon (arquivo ou pasta) ao grupo com base no caminho do arquivo.
        /// </summary>
        /// <param name="filePath">Caminho do arquivo ou pasta.</param>
        public void AddAppIcon(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            // Determina o tipo do item com base na existência como diretório ou arquivo
            DockItemType itemType = DockItemType.File; // Padrão genérico
            string itemName = "Item Desconhecido";

            if (Directory.Exists(filePath))
            {
                itemType = DockItemType.Folder;
                itemName = Path.GetFileName(filePath); // Nome da pasta
            }
            else if (File.Exists(filePath))
            {
                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                if (extension == ".exe")
                {
                    itemType = DockItemType.Application;
                }
                // else mantém como File
                itemName = Path.GetFileNameWithoutExtension(filePath); // Nome do arquivo sem extensão
            }
            else
            {
                // Caminho não existe, assume como arquivo genérico
                itemName = Path.GetFileNameWithoutExtension(filePath);
                Debug.WriteLine($"Caminho não encontrado ao adicionar item: {filePath}. Adicionando como tipo genérico.");
            }

            var appIcon = new AppIcon
            {
                Name = itemName,
                ExecutablePath = filePath, // Reutiliza ExecutablePath para o caminho
                ItemType = itemType // Define o tipo do item
            };

            // Adiciona ao modelo primeiro
            _model.AppIcons.Add(appIcon);

            // Depois cria o ViewModel e adiciona à coleção observable
            // Passa o IDialogService para o construtor do AppIconViewModel
            var appIconViewModel = new AppIconViewModel(appIcon, _dialogService);
            appIconViewModel.RemoveRequested += (sender, e) => RemoveApp(appIconViewModel);
            // Este evento agora serve para "Abrir Local"
            appIconViewModel.OpenFolderRequested += (sender, e) => OpenAppFolder(appIconViewModel);
            AppIcons.Add(appIconViewModel);

            Debug.WriteLine($"Item adicionado ao grupo: {appIcon.Name} ({appIcon.ItemType}). Total no modelo: {_model.AppIcons.Count}, Total no ViewModel: {AppIcons.Count}");
            _mainViewModel.SaveConfiguration();
        }

        /// <summary>
        /// Abre um diálogo para selecionar um arquivo e adiciona ao grupo.
        /// </summary>
        private void AddAppFromFileDialog() // Renomeado de AddAppFromDialog para clareza
        {
            // Usa o serviço injetado para selecionar o arquivo
            // Atualiza o filtro para permitir qualquer arquivo
            // Como OpenFileDialog é específico do WPF, precisamos usar Microsoft.Win32.OpenFileDialog
            // ou injetar um serviço específico. Para simplificar, vamos criar uma instância aqui.
            // Idealmente, o FileInteractionService teria um método SelectFile genérico.
            var openFileDialog = new OpenFileDialog
            {
                // Filtro atualizado para permitir qualquer arquivo, não apenas .exe
                Filter = "Todos os arquivos (*.*)|*.*",
                Title = "Selecione um arquivo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Usa o método AddAppIcon existente para tratar o arquivo
                AddAppIcon(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Abre um diálogo para selecionar uma pasta e adiciona ao grupo.
        /// </summary>
        private void AddFolderFromDialog()
        {
            // Usa o serviço injetado para selecionar a pasta
            string folderPath = _fileInteractionService?.SelectFolder();

            if (!string.IsNullOrEmpty(folderPath))
            {
                // Usa o método AddAppIcon existente que agora trata tipos
                AddAppIcon(folderPath);
            }
        }

        /// <summary>
        /// Remove um AppIconViewModel após confirmação.
        /// </summary>
        /// <param name="appViewModel">O ViewModel do AppIcon a ser removido.</param>
        public void RemoveApp(AppIconViewModel appViewModel)
        {
            if (appViewModel != null && AppIcons.Contains(appViewModel))
            {
                string message = $"Deseja remover o item '{appViewModel.Name}'?";
                string title = "Confirmar Remoção";

                // Usa o serviço de diálogo injetado para confirmação
                if (_dialogService.ShowConfirmationDialog(message, title))
                {
                    RemoveAppIcon(appViewModel);
                }
            }
        }

        /// <summary>
        /// Remove um AppIconViewModel da coleção e do modelo.
        /// </summary>
        /// <param name="appViewModel">O ViewModel do AppIcon a ser removido.</param>
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
                Debug.WriteLine($"Item removido do grupo: {appViewModel.Name}. Restantes no modelo: {_model.AppIcons.Count}");
            }
            _mainViewModel.SaveConfiguration();
        }

        /// <summary>
        /// Abre o local (pasta) do item. Se o item for uma pasta, abre a própria pasta.
        /// </summary>
        /// <param name="appViewModel">O ViewModel do AppIcon cujo local será aberto.</param>
        private void OpenAppFolder(AppIconViewModel appViewModel) // Renomeado conceitualmente para "Abrir Local"
        {
            if (appViewModel != null && !string.IsNullOrEmpty(appViewModel.ExecutablePath))
            {
                try
                {
                    string pathToOpen = null;

                    // --- Lógica Adaptada ---
                    // Verifica se o caminho é uma pasta (ou usa ItemType)
                    if (appViewModel.ItemType == DockItemType.Folder && Directory.Exists(appViewModel.ExecutablePath))
                    {
                        // Se for uma pasta, abre a própria pasta
                        pathToOpen = appViewModel.ExecutablePath;
                    }
                    else if ((appViewModel.ItemType == DockItemType.Application || appViewModel.ItemType == DockItemType.File) && File.Exists(appViewModel.ExecutablePath))
                    {
                        // Se for um arquivo, abre a pasta pai (comportamento original)
                        pathToOpen = Path.GetDirectoryName(appViewModel.ExecutablePath);
                    }
                    // else: pathToOpen permanece null, e nada acontece

                    if (!string.IsNullOrEmpty(pathToOpen) && (Directory.Exists(pathToOpen) || File.Exists(pathToOpen)))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = pathToOpen,
                            UseShellExecute = true
                        });
                        // Ou, se quiser garantir que seja uma janela do explorer:
                        // Process.Start("explorer.exe", $"\"{pathToOpen}\"");
                    }
                    else
                    {
                        // Opcional: Mostrar mensagem se o caminho não existir
                        // _dialogService?.ShowMessage("O item ou sua pasta não foi encontrado.", "Erro");
                        // Ou MessageBox.Show("O item ou sua pasta não foi encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    // Usar IDialogService se disponível
                    MessageBox.Show($"Erro ao abrir local: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Ou _dialogService?.ShowMessage($"Erro ao abrir local: {ex.Message}", "Erro");
                }
            }
        }

        /// <summary>
        /// Reordena um AppIcon dentro do grupo.
        /// </summary>
        /// <param name="appIcon">O ViewModel do AppIcon a ser reordenado.</param>
        /// <param name="newIndex">O novo índice para o AppIcon.</param>
        public void ReorderAppIcon(AppIconViewModel appIcon, int newIndex)
        {
            if (appIcon == null || !AppIcons.Contains(appIcon)) return;

            int currentIndex = AppIcons.IndexOf(appIcon);
            if (currentIndex == newIndex) return;

            // Clamp the new index to valid range
            newIndex = Math.Max(0, Math.Min(newIndex, AppIcons.Count - 1));

            // Remove from current position
            AppIcons.RemoveAt(currentIndex);

            // Insert at new position
            AppIcons.Insert(newIndex, appIcon);

            // Update the model to match the ViewModel order
            SyncModelFromViewModel();

            Debug.WriteLine($"Item reordenado: {appIcon.Name} de posição {currentIndex} para {newIndex}");
            _mainViewModel.SaveConfiguration();
        }

        /// <summary>
        /// Sincroniza o modelo (MenuGroup.AppIcons) com o ViewModel (AppIcons).
        /// </summary>
        private void SyncModelFromViewModel()
        {
            _model.AppIcons.Clear();
            // Recria a lista de modelos com base na ordem do ViewModel
            foreach (var appIconViewModel in AppIcons)
            {
                var modelAppIcon = new AppIcon
                {
                    Name = appIconViewModel.Name,
                    ExecutablePath = appIconViewModel.ExecutablePath,
                    ItemType = appIconViewModel.ItemType // Sincroniza o tipo também
                };
                _model.AppIcons.Add(modelAppIcon);
            }
        }

        // Implementação da interface INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}