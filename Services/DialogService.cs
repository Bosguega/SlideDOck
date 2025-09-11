// Arquivo: Services\DialogService.cs
using System.Windows;

namespace SlideDock.Services
{
    public class DialogService : IDialogService
    {
        public bool ShowConfirmationDialog(string message, string title)
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);
            return result == MessageBoxResult.Yes;
        }

        // Nova implementação
        public void ShowMessage(string message, string title)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information); // Ou outro tipo de ícone apropriado
        }
    }
}