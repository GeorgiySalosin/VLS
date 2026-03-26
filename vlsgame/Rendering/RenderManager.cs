using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.Rendering.Layers;

namespace VLSGame.Rendering
{
    public sealed class RenderManager
    {
        private static readonly RenderManager _instance = new();
        public static RenderManager Instance => _instance;

        private readonly SortedDictionary<RenderOrder, Layer> _layers; 
        private Viewport3D? _mainViewport;
        private DateTime _lastUpdate;
        private List<ModelVisual3D> _lights = new();

        private RenderManager()
        {
            _layers = new SortedDictionary<RenderOrder,Layer>();
        }

        // this is used directly from the match view as viewmodel
        public void Initialize(Viewport3D viewport, Panel hudPanel)
        {
            _mainViewport = viewport;

            // REGISTRATING NEW LAYERS THERE
            RegisterLayer(new BackgroundLayer());
            RegisterLayer(new HudLayer(hudPanel));

            SetupLighting();
            _lastUpdate = DateTime.Now;
        }

        public void RegisterLayer(Layer layer)  // ← gets abstract class
        {
            if (!_layers.ContainsKey(layer.Order))
            {
                _layers.Add(layer.Order, layer);
            }
        }

        public T? GetLayer<T>() where T : Layer  // able to get any layer though they are different classes
        {
            return _layers.Values.OfType<T>().FirstOrDefault();
        }


        public void Render()
        {
            if (_mainViewport == null) return;

            var camera = _mainViewport.Camera;
            var lightsToKeep = _mainViewport.Children
                .OfType<ModelVisual3D>()
                .Where(v => v.Content is AmbientLight || v.Content is DirectionalLight)
                .ToList();

            _mainViewport.Children.Clear();
            _mainViewport.Camera = camera;

            foreach (var light in lightsToKeep)
            {
                _mainViewport.Children.Add(light);
            }

            foreach (var layer in _layers.Values)
            {
                layer.Render(_mainViewport);
            }
        }

        private void SetupLighting()
        {
            if (_mainViewport == null) return;

            var ambientLight = new ModelVisual3D();
            ambientLight.Content = new AmbientLight(Colors.White);
            _lights.Add(ambientLight);

            foreach (var lightVisual in _lights)
            {
                if (!_mainViewport.Children.Contains(lightVisual))
                {
                    _mainViewport.Children.Add(lightVisual);
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