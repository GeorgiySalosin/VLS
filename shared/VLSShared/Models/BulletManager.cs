using System.Numerics;

namespace VLSShared.Models
{
    public static class BulletManager
    {
        public static event Action<int, int, double, double>? BulletLanded; // (x, y, distance, flightTime)


        public static event Action<Guid>? BulletRemoved;                // notifies viewmodel that it should unload related 3d model
        public static event Action<Guid, Vector3>? BulletUpdated;       // notifies viewmodel that it should transform related 3d model with new direction
        public static event Action<Guid, Vector3>? BulletCreated;       // notifies viewmodel that it should create a new 3d object assigned to bullet

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
                        BulletRemoved?.Invoke(bullet.Id);
                        Bullets.RemoveAt(i);
                    }
                    else
                    {
                        bullet.Update(dt);
                        BulletUpdated?.Invoke(bullet.Id, bullet.Direction);
                    }
                }
            }
        }

        public static void AddBullet(Bullet bullet)
        {
            lock (_lock)
            {
                Bullets.Add(bullet);
                BulletCreated?.Invoke(bullet.Id, bullet.Direction);

                //var model = RenderManager.Instance.CreateBulletObject3D(bullet.Id);
                //model.UpdateOrbit(bullet.Direction);        // initial transform rotation
                //RenderManager.Instance.Add3D(model);

            }
        }
    }
}