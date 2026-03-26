using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace VLSGame.Rendering.Layers
{
    /* base class for layers*/
    public abstract class Layer
    {
        private bool _isVisible = true;

        public string Name { get; }
        public RenderOrder Order { get; }
        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        protected Layer(string name, RenderOrder order)
        {
            Name = name;
            Order = order;
        }


        public virtual void Render(Viewport3D viewport)
        {
            /*Implement in inherited class if it has to be rendered in the viewport*/
        }
    }
}