using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SlideDOck
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Verifica e cria pasta de recursos se necessário
            EnsureResourcesDirectory();
        }

        private void EnsureResourcesDirectory()
        {
            try
            {
                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(resourcesPath))
                {
                    Directory.CreateDirectory(resourcesPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao garantir diretório de recursos: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // O salvamento já é feito automaticamente no MainViewModel
            base.OnExit(e);
        }
    }
}