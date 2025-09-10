using SlideDock.Models;

namespace SlideDock.Services
{
    public interface ISampleDataProvider
    {
        /// <summary>
        /// Fornece uma configuração de exemplo inicial para o aplicativo
        /// </summary>
        /// <returns>Configuração com grupos e aplicativos de exemplo</returns>
        DockConfiguration GetSampleConfiguration();
    }
}