namespace VLSShared.Models
{
    public static class BulletManager
    {
        private static Bullet? LastBullet; // последняя добавленная пуля
        public static event Action<string>? LastBulletInfoChanged;
        public static string LastBulletInfo => LastBullet?.ToString() ?? "No active bullets";

        private static List<Bullet> Bullets { get; } = new List<Bullet>();
        private static readonly object _lock = new object();

        public static void UpdateBullets(float dt)
        {
            lock (_lock)
            {
                for (int i = Bullets.Count - 1; i >= 0; i--)
                {
                    Bullet bullet = Bullets[i];
                    
                    if (bullet.IsLanded) Bullets.RemoveAt(i);
                    else bullet.Update(dt);

                    if (bullet == LastBullet) LastBulletInfoChanged?.Invoke(LastBulletInfo);
                }
            }
        }

        public static void AddBullet(Bullet bullet) 
        {
            lock (_lock)
            {
                Bullets.Add(bullet);
                LastBullet = bullet;
                LastBulletInfoChanged?.Invoke(LastBulletInfo);
            }
        } 
    }
}
