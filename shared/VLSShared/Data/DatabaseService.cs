using VLSShared.Data.Entities;

namespace VLSShared.Data
{
    public static class DatabaseService
    {
        public static List<Weather> GetAllWeathers()
        {
            using var context = new AppDbContext(DatabaseInitializer.DefaultConnectionString);
            return context.Weathers.ToList();
        }
    }
}
