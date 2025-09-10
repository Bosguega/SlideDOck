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
    }
}