using System.IO;
using VLSGame.Config.SingleplayerSpawn;

namespace VLSGame.Config
{
    internal class TargetsConfig : JsonConfigBase<TargetsSettings>
    {
        private static readonly string configPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            @"Config\Targets.json"
        );
        private static TargetsConfig instance;
        private static readonly object _lock = new object();

        private TargetsConfig() : base(configPath) { }

        internal static TargetsConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                            instance = new TargetsConfig();
                    }
                }
                return instance;
            }
        }
    }
}