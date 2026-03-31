using System.Windows;
using System.Windows.Controls;

namespace VLSGame.Rendering.HUD
{
    public abstract class Element(string name)
    {
        public string Name { get; protected set; } = name;
        public UIElement? Visual { get; protected set; }
        public bool IsVisible { get; set; } = true;

        public abstract void Update(double deltaTime);
        
        public virtual void Show()
        {
            IsVisible = true;
            if (Visual != null)
            {
                Visual.Visibility = Visibility.Visible;
            }
        }
        
        public virtual void Hide()
        {
            IsVisible = false;
            if (Visual != null)
            {
                Visual.Visibility = Visibility.Collapsed;
            }
        }
    }
}