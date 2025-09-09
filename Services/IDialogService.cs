using System.Windows;

namespace SlideDOck.Services
{
    public interface IDialogService
    {
        /// <summary>
        /// Exibe uma mensagem de confirmação
        /// </summary>
        /// <param name="message">Mensagem a ser exibida</param>
        /// <param name="title">Título da janela</param>
        /// <returns>True se o usuário confirmar, False caso contrário</returns>
        bool ShowConfirmationDialog(string message, string title);
    }
}