using System.Windows.Media.Media3D;

namespace VLSGame.Rendering
{
    public interface IRenderable
    {
        string Id { get; }
        bool IsVisible { get; set; }
        Model3D? Model { get; }
        
        void Update(double deltaTime);
        void Render();
    }
}