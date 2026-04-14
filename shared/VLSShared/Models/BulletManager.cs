namespace VLSShared.Models
{
    public static class BulletManager
    {
        public static event Action<Bullet>? BulletAdded;     // ADDED 
        public static event Action<Bullet>? BulletLanded;    // CHANGED !!!!    // deprecated (x, y, distance, flightTime)
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
                        BulletLanded?.Invoke(bullet);  // CHANGED: send the whole object bullet
                        Bullets.RemoveAt(i);           // CHANDED: remove by idx
                    }
                    else bullet.Process(dt);
                }
            }
        }

        public static void AddBullet(Bullet bullet)
        {
            BulletAdded?.Invoke(bullet);    // ADDED 
            Bullets.Add(bullet);
        }
    }
}
