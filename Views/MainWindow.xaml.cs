using System.Windows;

namespace SlideDOck.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Left = 0;
            Top = 0;
            Height = SystemParameters.WorkArea.Height;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Main window closing...");
        }
    }
}