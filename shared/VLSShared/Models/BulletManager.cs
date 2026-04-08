namespace VLSShared.Models
{
    public static class BulletManager
    {
        public static event Action<int, int, double, double>? BulletLanded; // (x, y, distance, flightTime)
        private static List<Bullet> Bullets { get; } = new List<Bullet>();
        private static readonly object _lock = new object();

        public static void UpdateBullets(int tickHz = 100)
        {
            lock (_lock)
            {
                double dt = 1.0 / tickHz;
                for (int i = Bullets.Count - 1; i >= 0; i--)
                {
                    Bullet bullet = Bullets[i];
                    if (bullet.IsLanded)
                    {
                        BulletLanded?.Invoke(bullet.X, bullet.Y, bullet.Distance, bullet.FlightTime);
                        Bullets.RemoveAt(i);
                    }
                    else bullet.Update(dt);
                }
            }
        }

        public static void AddBullet(Bullet bullet) { lock (_lock) Bullets.Add(bullet); } 
    }
}
