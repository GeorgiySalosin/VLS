using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace VLSGame.Rendering.Content3D
{
    /// <summary>
    /// Manages background 3D rendering (panorama/sphere)
    /// </summary>
    public sealed class BackgroundRenderer
    {
        private ModelVisual3D? currentBackground;
        private Viewport3D? viewport;

        public bool IsVisible { get; set; } = true;

        public void Initialize(Viewport3D viewport)
        {
            this.viewport = viewport;
        }

        public void SetBackground(ModelVisual3D backgroundVisual)
        {
            ClearBackground();
            currentBackground = backgroundVisual;
            Render();
        }

        public void ClearBackground()
        {
            if (currentBackground != null && viewport != null && viewport.Children.Contains(currentBackground))
            {
                viewport.Children.Remove(currentBackground);
            }
            currentBackground = null;
        }

        public void Render()
        {
            if (viewport == null || !IsVisible || currentBackground == null) return;

            if (!viewport.Children.Contains(currentBackground))
            {
                viewport.Children.Add(currentBackground);
            }
        }
    }
}