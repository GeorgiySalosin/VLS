using System.Collections.Generic;
using System.Windows.Controls;

namespace VLSGame.Rendering.Content2D
{
    public class TextureLayer(string name, RenderOrder order, Panel parentPanel) : Layer(name, order, parentPanel)
    {
        private readonly Dictionary<string, Texture> textures = [];

        public void RegisterTexture(Texture texture)
        {
            if (textures.TryAdd(texture.Name, texture))
            {
                if (!parentPanel.Children.Contains(texture.Image))
                {
                    parentPanel.Children.Add(texture.Image);
                }
            }
        }

        public void ShowTexture(string name)
        {
            if (textures.TryGetValue(name, out var texture))
            {
                texture.Show();
            }
        }

        public void HideTexture(string name)
        {
            if (textures.TryGetValue(name, out var texture))
            {
                texture.Hide();
            }
        }

        public Texture? GetTexture(string name)
        {
            textures.TryGetValue(name, out var texture);
            return texture;
        }

        public override void ShowAll()
        {
            foreach (var texture in textures.Values)
            {
                texture.Show();
            }
        }

        public override void HideAll()
        {
            foreach (var texture in textures.Values)
            {
                texture.Hide();
            }
        }

        public virtual void Clear()
        {
            foreach (var texture in textures.Values)
            {
                parentPanel.Children.Remove(texture.Image);
            }
            textures.Clear();
        }
    }
}