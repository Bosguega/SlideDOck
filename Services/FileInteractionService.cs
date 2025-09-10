using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Linq;

namespace SlideDock.Services
{
    public class FileInteractionService : IFileInteractionService
    {
        public string SelectExecutableFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executáveis (*.exe)|*.exe|Todos os arquivos (*.*)|*.*",
                Title = "Selecione um aplicativo",
                CheckFileExists = true,
                CheckPathExists = true
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        public string[] GetDroppedExecutableFiles(DragEventArgs e)
        {
            return GetDroppedFiles(e).Where(f => Path.GetExtension(f).ToLower() == ".exe").ToArray();
        }

        public string[] GetDroppedFiles(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return (string[])e.Data.GetData(DataFormats.FileDrop);
            }
            return new string[0];
        }
    }
}