using SlideDock.Models;
using SlideDock.Commands;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SlideDock.Utils;
using System;
using System.Windows; // Adicionado para MessageBox (opcional, se não usar IDialogService para tudo)
using SlideDock.Services; // Adicionado para IDialogService

namespace SlideDock.ViewModels
{
    public class AppIconViewModel : INotifyPropertyChanged
    {
        private readonly AppIcon _model;
        // 1. Adicionar campo para o serviço
        private readonly IDialogService _dialogService;

        public ICommand LaunchAppCommand { get; }
        public ICommand RemoveAppCommand { get; }
        public ICommand OpenFolderCommand { get; }

        public event EventHandler RemoveRequested;
        public event EventHandler OpenFolderRequested;

        // 2. Modificar o construtor para aceitar IDialogService
        public AppIconViewModel(AppIcon model, IDialogService dialogService) // <-- Modificação aqui
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            // 3. Atribuir o serviço injetado
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            LaunchAppCommand = new RelayCommand(_ => LaunchApp());
            RemoveAppCommand = new RelayCommand(_ => OnRemoveRequested());
            OpenFolderCommand = new RelayCommand(_ => OnOpenFolderRequested());
            LoadIcon();
        }

        // Construtor existente pode ser mantido para compatibilidade (opcional, mas requer ajuste na criação)
        // Se mantido, ele deve chamar o novo construtor ou garantir que _dialogService seja inicializado.
        // Por simplicidade e clareza, vamos assumir que o novo construtor será usado.
        // public AppIconViewModel(AppIcon model) : this(model, ???) { } // Como obter o IDialogService aqui? Melhor injetar no local de criação.

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

        private void LoadIcon()
        {
            try
            {
                if (!string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath))
                {
                    IconSource = IconExtractor.ExtractIconToBitmapSource(ExecutablePath);
                }
            }
            catch (Exception ex) // Capturar exceções específicas é melhor
            {
                Debug.WriteLine($"Erro ao carregar ícone para {ExecutablePath}: {ex.Message}");
                // O XAML cuida do fallback
            }
        }

        private void LaunchApp()
        {
            // Verifica se o caminho está preenchido
            if (string.IsNullOrEmpty(ExecutablePath))
            {
                // Opcional: usar IDialogService ou MessageBox diretamente
                MessageBox.Show("Caminho do aplicativo não configurado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Ou _dialogService.ShowMessage("Caminho do aplicativo não configurado.", "Erro"); // se tiver um método ShowMessage
                return;
            }

            // Verifica se o arquivo existe
            if (File.Exists(ExecutablePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = ExecutablePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    // Opcional: usar IDialogService
                    MessageBox.Show($"Erro ao abrir aplicativo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Ou _dialogService.ShowMessage($"Erro ao abrir aplicativo: {ex.Message}", "Erro");
                }
            }
            else
            {
                // --- Arquivo NÃO encontrado - Implementação da Melhoria ---
                string message = $"O arquivo '{Name}' não foi encontrado no caminho:\n{ExecutablePath}\n\nEle pode ter sido movido ou excluído. Deseja removê-lo do SlideDock?";
                string title = "Arquivo Não Encontrado";

                // Usando o IDialogService injetado
                bool shouldRemove = _dialogService.ShowConfirmationDialog(message, title);

                if (shouldRemove)
                {
                    OnRemoveRequested(); // Dispara o evento para remover este ícone
                }
            }
        }


        private void OnRemoveRequested()
        {
            RemoveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnOpenFolderRequested()
        {
            OpenFolderRequested?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}