using System.Windows.Controls;
using VLSGame.Rendering.HUD;

namespace VLSGame.Rendering.Layers
{
    /// <summary>
    /// contains a dictionary of elements, implementing control over them. requires a panel on target window
    /// </summary>
    public class HudLayer(Panel parentPanel) : Layer("HUD", RenderOrder.HUD)
    {
        private readonly Panel parentPanel = parentPanel;
        private readonly Dictionary<string, Element> elements = [];

        public override bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;

                if (value)
                {
                    ShowAll();
                }
                else
                {
                    HideAll();
                }
            }
        }

        public void RegisterElement(Element element)
        {
            if (elements.TryAdd(element.Name, element))
            {
                if (element.Visual != null && IsVisible && element.IsVisible)
                {
                    parentPanel.Children.Add(element.Visual);
                }
            }
        }

        public void ShowElement(string name)
        {
            if (elements.TryGetValue(name, out var element))
            {
                element.Show();
                if (IsVisible && element.Visual != null && !parentPanel.Children.Contains(element.Visual))
                {
                    parentPanel.Children.Add(element.Visual);
                }
            }
        }


        public void HideElement(string name)
        {
            if (elements.TryGetValue(name, out var element))
            {
                element.Hide();
                if (element.Visual != null)
                {
                    parentPanel.Children.Remove(element.Visual);
                }
            }
        }


        public void ShowAll()
        {
            foreach (var element in elements.Values)
            {
                element.Show();
            }
        }

        public void HideAll()
        {
            foreach (var element in elements.Values)
            {
                element.Hide();
            }
        }

        //public override void Update(double deltaTime)
        //{
        //    foreach (var element in elements.Values)
        //    {
        //        element.Update(deltaTime);
        //    }
        //}

        // Очистка всех элементов
        public void Clear()
        {
            foreach (var element in elements.Values)
            {
                if (element.Visual != null)
                {
                    parentPanel.Children.Remove(element.Visual);
                }
            }
            elements.Clear();
        }
    }
}