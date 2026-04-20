using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace VLSGame.Rendering.Content3D
{
    /// <summary>
    /// Contains all methods that return DiffuseMaterial object reference.
    /// </summary>
    static class Material
    {

        public static DiffuseMaterial TextureMaterial(ImageSource? texture)
        {
            var brush = new ImageBrush(texture)
            {
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.None,
                Stretch = Stretch.Fill
            };

            return new DiffuseMaterial(brush);
        }


        /// <summary>
        ///     Creates solid color material from separate per-pixel values. 
        /// </summary>
        public static DiffuseMaterial RGBAMaterial(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            var brush = new SolidColorBrush()
            {
                Color = Color.FromArgb(a, r, g, b)
            };

            return new DiffuseMaterial(brush);
        }

};
}
