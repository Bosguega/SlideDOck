using System.Windows.Controls;
using System.Windows.Input;

namespace SlideDOck.Views
{
    public partial class AppIconView : UserControl
    {
        public AppIconView()
        {
            InitializeComponent();
        }

        private void AppIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Implementar drag de ícones existentes se necessário
            }
        }
    }
}