// Arquivo: Models\AppIcon.cs
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace SlideDock.Models
{
    // Novo Enum
    public enum DockItemType
    {
        Application, // Para .exe e similares, se quisermos diferenciar
        File,        // Para arquivos genéricos
        Folder       // Para pastas
    }

    public class AppIcon : INotifyPropertyChanged
    {
        private string _name;
        private string _executablePath; // Mantém o nome para compatibilidade
        private BitmapSource _iconSource;
        private DockItemType _itemType = DockItemType.File; // Valor padrão

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

        // Renomear para Path ou manter para compatibilidade?
        // Vamos manter por enquanto para evitar mudanças muito amplas.
        public string ExecutablePath
        {
            get => _executablePath;
            set
            {
                _executablePath = value;
                OnPropertyChanged();
            }
        }

        // Nova propriedade
        public DockItemType ItemType
        {
            get => _itemType;
            set
            {
                _itemType = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}