using System.IO;
using System.Text.Json;

namespace VLSGame.Config
{
    internal abstract class JsonConfigBase<T> where T : class, new()
    {
        private readonly string configPath;
        private T settings;

        protected JsonConfigBase(string configPath)
        {
            this.configPath = configPath;
            settings = new T();
        }

        internal T Settings => settings;

        internal virtual bool Load()
        {
            try
            {
                string directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (!File.Exists(configPath))
                {
                    CreateDefaultSettings();
                    Save();
                    return true;
                }

                string jsonContent = File.ReadAllText(configPath);
                var settings = JsonSerializer.Deserialize<T>(jsonContent);
                if (settings == null)
                    throw new InvalidOperationException($"Invalid JSON: {configPath}");

                this.settings = settings;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading {configPath}: {ex.Message}");
                settings = new T();
                return false;
            }
        }

        public virtual bool Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string jsonContent = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, jsonContent);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving {configPath}: {ex.Message}");
                return false;
            }
        }

        protected virtual void CreateDefaultSettings()
        {
            settings = new T();
        }
    }
}