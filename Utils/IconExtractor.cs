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
        private static BitmapSource _defaultFolderIcon;

        public static BitmapSource DefaultFolderIcon
        {
            get
            {
                if (_defaultFolderIcon == null)
                {
                    try
                    {
                        _defaultFolderIcon = GetFolderIcon()?.ToBitmapSource();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Erro ao carregar ícone padrão de pasta: {ex.Message}");
                    }
                }
                return _defaultFolderIcon;
            }
        }

        public static BitmapSource ExtractIconToBitmapSource(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;

            try
            {
                Icon? icon = null;

                if (Directory.Exists(filePath))
                {
                    icon = Icon.ExtractAssociatedIcon(filePath) ?? GetFolderIcon();
                }
                else if (File.Exists(filePath))
                {
                    icon = Icon.ExtractAssociatedIcon(filePath);
                }

                return icon?.ToBitmapSource();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao extrair ícone de '{filePath}': {ex.Message}");
                return null;
            }
        }

        public static string ExtractIconToFile(string filePath, string outputPath)
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(filePath);
                if (icon == null) return null;

                using Bitmap bitmap = icon.ToBitmap();
                Directory.CreateDirectory(outputPath);
                string iconPath = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(filePath)}.png");
                bitmap.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
                return iconPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao salvar ícone em arquivo: {ex.Message}");
                return null;
            }
        }

        public static BitmapSource? ToBitmapSource(this Icon icon)
        {
            if (icon == null) return null;

            using Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        // =============================
        // Ícone padrão de pasta via Windows API
        // =============================
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        private static Icon GetFolderIcon()
        {
            SHFILEINFO shinfo = new();
            SHGetFileInfo("", FILE_ATTRIBUTE_DIRECTORY, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);
            return shinfo.hIcon != IntPtr.Zero ? Icon.FromHandle(shinfo.hIcon) : null;
        }
    }
}
