using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace SlideDock.Models
{
    public class AppIcon : INotifyPropertyChanged
    {
        private string _name;
        private string _executablePath;
        private BitmapSource _iconSource;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public BitmapSource IconSource
        {
            get => _iconSource;
            set
            {
                _iconSource = value;
                OnPropertyChanged();
            }
        }

        public string ExecutablePath
        {
            get => _executablePath;
            set
            {
                _executablePath = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}