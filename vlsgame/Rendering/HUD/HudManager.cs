using VLSGame.HUD;

namespace VLSGame.Rendering.Layers
{
    public class HudManager
    {
        private readonly HudLayer _hudLayer;
        private readonly Dictionary<string, HudElement> _elements = new();
        
        public HudManager(HudLayer hudLayer)
        {
            _hudLayer = hudLayer;
        }
        
        public void RegisterElement(HudElement element)
        {
            if (!_elements.ContainsKey(element.Name))
            {
                _elements.Add(element.Name, element);
                if (element.Visual != null)
                {
                    _hudLayer.AddElement(element.Visual);
                }
            }
        }
        
        public T? GetElement<T>(string name) where T : HudElement
        {
            return _elements.TryGetValue(name, out var element) ? element as T : null;
        }
        
        public void Update(double deltaTime)
        {
            foreach (var element in _elements.Values)
            {
                element.Update(deltaTime);
            }
        }
        
        public void ShowElement(string name)
        {
            if (_elements.TryGetValue(name, out var element))
            {
                element.Show();
            }
        }
        
        public void HideElement(string name)
        {
            if (_elements.TryGetValue(name, out var element))
            {
                element.Hide();
            }
        }
        
        public void ShowAll()
        {
            foreach (var element in _elements.Values)
            {
                element.Show();
            }
            _hudLayer.Show();
        }
        
        public void HideAll()
        {
            foreach (var element in _elements.Values)
            {
                element.Hide();
            }
            _hudLayer.Hide();
        }
    }
}