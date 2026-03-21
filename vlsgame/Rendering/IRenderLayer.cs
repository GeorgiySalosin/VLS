using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace VLSGame.Rendering
{

    
    public interface IRenderLayer
    {
        string Name { get; }
        RenderOrder Order { get; }
        bool IsVisible { get; set; }
        
        void Render(Viewport3D viewport);
        void Update(double deltaTime);
    }
}