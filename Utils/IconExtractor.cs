// Arquivo: Utils\IconExtractor.cs
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                    // Tenta extrair o ícone padrão da pasta do sistema Windows
                    try
                    {
                        string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        if (Directory.Exists(systemFolder))
                        {
                            Icon folderIcon = Icon.ExtractAssociatedIcon(systemFolder);
                            if (folderIcon != null)
                            {
                                _defaultFolderIcon = folderIcon.ToBitmapSource();
                                return _defaultFolderIcon;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao carregar ícone padrão de pasta (Windows): {ex.Message}");
                    }

                    // Se falhar, tenta com a pasta do usuário
                    try
                    {
                        string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        if (Directory.Exists(userFolder))
                        {
                            Icon folderIcon = Icon.ExtractAssociatedIcon(userFolder);
                            if (folderIcon != null)
                            {
                                _defaultFolderIcon = folderIcon.ToBitmapSource();
                                return _defaultFolderIcon;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao carregar ícone padrão de pasta (User): {ex.Message}");
                    }

                    // Se ainda falhar, cria um ícone padrão simples (fallback)
                    _defaultFolderIcon = CreateDefaultFolderIcon();
                }
                return _defaultFolderIcon;
            }
        }

        /// <summary>
        /// Cria um ícone de pasta padrão programaticamente como fallback final.
        /// </summary>
        /// <returns>Um BitmapSource representando um ícone de pasta simples.</returns>
        private static BitmapSource CreateDefaultFolderIcon()
        {
            // Criar um RenderTargetBitmap para renderizar o desenho
            int size = 48;
            var renderTarget = new RenderTargetBitmap(size, size, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);

            // Criar o desenho
            var drawingGroup = new System.Windows.Media.DrawingGroup();

            // Fundo amarelo simulando uma pasta
            var backgroundGeometry = new System.Windows.Media.RectangleGeometry(new Rect(0, 0, size, size));
            // Usar System.Windows.Media.Colors
            var backgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow);
            var backgroundDrawing = new System.Windows.Media.GeometryDrawing(backgroundBrush, null, backgroundGeometry);

            // Detalhe para parecer mais uma pasta (opcional)
            var tabGeometry = new System.Windows.Media.RectangleGeometry(new Rect(0, 0, size / 2, size / 3)); // Aba da pasta
            var tabBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Goldenrod);
            var tabDrawing = new System.Windows.Media.GeometryDrawing(tabBrush, null, tabGeometry);

            drawingGroup.Children.Add(backgroundDrawing);
            drawingGroup.Children.Add(tabDrawing);

            // Renderizar o desenho em um DrawingVisual e depois no RenderTargetBitmap
            var drawingVisual = new System.Windows.Media.DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                context.DrawDrawing(drawingGroup);
            }
            renderTarget.Render(drawingVisual);

            return renderTarget;
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

                // Verifica se o caminho é de uma pasta existente
                if (Directory.Exists(filePath))
                {
                    // Tenta extrair o ícone associado à pasta
                    Icon icon = Icon.ExtractAssociatedIcon(filePath);
                    if (icon != null)
                    {
                        Debug.WriteLine($"Ícone extraído para pasta: {filePath}");
                        return icon.ToBitmapSource();
                    }
                    else
                    {
                        // Se não conseguir, sinaliza para usar o padrão
                        Debug.WriteLine($"Ícone associado não encontrado para a pasta: {filePath}. Sinalizando para usar padrão.");
                        return null; // Importante retornar null aqui para acionar o padrão
                    }
                }
                // Verifica se o caminho é de um arquivo existente
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
            // Retorna null se não foi possível extrair um ícone específico
            // O chamador deve cuidar do fallback (placeholder ou ícone padrão)
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
                    // Libera o handle do bitmap não gerenciado
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
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}