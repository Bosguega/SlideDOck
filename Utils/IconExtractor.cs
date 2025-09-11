// Arquivo: Utils\IconExtractor.cs
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SlideDock.Utils
{
    public static class IconExtractor
    {
        // Cache para o ícone padrão de pasta
        private static BitmapSource _defaultFolderIcon;

        /// <summary>
        /// Obtém um ícone padrão para pastas, com fallback.
        /// </summary>
        public static BitmapSource DefaultFolderIcon
        {
            get
            {
                if (_defaultFolderIcon == null)
                {
                    try
                    {
                        Icon icon = GetFolderIcon();
                        if (icon != null)
                        {
                            _defaultFolderIcon = icon.ToBitmapSource();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao carregar ícone padrão de pasta (Windows): {ex.Message}");
                        _defaultFolderIcon = null;
                    }
                }
                return _defaultFolderIcon;
            }
        }

        /// <summary>
        /// Extrai o ícone associado a um caminho de arquivo ou pasta e o converte para BitmapSource.
        /// </summary>
        /// <param name="filePath">O caminho completo do arquivo ou pasta.</param>
        /// <returns>Um BitmapSource do ícone, ou null se não puder ser extraído.</returns>
        public static BitmapSource ExtractIconToBitmapSource(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return null;

                // Se for pasta
                if (Directory.Exists(filePath))
                {
                    Icon icon = Icon.ExtractAssociatedIcon(filePath);

                    if (icon == null)
                    {
                        // Se não tiver ícone associado, usa o padrão do Windows
                        icon = GetFolderIcon();
                    }

                    if (icon != null)
                    {
                        Debug.WriteLine($"Ícone usado para pasta: {filePath}");
                        return icon.ToBitmapSource();
                    }

                    Debug.WriteLine($"Nenhum ícone encontrado para a pasta: {filePath}");
                    return null;
                }
                // Se for arquivo
                else if (File.Exists(filePath))
                {
                    Icon icon = Icon.ExtractAssociatedIcon(filePath);
                    if (icon != null)
                    {
                        Debug.WriteLine($"Ícone extraído para arquivo: {filePath}");
                        return icon.ToBitmapSource();
                    }
                    else
                    {
                        Debug.WriteLine($"Ícone associado não encontrado para o arquivo: {filePath}.");
                    }
                }
                else
                {
                    Debug.WriteLine($"Caminho não encontrado para extração de ícone: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao extrair ícone para BitmapSource de '{filePath}': {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// (Mantido para compatibilidade) Extrai o ícone e salva como PNG em um diretório.
        /// </summary>
        /// <param name="filePath">Caminho do arquivo.</param>
        /// <param name="outputPath">Diretório de saída.</param>
        /// <returns>Caminho do arquivo PNG salvo, ou null se falhar.</returns>
        public static string ExtractIconToFile(string filePath, string outputPath)
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(filePath);
                if (icon != null)
                {
                    using (Bitmap bitmap = icon.ToBitmap())
                    {
                        Directory.CreateDirectory(outputPath);

                        string iconPath = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(filePath)}.png");
                        bitmap.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
                        return iconPath;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao extrair ícone para arquivo: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Converte um System.Drawing.Icon para System.Windows.Media.Imaging.BitmapSource.
        /// </summary>
        /// <param name="icon">O ícone a ser convertido.</param>
        /// <returns>O BitmapSource convertido.</returns>
        public static BitmapSource ToBitmapSource(this Icon icon)
        {
            if (icon == null) return null;

            using (Bitmap bitmap = icon.ToBitmap())
            {
                IntPtr hBitmap = bitmap.GetHbitmap();
                BitmapSource retval;

                try
                {
                    retval = Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(hBitmap);
                }

                return retval;
            }
        }

        /// <summary>
        /// Libera um objeto gráfico não gerenciado.
        /// </summary>
        /// <param name="hObject">Handle do objeto.</param>
        /// <returns>True se bem sucedido.</returns>
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        // ============================
        // Código novo para pegar ícone de pasta do Windows
        // ============================

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0; // 32x32
        private const uint SHGFI_SMALLICON = 0x1; // 16x16
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        private static Icon GetFolderIcon()
        {
            SHFILEINFO shinfo = new SHFILEINFO();

            SHGetFileInfo(
                "",
                FILE_ATTRIBUTE_DIRECTORY,
                ref shinfo,
                (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON);

            if (shinfo.hIcon != IntPtr.Zero)
                return Icon.FromHandle(shinfo.hIcon);

            return null;
        }
    }
}
