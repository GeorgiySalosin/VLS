using System.Windows.Controls;

namespace VLSGame.Rendering.Layers
{
    /// <summary>
    /// Base class for layers
    /// </summary>
    public abstract class Layer(string name, RenderOrder order)
    {
        private bool isVisible = true;

        public string Name { get; } = name;
        public RenderOrder Order { get; } = order;
        public virtual bool IsVisible
        {
            get => isVisible;
            set => isVisible = value;
        }

        public virtual void Render(Viewport3D viewport) // why not abstract?
        {
            /*Implement in inherited class if it has to be rendered in the viewport*/
        }
    }
}