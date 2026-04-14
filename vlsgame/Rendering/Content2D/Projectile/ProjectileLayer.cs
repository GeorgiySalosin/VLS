using System.Windows.Controls;
using VLSShared.Models;

namespace VLSGame.Rendering.Content2D.Projectile
{
    public class ProjectileLayer : TextureLayer
    {
        private readonly Dictionary<Guid, ProjectileTexture> projectiles = new();

        public ProjectileLayer(Panel parentPanel)
            : base("Projectile", RenderOrder.Projectile, parentPanel)
        {
            BulletManager.BulletAdded += OnBulletAdded;
            BulletManager.BulletLanded += OnBulletLanded;
        }

        private void OnBulletAdded(Bullet bullet)
        {
            var projectile = new ProjectileTexture($"Projectile_{bullet.Id}");
            RegisterTexture(projectile);
            projectile.Show();
            projectiles[bullet.Id] = projectile;
        }

        private void OnBulletLanded(Bullet bullet)
        {
            if (projectiles.TryGetValue(bullet.Id, out var projectile))
            {
                projectile.Hide();
                projectiles.Remove(bullet.Id);
            }
        }
        //public void ShowProjectile() => ShowTexture("DefaultProjectile");
        //public void HideProjectile() => HideTexture("DefaultProjectile");

        //public void MoveProjectile(double x, double y) => Projectile?.Move(x, y);

        //public void ScaleProjectile(double scale) => Projectile?.SetScale(scale);

        public override void Clear()
        {
            BulletManager.BulletAdded -= OnBulletAdded;
            BulletManager.BulletLanded -= OnBulletLanded;
            base.Clear();
        }
    }

}