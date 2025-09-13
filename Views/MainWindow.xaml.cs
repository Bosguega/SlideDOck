using System.Windows;
using SlideDock.ViewModels;
using System.ComponentModel;

namespace SlideDock.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            
            // Definir a posição inicial da janela
            SetInitialWindowPosition();

            // Se o DataContext já estiver definido (como em Design-time), ouça as mudanças.
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
            // Caso contrário, aguarde o DataContext ser definido e então ouça as mudanças.
            else
            {
                this.DataContextChanged += MainWindow_DataContextChanged;
            }

            Closing += MainWindow_Closing;
        }

        private void SetInitialWindowPosition()
        {
            // Define a altura da janela para a altura da área de trabalho
            Height = SystemParameters.WorkArea.Height;

            // Define a posição inicial com base na DockPosition no ViewModel (se disponível)
            if (ViewModel != null && ViewModel.DockPosition == Models.DockPosition.Right)
            {
                Left = SystemParameters.WorkArea.Width - Width;
            }
            else
            {
                Left = 0;
            }
            Top = 0;
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged; // Evitar duplicação
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                SetInitialWindowPosition(); // Definir a posição assim que o DataContext estiver pronto
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.DockPosition))
            {
                if (ViewModel.DockPosition == Models.DockPosition.Right)
                {
                    Left = SystemParameters.WorkArea.Width - Width; // Mover para a direita
                }
                else
                {
                    Left = 0; // Mover para a esquerda
                }
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Main window closing...");
            // Save configuration on close
            ViewModel?.SaveConfiguration();
        }
    }
}