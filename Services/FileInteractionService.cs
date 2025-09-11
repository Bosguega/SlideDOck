// Arquivo: Services\FileInteractionService.cs
// Especificar os using necessários e evitar conflitos
using Microsoft.Win32; // Para OpenFileDialog WPF
using System.IO; // Para Path, File, Directory
using System.Linq; // Para Where, ToArray
// Adicionando o using para Windows Forms com um alias para evitar conflitos
using WinForms = System.Windows.Forms; // Alias para Windows Forms

namespace SlideDock.Services
{
    public class FileInteractionService : IFileInteractionService
    {
        public string SelectExecutableFile()
        {
            // Qualificar OpenFileDialog com Microsoft.Win32
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executáveis (.exe)|.exe|Todos os arquivos (.)|.",
                Title = "Selecione um aplicativo",
                CheckFileExists = true,
                CheckPathExists = true
            };
            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        // Qualificar DragEventArgs com System.Windows
        public string[] GetDroppedExecutableFiles(System.Windows.DragEventArgs e)
        {
            // Qualificar DataFormats com System.Windows
            return GetDroppedFiles(e).Where(f => Path.GetExtension(f).ToLower() == ".exe").ToArray();
        }

        // Qualificar DragEventArgs com System.Windows
        public string[] GetDroppedFiles(System.Windows.DragEventArgs e)
        {
            // Qualificar DataFormats com System.Windows
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                return (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            }
            return new string[0];
        }

        // Nova implementação
        public string SelectFolder()
        {
            // Usar o alias WinForms para FolderBrowserDialog
            using (var folderDialog = new WinForms.FolderBrowserDialog())
            {
                folderDialog.Description = "Selecione uma pasta";
                folderDialog.UseDescriptionForTitle = true; // Requer .NET 5+ ou referência a System.Windows.Forms

                // Usar o alias WinForms para DialogResult
                var result = folderDialog.ShowDialog();
                // Verifica se o resultado é OK (usuário selecionou uma pasta)
                // Usar o alias WinForms para DialogResult.OK
                if (result == WinForms.DialogResult.OK)
                {
                    return folderDialog.SelectedPath;
                }
            }
            // Retorna null se o usuário cancelar ou ocorrer um erro
            return null;
        }
    }
}