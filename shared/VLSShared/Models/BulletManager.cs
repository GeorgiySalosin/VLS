namespace VLSShared.Models
{
    public static class BulletManager
    {
        private static List<Bullet> Bullets { get; } = new List<Bullet>();

        public static void UpdateBullets(int tickHz = 100)
        {
            for (int i = 0; i < Bullets.Count; i++)
            {
                Bullet bullet = Bullets[i];
                if (bullet != null)
                {
                    if (bullet.IsLanded) Bullets.Remove(bullet);
                    else bullet.Process(tickHz);
                }
            }
        }

        public static void AddBullet(Bullet bullet) => Bullets.Add(bullet); 
    }
}
