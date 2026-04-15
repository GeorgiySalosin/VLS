using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.Rendering.Content2D;
using VLSGame.Rendering.Content2D.HUD;
using VLSGame.Rendering.Content3D;

namespace VLSGame.Rendering
{
    public sealed class RenderManager
    {
        private static readonly RenderManager instance = new();
        public static RenderManager Instance => instance;

        private readonly SortedDictionary<RenderOrder, Layer> Layers = [];
        private readonly BackgroundRenderer backgroundRenderer = new();
        private Viewport3D? mainViewport;
        private readonly List<ModelVisual3D> lights = [];

        public void Initialize(Viewport3D viewport, Panel hudPanel)
        {
            mainViewport = viewport;
            backgroundRenderer.Initialize(viewport);

            RegisterLayer(new HudLayer(hudPanel));

            SetupLighting();
        }

        public void RegisterLayer(Layer layer)
        {
            if (!Layers.ContainsKey(layer.Order))
            {
                Layers.Add(layer.Order, layer);
            }
        }

        public void SetBackground(ModelVisual3D backgroundVisual)
        {
            backgroundRenderer.SetBackground(backgroundVisual);
        }

        public void ClearBackground()
        {
            backgroundRenderer.ClearBackground();
        }


        public T? GetLayer<T>() where T : Layer
        {
            return Layers.Values.OfType<T>().FirstOrDefault();
        }

        public void Render()
        {
            if (mainViewport == null) return;

            var camera = mainViewport.Camera;
            var lightsToKeep = mainViewport.Children
                .OfType<ModelVisual3D>()
                .Where(v => v.Content is AmbientLight || v.Content is DirectionalLight)
                .ToList();

            mainViewport.Children.Clear();
            mainViewport.Camera = camera;

            foreach (var light in lightsToKeep)
            {
                mainViewport.Children.Add(light);
            }

            backgroundRenderer.Render();

        }

        private void SetupLighting()
        {
            if (mainViewport == null) return;

            var ambientLight = new ModelVisual3D();
            ambientLight.Content = new AmbientLight(Colors.White);
            lights.Add(ambientLight);

            foreach (var lightVisual in lights)
            {
                if (!mainViewport.Children.Contains(lightVisual))
                {
                    mainViewport.Children.Add(lightVisual);
                }
            }
        }
    }
}