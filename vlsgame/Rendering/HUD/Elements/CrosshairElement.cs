using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VLSGame.HUD;

namespace VLSGame.Rendering.HUD.Elements
{
    public class CrosshairElement : HudElement
    {
        private Image _crosshairImage;
        
        public CrosshairElement() : base("Crosshair")
        {
            _crosshairImage = new Image
            {
                Width = 192,
                Height = 192,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
                //,RenderOptions.BitmapScalingMode = BitmapScalingMode.HighQuality
            };
            
            var uri = new Uri("pack://application:,,,/Content/ui/T_CrossAIM.png");
            _crosshairImage.Source = new BitmapImage(uri);

            Visual = _crosshairImage;
        }
        
        public override void Update(double deltaTime)
        {
            
        }
        
    }
}