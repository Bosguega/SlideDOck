using System.ComponentModel;
using System.Windows.Input;
using SlideDock.Commands;
using System;

namespace SlideDock.ViewModels
{
    public class SettingsMenuViewModel : INotifyPropertyChanged
    {
        private bool _isAlwaysOnTop;

        public ICommand CloseSettingsCommand { get; }

        public event EventHandler CloseRequested;

        public bool IsAlwaysOnTop
        {
            get => _isAlwaysOnTop;
            set
            {
                if (_isAlwaysOnTop != value)
                {
                    _isAlwaysOnTop = value;
                    OnPropertyChanged();
                }
            }
        }

        public SettingsMenuViewModel()
        {
            CloseSettingsCommand = new RelayCommand(_ => OnCloseRequested());
        }

        private void OnCloseRequested()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
