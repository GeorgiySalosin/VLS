using Microsoft.EntityFrameworkCore;
using VLSShared.Data.Entities;
using VLSShared.Data.Entities.Multiplayer;
using VLSShared.Data.Entities.Singleplayer;

namespace VLSShared.Data
{
    public class AppDbContext : DbContext
    {
        private readonly string connectionString;

        #region Tables
        public DbSet<Weather> Weathers { get; set; }
        public DbSet<MultiPanorama> MultiPanoramas { get; set; }
        public DbSet<MatchPanorama> MatchPanoramas { get; set; }
        public DbSet<SinglePanorama> SinglePanoramas { get; set; }
        public DbSet<SingleTargets> SingleTargets { get; set; }
        #endregion

        public AppDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relationships

            // MatchPanorama → Panorama1
            modelBuilder.Entity<MatchPanorama>()
                .HasOne(m => m.Panorama1)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // MatchPanorama → Panorama2
            modelBuilder.Entity<MatchPanorama>()
                .HasOne(m => m.Panorama2)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // SingleTargets → SinglePanorama
            modelBuilder.Entity<SingleTargets>()
                .HasOne(st => st.SinglePanorama)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
