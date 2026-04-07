namespace VLSShared.Models
{
    public static class BulletManager
    {
        private static List<Bullet> Bullets { get; } = new List<Bullet>();

        public static void UpdateBullets(int tickHz = 100)
        {
            if (Bullets.Count == 0) return;
            foreach (Bullet bullet in Bullets)
            {
                if (bullet.IsLanded) Bullets.Remove(bullet);
                else bullet.Process(tickHz);
            }
        }

        public static void AddBullet(Bullet bullet) => Bullets.Add(bullet); 
    }
}
