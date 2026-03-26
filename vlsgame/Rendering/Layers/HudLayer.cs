using System.Windows;
using System.Windows.Controls;
using VLSGame.HUD;

namespace VLSGame.Rendering.Layers
{
    /*contains a panel and a list of elements. also implements control over elements*/
    public class HudLayer : Layer
    {
        private readonly Panel _parentPanel;
        private readonly Dictionary<string, Element> _elements = new();

        public HudLayer(Panel parentPanel)
            : base("HUD", RenderOrder.HUD)
        {
            _parentPanel = parentPanel;
        }

        public void RegisterElement(Element element)
        {
            if (!_elements.ContainsKey(element.Name))
            {
                _elements.Add(element.Name, element);
                if (element.Visual != null && IsVisible && element.IsVisible)
                {
                    _parentPanel.Children.Add(element.Visual);
                }
            }
        }

        public void ShowElement(string name)
        {
            if (_elements.TryGetValue(name, out var element))
            {
                element.Show();
                if (IsVisible && element.Visual != null && !_parentPanel.Children.Contains(element.Visual))
                {
                    _parentPanel.Children.Add(element.Visual);
                }
            }
        }


        public void HideElement(string name)
        {
            if (_elements.TryGetValue(name, out var element))
            {
                element.Hide();
                if (element.Visual != null)
                {
                    _parentPanel.Children.Remove(element.Visual);
                }
            }
        }


        public void ShowAll()
        {
            foreach (var element in _elements.Values)
            {
                element.Show();
            }
        }

        public void HideAll()
        {
            foreach (var element in _elements.Values)
            {
                element.Hide();
            }
        }

        //public override void Update(double deltaTime)
        //{
        //    foreach (var element in _elements.Values)
        //    {
        //        element.Update(deltaTime);
        //    }
        //}

        // Очистка всех элементов
        public void Clear()
        {
            foreach (var element in _elements.Values)
            {
                if (element.Visual != null)
                {
                    _parentPanel.Children.Remove(element.Visual);
                }
            }
            _elements.Clear();
        }
    }
}