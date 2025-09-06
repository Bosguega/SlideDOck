using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SlideDOck.Utils
{
    public static class IconExtractor
    {
        public static string ExtractIconToFile(string filePath, string outputPath)
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(filePath);
                if (icon != null)
                {
                    using (Bitmap bitmap = icon.ToBitmap())
                    {
                        // Garante que o diretório de saída exista
                        Directory.CreateDirectory(outputPath);

                        string iconPath = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(filePath)}.png");
                        bitmap.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
                        return iconPath;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Erro ao extrair ícone: {ex.Message}");
            }

            return "pack://application:,,,/Resources/default_app.png";
        }

        public static BitmapSource ExtractIconToBitmapSource(string filePath)
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(filePath);
                if (icon != null)
                {
                    return icon.ToBitmapSource();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao extrair ícone para BitmapSource: {ex.Message}");
            }

            return null;
        }

        public static BitmapSource ToBitmapSource(this Icon icon)
        {
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

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}