using System.Windows;
using System.Windows.Controls;

namespace VLSGame.Rendering.Content2D
{
    /// <summary>
    /// Base class for all layers that render on panels
    /// </summary>
    public abstract class Layer(string name, RenderOrder order, Panel parentPanel)
    {
        protected readonly Panel parentPanel = parentPanel;

        public string Name { get; } = name;
        public RenderOrder Order { get; } = order;

        public virtual void ShowAll()
        {

        }

        public virtual void HideAll()
        {

        }


    }
}