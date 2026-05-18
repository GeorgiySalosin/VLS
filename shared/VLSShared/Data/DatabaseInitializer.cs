using Microsoft.Extensions.Options;
using VLSShared.Data.Entities;
using VLSShared.Data.Entities.Singleplayer;

namespace VLSShared.Data
{
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Gets the default connection string (the folder with the executable file).
        /// </summary>
        public static string DefaultConnectionString // maybe it should be in AppDbContext
        {
            get
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string dbPath = Path.Combine(baseDirectory, "GameData.db");
                return $"Data Source={dbPath}";
            }
        }

        /// <summary>
        /// Initializes the database using the specified connection string.
        /// </summary>
        public static void Initialize(string connectionString)
        {
            using var context = new AppDbContext(connectionString);
            context.Database.EnsureCreated(); // Creates database and tables if not exists

            // Seed data if tables are empty
            SeedWeather(context);
            SeedSinglePanorama(context);
            // SeedMultiPanorama(context);
            // SeedMatchPanorama(context);

            context.SaveChanges();
        }

        /// <summary>
        /// Initializes the database using the default connection string (the folder with the executable file).
        /// </summary>
        public static void Initialize() =>
            Initialize(DefaultConnectionString);

        #region Seeds
        private static void SeedWeather(AppDbContext context)
        {
            if (!context.Weathers.Any())
            {
                string basePath = Path.Combine("Content", "Lobby"); // need to test
                context.Weathers.AddRange(
                    new Weather { Title = "Sunny", Description = "common map", PreviewPath = Path.Combine(basePath, "T_MapPreview_Sun.png") },
                    new Weather { Title = "Foggy", Description = "for nature lovers", PreviewPath = Path.Combine(basePath, "T_MapPreview_Fog.png") },
                    new Weather { Title = "Sunset", Description = "prove your skill!", PreviewPath = Path.Combine(basePath, "T_MapPreview_Sunset.png") }
                    );
                context.SaveChanges();
            }
        }

        private static void SeedSinglePanorama(AppDbContext context)
        {
            if (!context.SinglePanoramas.Any())
            {
                context.SinglePanoramas.Add(
                    new SinglePanorama { Name = "W001" }
                    );
                context.SaveChanges();
            }
        }

        private static void SeedMultiPanorama(AppDbContext context)
        {
            if (!context.MultiPanoramas.Any())
            {
                // todo
                context.SaveChanges();
            }
        }

        private static void SeedMatchPanorama(AppDbContext context)
        {
            if (!context.MatchPanoramas.Any())
            {
                // todo
                context.SaveChanges();
            }
        }

        #endregion
    }
}
