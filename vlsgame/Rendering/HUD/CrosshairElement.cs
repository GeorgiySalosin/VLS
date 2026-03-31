using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace VLSGame.Rendering.HUD
{
    public class CrosshairElement : Element
    {
        private Image _crosshairImage;
        
        public CrosshairElement(string name) : base(name)
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