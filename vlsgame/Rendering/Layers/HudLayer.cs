using System.Windows;
using System.Windows.Controls;

namespace VLSGame.Rendering.Layers
{
    public class HudLayer : IRenderLayer
    {
        private readonly Panel _parentPanel;
        private readonly List<UIElement> _hudElements = new();
        
        public string Name => "HUD";
        public RenderOrder Order => RenderOrder.HUD;
        public bool IsVisible { get; set; } = true;
        
        public HudLayer(Panel parentPanel)
        {
            _parentPanel = parentPanel;
        }
        
        public void AddElement(UIElement element)
        {
            _hudElements.Add(element);
            if (IsVisible)
            {
                _parentPanel.Children.Add(element);
            }
        }
        
        public void RemoveElement(UIElement element)
        {
            _hudElements.Remove(element);
            _parentPanel.Children.Remove(element);
        }
        
        public void Update(double deltaTime)
        {
            // Обновление HUD элементов
        }
        
        public void Render(Viewport3D viewport)
        {
            // HUD рендерится через WPF, не через Viewport3D
        }
        
        public void Show()
        {
            IsVisible = true;
            foreach (var element in _hudElements)
            {
                if (!_parentPanel.Children.Contains(element))
                {
                    _parentPanel.Children.Add(element);
                }
            }
        }
        
        public void Hide()
        {
            IsVisible = false;
            foreach (var element in _hudElements)
            {
                _parentPanel.Children.Remove(element);
            }
        }
        
        public void Clear()
        {
            foreach (var element in _hudElements)
            {
                _parentPanel.Children.Remove(element);
            }
            _hudElements.Clear();
        }
    }
}