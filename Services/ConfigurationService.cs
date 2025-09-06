using Newtonsoft.Json;
using SlideDOck.Models;
using SlideDOck.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SlideDOck.Services
{
    public class ConfigurationService
    {
        private readonly string _configPath;

        public ConfigurationService()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SlideDOck");
            // Garante que o diretório exista
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
                    Debug.WriteLine("Configuração carregada com sucesso");
                    return JsonConvert.DeserializeObject<DockConfiguration>(json) ?? new DockConfiguration();
                }
                else
                {
                    Debug.WriteLine("Arquivo de configuração não encontrado, usando padrão");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar configuração: {ex.Message}");
            }

            return new DockConfiguration();
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

        public void SaveMenuGroups(IEnumerable<MenuGroupViewModel> menuGroups)
        {
            var config = new DockConfiguration();

            foreach (var group in menuGroups)
            {
                var groupData = new MenuGroupData
                {
                    Name = group.Name,
                    IsExpanded = group.IsExpanded
                };

                foreach (var app in group.AppIcons)
                {
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