using System;
using System.Windows.Media;

namespace VLSGame.Rendering.Content2D
{
    public sealed class CustomObject2D(ImageSource texture, Guid id = default, string tag = "")
    {
        /// <summary> An indentifier </summary>
        public Guid Id { get; } = id == default ? Guid.NewGuid() : id;

        /// <summary> A tag according to which we consider how to render/what to do with the object </summary>
        public string Tag { get; set; } = tag;

        /// <summary> A texture that will be rendered onto wpf window panel </summary>
        public ImageSource Texture { get; set; } = texture;

        /// <summary> A value that represents texture center horizontal offset from screen center </summary>
        public double X { get; set; }

        /// <summary> A value that represents texture center vertical offset from screen center </summary>
        public double Y { get; set; }

        /// <summary> A value that represents texture scale </summary>
        public double Scale { get; set; } = 1.0;

        /// <summary>
        /// A property that results in realtime visibility toggle
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// An additional property for easy texture switch per frame
        /// </summary>
        public Animation Animation { get; } = new();
    }
}