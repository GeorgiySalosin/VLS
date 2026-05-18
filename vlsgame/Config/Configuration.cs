using System.IO;
using System.Text.Json;

namespace VLSGame.Config
{
    public class Configuration
    {
        private static Configuration _instance;
        private static readonly object _lock = new object();
        private static readonly string configPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            @"Config\GameSettings.json"
        );

        public GameSettings GameSettings { get; private set; }
        public CameraAnimationSettings CameraAnimationSettings { get; private set; } = new CameraAnimationSettings();

        private Configuration()
        {
            GameSettings = null;
        }

        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Configuration();
                        }
                    }
                }
                return _instance;
            }
        }

        public bool LoadConfiguration()
        {
            try
            {
                string directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (!File.Exists(configPath))
                {
                    // Creating default settings
                    GameSettings = new GameSettings(); // uses default values from properties
                    SaveConfiguration(); // saving the created file
                    return true;
                }

                string jsonContent = File.ReadAllText(configPath);
                var settings = JsonSerializer.Deserialize<GameSettings>(jsonContent);

                if (settings == null)
                {
                    throw new InvalidOperationException("Invalid Json configuration");
                }

                GameSettings = settings;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Json configuration: {ex.Message}");
                GameSettings = null;
                return false;
            }
        }

        public bool SaveConfiguration()
        {
            try
            {
                if (GameSettings == null)
                    return false;

                string directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string jsonContent = JsonSerializer.Serialize(GameSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, jsonContent);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
                return false;
            }
        }


    }
}
