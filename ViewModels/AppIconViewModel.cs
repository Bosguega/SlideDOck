using SlideDock.Models;
using SlideDock.Commands;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SlideDock.Utils;
using System;
using System.Windows;

namespace SlideDock.ViewModels
{
    public class AppIconViewModel : INotifyPropertyChanged
    {
        private readonly AppIcon _model;
        public ICommand LaunchAppCommand { get; }
        public ICommand RemoveAppCommand { get; }
        public ICommand OpenFolderCommand { get; }

        public event EventHandler RemoveRequested;
        public event EventHandler OpenFolderRequested;

        public AppIconViewModel(AppIcon model)
        {
            _model = model;
            LaunchAppCommand = new RelayCommand(_ => LaunchApp());
            RemoveAppCommand = new RelayCommand(_ => OnRemoveRequested());
            OpenFolderCommand = new RelayCommand(_ => OnOpenFolderRequested());
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
                    IconSource = IconExtractor.ExtractIconToBitmapSource(ExecutablePath);
                }
            }
            catch
            {
                // O XAML cuida do fallback
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
                    MessageBox.Show($"Erro ao abrir aplicativo: {ex.Message}");
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