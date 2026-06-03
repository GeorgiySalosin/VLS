using System.IO;

namespace VLSGame.Config.GameConfig
{
    internal class Configuration : JsonConfigBase<GameSettings>
    {
        private static readonly string configPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            @"Config\GameSettings.json"
        );
        private static Configuration instance;
        private static readonly object _lock = new object();

        private Configuration() : base(configPath) { }

        internal static Configuration Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                            instance = new Configuration();
                    }
                }
                return instance;
            }
        }

        internal CameraAnimationSettings CameraAnimationSettings { get; private set; } = new CameraAnimationSettings();
    }
}
