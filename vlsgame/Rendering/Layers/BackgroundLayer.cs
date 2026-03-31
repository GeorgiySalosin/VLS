using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace VLSGame.Rendering.Layers
{
    public class BackgroundLayer : Layer
    {
        private ModelVisual3D? sphereVisual;
        
        public BackgroundLayer() : base("Background", RenderOrder.Background)
        {
        }
        
        public void SetPanorama(Viewport3D viewport, ModelVisual3D sphereVisual)
        {
            ClearPanorama(viewport);
            this.sphereVisual = sphereVisual;
        }
        
        public override void Render(Viewport3D viewport)
        {
            if (!IsVisible || sphereVisual == null) return;
            
            if (!viewport.Children.Contains(sphereVisual))
            {
                viewport.Children.Add(sphereVisual);
            }
        }
        
        private void ClearPanorama(Viewport3D viewport)
        {
            if (sphereVisual != null && viewport.Children.Contains(sphereVisual))
            {
                viewport.Children.Remove(sphereVisual);
            }
        }
    }
}