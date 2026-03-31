using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.Rendering.Layers;

namespace VLSGame.Rendering
{
    public sealed class RenderManager
    {
        private static readonly RenderManager instance = new();
        public static RenderManager Instance => instance;

        private readonly SortedDictionary<RenderOrder, Layer> layers = []; 
        private Viewport3D? mainViewport;
        private readonly List<ModelVisual3D> lights = [];

        // this is used directly from the match view as viewmodel
        public void Initialize(Viewport3D viewport, Panel hudPanel)
        {
            mainViewport = viewport;

            // REGISTRATING NEW LAYERS THERE
            RegisterLayer(new BackgroundLayer());
            RegisterLayer(new HudLayer(hudPanel));

            SetupLighting();
        }

        public void RegisterLayer(Layer layer)  // ← gets abstract class
        {
            if (!layers.ContainsKey(layer.Order))
            {
                layers.Add(layer.Order, layer);
            }
        }

        public T? GetLayer<T>() where T : Layer  // able to get any layer though they are different classes
        {
            return layers.Values.OfType<T>().FirstOrDefault();
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

            foreach (var layer in layers.Values)
            {
                layer.Render(mainViewport);
            }
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

        /*this could be used later*/
        public void SetLayerVisibility<T>(bool visible) where T : Layer
        {
            var layer = GetLayer<T>();
            if (layer != null)
            {
                layer.IsVisible = visible;
            }
        }
    }
}