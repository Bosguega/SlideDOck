// Arquivo: ViewModels\AppIconViewModel.cs
using SlideDock.Models; // Para DockItemType
using SlideDock.Commands;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SlideDock.Utils;
using System;
using System.Windows; // Para MessageBox (fallback)
using SlideDock.Services; // Para IDialogService

namespace SlideDock.ViewModels
{
    public class AppIconViewModel : INotifyPropertyChanged
    {
        private readonly AppIcon _model;
        // Campo para o serviço injetado
        private readonly IDialogService _dialogService;

        public ICommand LaunchAppCommand { get; }
        public ICommand RemoveAppCommand { get; }
        public ICommand OpenFolderCommand { get; } // Agora serve para "Abrir Local"

        public event EventHandler RemoveRequested;
        public event EventHandler OpenFolderRequested; // Agora serve para "Abrir Local"

        /// <summary>
        /// Construtor que recebe o modelo e o serviço de diálogo.
        /// </summary>
        /// <param name="model">O modelo de dados AppIcon.</param>
        /// <param name="dialogService">Serviço para exibir diálogos.</param>
        public AppIconViewModel(AppIcon model, IDialogService dialogService)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            // Atribui o serviço injetado
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            LaunchAppCommand = new RelayCommand(_ => LaunchApp());
            RemoveAppCommand = new RelayCommand(_ => OnRemoveRequested());
            OpenFolderCommand = new RelayCommand(_ => OnOpenFolderRequested()); // Agora "Abrir Local"
            LoadIcon();
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

        public BitmapSource IconSource
        {
            get => _model.IconSource;
            set
            {
                _model.IconSource = value;
                OnPropertyChanged();
            }
        }

        public string ExecutablePath
        {
            get => _model.ExecutablePath;
            set
            {
                _model.ExecutablePath = value;
                OnPropertyChanged();
            }
        }

        // Propriedade para acessar o tipo do item
        public DockItemType ItemType
        {
            get => _model.ItemType;
            set
            {
                _model.ItemType = value;
                OnPropertyChanged();
            }
        }

        private void LoadIcon()
{
    try
    {
        // Verificação de caminho não vazio
        if (!string.IsNullOrEmpty(ExecutablePath))
        {
            BitmapSource? icon = null;

            // Verifica se o item (arquivo ou pasta) existe antes de tentar carregar o ícone
            if (ItemType == DockItemType.Folder && Directory.Exists(ExecutablePath))
            {
                // Tenta extrair o ícone da pasta
                icon = IconExtractor.ExtractIconToBitmapSource(ExecutablePath);
                // Se falhar, usa o ícone padrão para pastas
                if (icon == null)
                {
                     icon = IconExtractor.DefaultFolderIcon;
                     Debug.WriteLine($"Usando ícone padrão para pasta '{Name}' ({ExecutablePath})");
                }
            }
            else if ((ItemType == DockItemType.Application || ItemType == DockItemType.File) && File.Exists(ExecutablePath))
            {
                // Tenta extrair o ícone do arquivo
                icon = IconExtractor.ExtractIconToBitmapSource(ExecutablePath);
                 // Para arquivos, se falhar, o XAML cuida do placeholder
                 if (icon == null)
                 {
                      Debug.WriteLine($"Ícone não encontrado para arquivo '{Name}' ({ExecutablePath})");
                 }
            }
            // Se o item não existir, mantém o ícone atual ou usa o placeholder do XAML

            if (icon != null)
            {
                 IconSource = icon;
            }
        }
    }
    catch (Exception ex) // Capturar exceções específicas é melhor
    {
        Debug.WriteLine($"Erro ao carregar ícone para {ExecutablePath} ({ItemType}): {ex.Message}");
        // O XAML cuida do fallback
    }
}

        // Método LaunchApp atualizado para tratar diferentes tipos
        private void LaunchApp()
        {
            // Verifica se o caminho está preenchido
            if (string.IsNullOrEmpty(ExecutablePath))
            {
                // Usar IDialogService se disponível
                _dialogService?.ShowMessage("Caminho do item não configurado.", "Erro");
                return;
            }

            try
            {
                if (ItemType == DockItemType.Folder)
                {
                    // --- Lógica para Pasta ---
                    if (Directory.Exists(ExecutablePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = ExecutablePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        // Pasta não encontrada - usar a lógica existente de confirmação
                        HandleItemNotFound("pasta");
                    }
                }
                else if (ItemType == DockItemType.Application)
                {
                    // --- Lógica para Aplicativo (.exe) ---
                    if (File.Exists(ExecutablePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = ExecutablePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        // App não encontrado - usar a lógica existente de confirmação
                        HandleItemNotFound("aplicativo");
                    }
                }
                else // ItemType.File ou outros tipos genéricos de arquivo
                {
                    // --- Lógica para Arquivo Genérico ---
                    if (File.Exists(ExecutablePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = ExecutablePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        HandleItemNotFound("arquivo");
                    }
                }
            }
            catch (Exception ex)
            {
                // Usar IDialogService se disponível
                _dialogService?.ShowMessage($"Erro ao abrir o item: {ex.Message}", "Erro");
                // Ou MessageBox.Show($"Erro ao abrir o item: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método auxiliar para lidar com itens não encontrados
        private void HandleItemNotFound(string itemTypeLabel)
        {
            string message = $"O {itemTypeLabel} '{Name}' não foi encontrado no caminho:\n{ExecutablePath}\n\nEle pode ter sido movido ou excluído. Deseja removê-lo do SlideDock?";
            string title = $"{itemTypeLabel} Não Encontrado";

            // Usa o serviço de diálogo injetado
            bool shouldRemove = _dialogService?.ShowConfirmationDialog(message, title) ?? false; // Segurança contra null

            if (shouldRemove)
            {
                OnRemoveRequested();
            }
        }

        // Método OnOpenFolderRequested renomeado conceitualmente para "Abrir Local"
        // Mas o nome do método e do evento permanecem os mesmos para compatibilidade
        private void OnOpenFolderRequested()
        {
            OpenFolderRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnRemoveRequested()
        {
            RemoveRequested?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}