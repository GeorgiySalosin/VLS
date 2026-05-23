using System;
using System.Windows.Media;

namespace VLSGame.Rendering.Content2D
{
    public sealed class CustomObject2D(ImageSource texture, Guid id = default, string tag = "")
    {
        public Guid Id { get; } = id == default ? Guid.NewGuid() : id;
        public string Tag { get; set; } = tag;
        public ImageSource Texture { get; set; } = texture;
        public double X { get; set; }
        public double Y { get; set; }
        public double Scale { get; set; } = 1.0;
        public bool IsVisible { get; set; } = true;
    }
}