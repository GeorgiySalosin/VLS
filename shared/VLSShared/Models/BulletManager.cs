using VLSGame.Rendering;

namespace VLSShared.Models
{
    public static class BulletManager
    {
        public static event Action<int, int, double, double>? BulletLanded; // (x, y, distance, flightTime)
        public static event Action<Guid>? BulletCreated;  // NEW
        public static event Action<Guid>? BulletRemoved;    // NEW


        public static List<Bullet> Bullets { get; } = [];
        private static readonly object _lock = new();

        public static void UpdateBullets(int tickHz)
        {
            lock (_lock)
            {
                float dt = 1.0f / tickHz;
                for (int i = Bullets.Count - 1; i >= 0; i--)
                {
                    Bullet bullet = Bullets[i];
                    if (bullet.IsLanded)
                    {
                        BulletLanded?.Invoke(bullet.X, bullet.Y, bullet.Distance, bullet.FlightTime);
                        RenderManager.Instance.Remove3D(bullet.Id);

                        Bullets.RemoveAt(i);
                    }
                    else
                    {
                        bullet.Update(dt);
                    }
                }
            }
        }

        public static void AddBullet(Bullet bullet)
        {
            lock (_lock)
            {
                Bullets.Add(bullet);
                RenderManager.Instance.Add3D(RenderManager.Instance.CreateBulletObject3D(bullet.Id));
            }
        }
    }
}