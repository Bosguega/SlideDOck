using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SlideDOck.Models
{
    public class MenuGroup : INotifyPropertyChanged
    {
        private string _name;
        private bool _isExpanded;

        public MenuGroup()
        {
            // Inicializa a coleção para evitar referências nulas
            AppIcons = new ObservableCollection<AppIcon>();
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AppIcon> AppIcons { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}