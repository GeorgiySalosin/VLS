namespace VLSShared.Models
{
    public static class BulletManager
    {
        public static event Action<int, int, double, double>? BulletLanded; // (x, y, distance, flightTime)
        private static List<Bullet> Bullets { get; } = new List<Bullet>();

        public static void UpdateBullets(int tickHz = 100)
        {
            double dt = 1.0 / tickHz;
            for (int i = 0; i < Bullets.Count; i++)
            {
                Bullet bullet = Bullets[i];
                if (bullet != null) // maybe useless
                {
                    if (bullet.IsLanded)
                    {
                        BulletLanded?.Invoke(bullet.X, bullet.Y, bullet.Distance, bullet.FlightTime);
                        Bullets.Remove(bullet);
                    }
                    else bullet.Process(dt);
                }
            }
        }

        public static void AddBullet(Bullet bullet) => Bullets.Add(bullet); 
    }
}
