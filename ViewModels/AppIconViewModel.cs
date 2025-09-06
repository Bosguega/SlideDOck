using SlideDOck.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

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
        }

        public string Name
        {
            get => _model.Name;
            set => _model.Name = value;
        }

        public string IconPath
        {
            get => _model.IconPath;
            set => _model.IconPath = value;
        }

        public string ExecutablePath
        {
            get => _model.ExecutablePath;
            set => _model.ExecutablePath = value;
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
                catch
                {
                    // Handle error
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