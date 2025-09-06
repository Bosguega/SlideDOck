using SlideDOck.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SlideDOck.Utils;
using System;

namespace SlideDOck.ViewModels
{
    public class AppIconViewModel : INotifyPropertyChanged
    {
        private readonly AppIcon _model;
        public ICommand LaunchAppCommand { get; }

        public AppIconViewModel(AppIcon model)
        {
            _model = model;
            LaunchAppCommand = new RelayCommand(_ => LaunchApp());
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

        private void LoadIcon()
        {
            try
            {
                if (!string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath))
                {
                    // Tenta extrair o ícone real
                    IconSource = IconExtractor.ExtractIconToBitmapSource(ExecutablePath);
                }

                // Se não conseguir, o XAML cuida do fallback
            }
            catch
            {
                // Deixa o XAML cuidar do ícone padrão
            }
        }

        private void LaunchApp()
        {
            if (!string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath))
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
                    System.Windows.MessageBox.Show($"Erro ao abrir aplicativo: {ex.Message}");
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