using System.Windows;

namespace SlideDock.Services
{
    public interface IFileInteractionService
    {
        /// <summary>
        /// Abre um diálogo para selecionar um arquivo executável
        /// </summary>
        /// <returns>Caminho do arquivo selecionado ou null se cancelado</returns>
        string SelectExecutableFile();

        /// <summary>
        /// Obtém os arquivos executáveis de um evento de drag & drop
        /// </summary>
        /// <param name="e">Argumentos do evento de drag</param>
        /// <returns>Array de caminhos de arquivos .exe</returns>
        string[] GetDroppedExecutableFiles(DragEventArgs e);

        /// <summary>
        /// Obtém todos os arquivos de um evento de drag & drop.
        /// </summary>
        /// <param name="e">Argumentos do evento de drag</param>
        /// <returns>Array de caminhos de arquivos</returns>
        string[] GetDroppedFiles(DragEventArgs e);
    }
}