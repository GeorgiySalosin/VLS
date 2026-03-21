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

        private readonly SortedDictionary<RenderOrder, IRenderLayer> _layers;
        private Viewport3D? _mainViewport;
        private DateTime _lastUpdate;
        private List<ModelVisual3D> _lights = new(); // Храним освещение отдельно

        private RenderManager()
        {
            _layers = new SortedDictionary<RenderOrder, IRenderLayer>();
        }

        public void Initialize(Viewport3D viewport, Panel hudPanel)
        {
            _mainViewport = viewport;

            // Регистрируем слои в правильном порядке
            RegisterLayer(new BackgroundLayer());
            //RegisterLayer(new CharacterLayer());
            //RegisterLayer(new ProjectileLayer());
            //RegisterLayer(new EffectsLayer());
            RegisterLayer(new HudLayer(hudPanel));

            // Создаем и добавляем освещение
            SetupLighting();

            _lastUpdate = DateTime.Now;
        }

        private void SetupLighting()
        {
            if (_mainViewport == null) return;

            // Ambient light
            var ambientLight = new ModelVisual3D();
            ambientLight.Content = new AmbientLight(Colors.White);
            _lights.Add(ambientLight);

            // Optional: Directional light for better shading
            var directionalLight = new ModelVisual3D();
            var light = new DirectionalLight(Colors.White, new Vector3D(0, -1, -0.5));
            directionalLight.Content = light;
            _lights.Add(directionalLight);

            foreach (var lightVisual in _lights)
            {
                if (!_mainViewport.Children.Contains(lightVisual))
                {
                    _mainViewport.Children.Add(lightVisual);
                }
            }
        }

        public void RegisterLayer(IRenderLayer layer)
        {
            if (!_layers.ContainsKey(layer.Order))
            {
                _layers.Add(layer.Order, layer);
            }
        }

        public T? GetLayer<T>() where T : IRenderLayer
        {
            return _layers.Values.OfType<T>().FirstOrDefault();
        }

        public IRenderLayer? GetLayer(RenderOrder depth)
        {
            return _layers.GetValueOrDefault(depth);
        }

        public void Update()
        {
            var now = DateTime.Now;
            var deltaTime = (now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;

            foreach (var layer in _layers.Values)
            {
                layer.Update(deltaTime);
            }
        }

        public void Render()
        {
            if (_mainViewport == null) return;

            // Сохраняем камеру
            var camera = _mainViewport.Camera;

            // Сохраняем освещение
            var lightsToKeep = _mainViewport.Children
                .OfType<ModelVisual3D>()
                .Where(v => v.Content is AmbientLight || v.Content is DirectionalLight)
                .ToList();

            // Очищаем Viewport от всего, кроме освещения
            _mainViewport.Children.Clear();
            _mainViewport.Camera = camera;

            // Добавляем освещение обратно
            foreach (var light in lightsToKeep)
            {
                _mainViewport.Children.Add(light);
            }

            // Рендерим слои в порядке возрастания глубины
            foreach (var layer in _layers.Values)
            {
                layer.Render(_mainViewport);
            }
        }

        public void SetLayerVisibility<T>(bool visible) where T : IRenderLayer
        {
            var layer = GetLayer<T>();
            if (layer != null)
            {
                layer.IsVisible = visible;
            }
        }
    }
}