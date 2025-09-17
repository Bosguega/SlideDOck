using System.Windows;

namespace SlideDock.Views
{
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();
            // Define o DataContext para permitir binding com o MainViewModel
            // Assume-se que a janela principal tenha o MainViewModel como DataContext
            if (Application.Current.MainWindow?.DataContext is ViewModels.MainViewModel mainViewModel)
            {
                this.DataContext = mainViewModel;
            }
            // Se o DataContext não for definido aqui, o binding não funcionará
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}