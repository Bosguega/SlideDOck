using SlideDOck.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace SlideDOck.ViewModels
{
    public class MenuGroupViewModel : INotifyPropertyChanged
    {
        private readonly MenuGroup _model;
        private readonly MainViewModel _mainViewModel;
        public ICommand ToggleExpandCommand { get; }
        public ICommand AddAppCommand { get; }
        public ICommand RemoveGroupCommand { get; }

        public MenuGroupViewModel(MenuGroup model, MainViewModel mainViewModel)
        {
            _model = model;
            _mainViewModel = mainViewModel;
            ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            AddAppCommand = new RelayCommand(_ => AddAppFromDialog());
            RemoveGroupCommand = new RelayCommand(_ => _mainViewModel.MenuGroups.Remove(this));
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

        public ObservableCollection<AppIconViewModel> AppIcons { get; } = new ObservableCollection<AppIconViewModel>();

        public void AddAppIcon(AppIcon appIcon)
        {
            AppIcons.Add(new AppIconViewModel(appIcon));
        }

        private void AddAppFromDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executáveis (*.exe)|*.exe|Todos os arquivos (*.*)|*.*",
                Title = "Selecione um aplicativo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var appIcon = new AppIcon
                {
                    Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName),
                    ExecutablePath = openFileDialog.FileName,
                    IconPath = GetIconPathForFile(openFileDialog.FileName)
                };

                AddAppIcon(appIcon);
            }
        }

        private string GetIconPathForFile(string filePath)
        {
            try
            {
                // Extrair ícone do executável
                string iconPath = ExtractIconFromFile(filePath);
                if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                {
                    return iconPath;
                }
            }
            catch
            {
                // Se falhar, retorna ícone padrão
            }

            // Retorna ícone padrão
            return "pack://application:,,,/Resources/default_app.png";
        }

        private string ExtractIconFromFile(string filePath)
        {
            try
            {
                // Aqui você pode implementar a extração real do ícone
                // Por enquanto, retorna um caminho padrão
                return "pack://application:,,,/Resources/default_app.png";
            }
            catch
            {
                return "pack://application:,,,/Resources/default_app.png";
            }
        }

        public void RemoveApp(AppIconViewModel appViewModel)
        {
            if (appViewModel != null && AppIcons.Contains(appViewModel))
            {
                AppIcons.Remove(appViewModel);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}