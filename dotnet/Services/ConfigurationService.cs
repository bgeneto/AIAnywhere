using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AIAnywhere.Models;

namespace AIAnywhere.Services
{
    public class ConfigurationService
    {
        private const string CONFIG_FILE = "config.json";
        private static Configuration? _configuration;

        public static Configuration GetConfiguration()
        {
            if (_configuration == null)
            {
                LoadConfiguration();
            }
            return _configuration!;
        }

        public static void LoadConfiguration()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    var json = File.ReadAllText(CONFIG_FILE);
                    _configuration =
                        JsonSerializer.Deserialize<Configuration>(json) ?? new Configuration();

                    // Handle migration from old plain text API key format
                    MigrateApiKeyIfNeeded();

                    // Ensure SystemPrompts are populated with defaults if missing
                    EnsureSystemPromptsArePopulated();
                }
                else
                {
                    _configuration = new Configuration();
                    EnsureSystemPromptsArePopulated();
                    SaveConfiguration();
                }
            }
            catch
            {
                _configuration = new Configuration();
                EnsureSystemPromptsArePopulated();
            }
        }

        /// <summary>
        /// Ensures that SystemPrompts dictionary contains all default prompts
        /// </summary>
        private static void EnsureSystemPromptsArePopulated()
        {
            if (_configuration != null)
            {
                // Initialize SystemPrompts if null
                if (_configuration.SystemPrompts == null)
                {
                    _configuration.SystemPrompts = Configuration.GetDefaultSystemPrompts();
                    SaveConfiguration();
                    return;
                }

                // Add any missing prompts from defaults
                var defaultPrompts = Configuration.GetDefaultSystemPrompts();
                bool needsSave = false;

                foreach (var kvp in defaultPrompts)
                {
                    if (!_configuration.SystemPrompts.ContainsKey(kvp.Key))
                    {
                        _configuration.SystemPrompts[kvp.Key] = kvp.Value;
                        needsSave = true;
                    }
                }

                if (needsSave)
                {
                    SaveConfiguration();
                }
            }
        }

        /// <summary>
        /// Migrates plain text API keys to encrypted format for existing configurations
        /// </summary>
        private static void MigrateApiKeyIfNeeded()
        {
            if (_configuration != null && !string.IsNullOrEmpty(_configuration.EncryptedApiKey))
            {
                // Check if the stored key is actually plain text (not encrypted)
                if (!PortableEncryptionService.IsEncrypted(_configuration.EncryptedApiKey))
                {
                    // Migrate plain text key to encrypted format
                    var plainTextKey = _configuration.EncryptedApiKey;
                    _configuration.ApiKey = plainTextKey; // This will encrypt it automatically
                    SaveConfiguration(); // Save the migrated configuration
                }
            }
        }

        public static async Task SaveConfigurationAsync()
        {
            if (_configuration != null)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                };
                var json = JsonSerializer.Serialize(_configuration, options);
                await File.WriteAllTextAsync(CONFIG_FILE, json);
            }
        }

        public static void SaveConfiguration()
        {
            if (_configuration != null)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                };
                var json = JsonSerializer.Serialize(_configuration, options);
                File.WriteAllText(CONFIG_FILE, json);
            }
        }

        public static void UpdateConfiguration(Configuration config)
        {
            _configuration = config;
            SaveConfiguration();
        }
    }
}
