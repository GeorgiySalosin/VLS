using System.Windows;
using System.Windows.Media;

namespace VLSGame.Rendering.Content2D.HUD
{
    public class CrosshairTexture : Texture
    {
        public CrosshairTexture() : base("Crosshair")
        {
            LoadFromFile("pack://application:,,,/Content/ui/T_CrossAIM.png");
        }

    }
}