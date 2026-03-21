using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace VLSGame.Rendering.Layers
{
    public class BackgroundLayer : RenderLayer
    {
        private ModelVisual3D? _sphereVisual;
        
        public BackgroundLayer() : base("Background", RenderOrder.Background)
        {
        }
        
        public void SetPanorama(ModelVisual3D sphereVisual)
        {
            _sphereVisual = sphereVisual;
        }
        
        public override void Render(Viewport3D viewport)
        {
            if (!IsVisible || _sphereVisual == null) return;
            
            if (!viewport.Children.Contains(_sphereVisual))
            {
                viewport.Children.Add(_sphereVisual);
            }
        }
        
        public void ClearPanorama(Viewport3D viewport)
        {
            if (_sphereVisual != null && viewport.Children.Contains(_sphereVisual))
            {
                viewport.Children.Remove(_sphereVisual);
            }
        }
    }
}