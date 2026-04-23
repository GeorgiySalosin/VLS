using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using VLSGame.Rendering.Content2D;

namespace VLSGame.Models
{
    public sealed class MatchTexturePool
    {
        private static readonly Random rnd = new Random();

        readonly ImageBrush T_Tracer_Common01 = LoadTexture(@"Content\Animation\BallisticsFX\CommonTracer\T_Tracer_Common01.png");
        readonly ImageBrush T_Tracer_Common02 = LoadTexture(@"Content\Animation\BallisticsFX\CommonTracer\T_Tracer_Common02.png");
        readonly ImageBrush T_Tracer_Common03 = LoadTexture(@"Content\Animation\BallisticsFX\CommonTracer\T_Tracer_Common03.png");
        readonly ImageBrush T_Tracer_Common04 = LoadTexture(@"Content\Animation\BallisticsFX\CommonTracer\T_Tracer_Common04.png");


        public ImageBrush GetBulletTexture()
        {
            return rnd.Next(4) switch
            {
                0 => T_Tracer_Common01,
                1 => T_Tracer_Common02,
                2 => T_Tracer_Common03,
                3 => T_Tracer_Common04
            };
        }


        private static ImageBrush LoadTexture(string path)
        {

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();


            var brush = new ImageBrush(bitmap)
            {
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.None,
                Stretch = Stretch.Fill
            };
            return brush;
        }
    }
}
