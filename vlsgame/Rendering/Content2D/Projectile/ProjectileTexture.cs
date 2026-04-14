using System.Windows;

namespace VLSGame.Rendering.Content2D.Projectile
{
    public class ProjectileTexture : Texture
    {
        public ProjectileTexture(string name) : base(name)
        {
            LoadFromFile("pack://application:,,,/Content/Animation/BallisticsFX/BulletDebug.png");

        }
    }
}