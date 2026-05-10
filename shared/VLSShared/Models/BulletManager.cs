using System.Numerics;

namespace VLSShared.Models
{
    public static class BulletManager
    {
        private static Bullet? LastBullet; // последняя добавленная пуля
        public static event Action<string>? LastBulletInfoChanged;
        public static string LastBulletInfo => LastBullet?.ToString() ?? "No active bullets";

        public static event Action<double, double>? BulletLanded; // (x, y, distance, flightTime)


        public static event Action<Guid>? BulletRemoved;                // notifies viewmodel that it should unload related 3d model
        public static event Action<Guid, Vector3>? BulletUpdated; // id, position
        public static event Action<Guid, Vector3>? BulletCreated;       // notifies viewmodel that it should create a new 3d object assigned to bullet


        private static List<Bullet> Bullets { get; } = [];
        private static readonly object _lock = new();



        public static void UpdateBullets(float dt)
        {
            lock (_lock)
            {
                for (int i = Bullets.Count - 1; i >= 0; i--)
                {
                    Bullet bullet = Bullets[i];

                    if (bullet.IsLanded)
                    {
                        BulletRemoved?.Invoke(bullet.Id);
                        Bullets.RemoveAt(i);
                        continue;
                    }

                    bullet.Update(dt);
                    PlayerManager.CheckBulletCollision(bullet);

                    if (bullet.IsLanded)
                    {
                        BulletRemoved?.Invoke(bullet.Id);
                        Bullets.RemoveAt(i);
                    }
                    else
                    {
                        BulletUpdated?.Invoke(bullet.Id, bullet.Position);
                    }

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
                BulletCreated?.Invoke(bullet.Id, bullet.Direction);
            }
        }
    }
}