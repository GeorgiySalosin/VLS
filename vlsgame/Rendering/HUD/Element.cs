using System.Windows;
using System.Windows.Controls;

namespace VLSGame.HUD
{
    public abstract class Element
    {
        public string Name { get; protected set; }
        public UIElement? Visual { get; protected set; }
        public bool IsVisible { get; set; } = true;
        
        protected Element(string name)
        {
            Name = name;
        }
        
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