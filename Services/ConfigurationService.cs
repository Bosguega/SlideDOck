using Newtonsoft.Json;
using SlideDock.Models;
using SlideDock.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SlideDock.Services
{
    public class ConfigurationService
    {
        private readonly string _configPath;
        private readonly ISampleDataProvider _sampleDataProvider;

        public ConfigurationService(ISampleDataProvider sampleDataProvider)
        {
            _sampleDataProvider = sampleDataProvider;
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SlideDock");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            _configPath = Path.Combine(appDataPath, "configuration.json");
            Debug.WriteLine($"Config path: {_configPath}");
        }

        public DockConfiguration LoadConfiguration()
        {
            try
            {
                Debug.WriteLine($"Tentando carregar configuração de: {_configPath}");

                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    Debug.WriteLine($"Arquivo de configuração encontrado. Tamanho: {json.Length} caracteres");

                    var config = JsonConvert.DeserializeObject<DockConfiguration>(json);
                    if (config != null)
                    {
                        Debug.WriteLine($"Configuração carregada com sucesso. Grupos: {config.MenuGroups.Count}");
                        return config;
                    }
                    else
                    {
                        Debug.WriteLine("Falha ao desserializar a configuração. Retornando configuração de exemplo.");
                        return _sampleDataProvider.GetSampleConfiguration();
                    }
                }
                else
                {
                    Debug.WriteLine("Arquivo de configuração não encontrado, usando configuração de exemplo");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar configuração: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            return _sampleDataProvider.GetSampleConfiguration();
        }

public void SaveConfiguration(DockConfiguration config)
        {
            try
            {
                Debug.WriteLine($"Tentando salvar configuração em: {_configPath}");
                // Garante que o diretório exista antes de salvar
                string directory = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
                Debug.WriteLine("Configuração salva com sucesso");

                // Verifica se o arquivo foi realmente criado
                if (File.Exists(_configPath))
                {
                    Debug.WriteLine($"Arquivo salvo com {new FileInfo(_configPath).Length} bytes");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao salvar configuração: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        public void SaveMenuGroups(IEnumerable<MenuGroupViewModel> menuGroups, bool isExpanded, DockPosition dockPosition)
        {
            var config = new DockConfiguration
            {
                IsExpanded = isExpanded,
                DockPosition = dockPosition
            };

            Debug.WriteLine($"Iniciando salvamento de {menuGroups.Count()} grupos");

            foreach (var group in menuGroups)
            {
                var groupData = new MenuGroupData
                {
                    Name = group.Name,
                    IsExpanded = group.IsExpanded
                };

                Debug.WriteLine($"Salvando grupo '{group.Name}' com {group.AppIcons.Count} aplicativos");

                foreach (var app in group.AppIcons)
                {
                    Debug.WriteLine($"Salvando app: {app.Name}, {app.ExecutablePath}");
                    groupData.AppIcons.Add(new AppIconData
                    {
                        Name = app.Name,
                        ExecutablePath = app.ExecutablePath
                    });
                }

                config.MenuGroups.Add(groupData);
            }

            SaveConfiguration(config);
        }
    }
}