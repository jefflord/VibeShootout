using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VibeShootout.Backend.Models;

namespace VibeShootout.Backend.Services
{
    public class ConfigService
    {
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        private AppConfig? _config;

        public async Task<AppConfig> GetConfigAsync()
        {
            if (_config == null)
            {
                await LoadConfigAsync();
            }
            return _config!;
        }

        public async Task SaveConfigAsync(AppConfig config)
        {
            _config = config;
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(_configPath, json);
        }

        private async Task LoadConfigAsync()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(_configPath);
                    _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
                catch
                {
                    _config = new AppConfig();
                }
            }
            else
            {
                _config = new AppConfig();
                await SaveConfigAsync(_config);
            }
        }
    }
}