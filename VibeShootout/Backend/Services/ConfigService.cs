using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VibeShootout.Backend.Models;

namespace VibeShootout.Backend.Services
{
    public class ConfigService
    {
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        private AppConfig? _config;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

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
            var json = JsonSerializer.Serialize(config, JsonOptions);
            await File.WriteAllTextAsync(_configPath, json);
        }

        private async Task LoadConfigAsync()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(_configPath);
                    
                    // Try to deserialize as the new format first
                    try
                    {
                        _config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
                    }
                    catch
                    {
                        // If that fails, try to handle migration from old format
                        var oldConfig = JsonSerializer.Deserialize<JsonElement>(json);
                        _config = MigrateOldConfig(oldConfig);
                        
                        // Save the migrated config
                        await SaveConfigAsync(_config);
                        Console.WriteLine("Migrated configuration from old format to new format");
                    }
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

        private AppConfig MigrateOldConfig(JsonElement oldConfig)
        {
            var newConfig = new AppConfig();
            
            // Migrate existing values
            if (oldConfig.TryGetProperty("RepositoryPath", out var repositoryPath))
            {
                newConfig.RepositoryPath = repositoryPath.GetString() ?? "";
            }
            
            if (oldConfig.TryGetProperty("ReviewPrompt", out var reviewPrompt))
            {
                newConfig.ReviewPrompt = reviewPrompt.GetString() ?? "Below is git diff, please create a code review using this information";
            }
            
            if (oldConfig.TryGetProperty("OllamaUrl", out var ollamaUrl))
            {
                newConfig.OllamaUrl = ollamaUrl.GetString() ?? "http://10.0.0.90:11434";
            }
            
            // Set default provider to Ollama for existing configs
            newConfig.Provider = AIProvider.Ollama;
            
            return newConfig;
        }
    }
}