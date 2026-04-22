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

                ValidateSettings(settings);
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

        private void ValidateSettings(GameSettings settings)
        {
            if (settings.MouseSensitivity <= 0)
                throw new InvalidOperationException("mouse_sensitivity must be greater 0");

            if (settings.SpeedBufferSize <= 0)
                throw new InvalidOperationException("speed_buffer_size  must be greater 0");

            if (settings.MinSpeedThreshold < 0)
                throw new InvalidOperationException("min_speed_threshold  must be greater 0");

            if (settings.MaxSpeedThreshold <= settings.MinSpeedThreshold)
                throw new InvalidOperationException("max_speed_threshold  must be greater min_speed_threshold");

            if (settings.MinSensitivityScale <= 0 || settings.MinSensitivityScale > 1)
                throw new InvalidOperationException("min_sensitivity_scale  must be in range (0, 1]");

            if (settings.ZoomSpeed <= 0)
                throw new InvalidOperationException("zoom_speed  must be greater 0");

            if (settings.MinFOV <= 0 || settings.MinFOV >= settings.MaxFOV)
                throw new InvalidOperationException("min_fov must be greater 0 and smaller max_fov");

            if (settings.MaxFOV <= settings.MinFOV)
                throw new InvalidOperationException("max_fov  must be greater больше min_fov");
        }
    }
}
