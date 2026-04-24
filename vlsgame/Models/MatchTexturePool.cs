using OpenCvSharp;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSGame.Config;
namespace VLSGame.Models
{
    public sealed class MatchTexturePool
    {
        public static MatchTexturePool Instance { get; } = new();

        private MatchTexturePool() { }

        private static readonly Random rnd = new ();

        #region Bullet 
        private readonly ImageBrush T_Tracer_Common01 = LoadTexture(@"Content\Animation\BallisticsFX\CommonTracer\T_Tracer_Common01.png");
        private readonly ImageBrush T_Tracer_Common02 = LoadTexture(@"Content\Animation\BallisticsFX\CommonTracer\T_Tracer_Common02.png");
        private readonly ImageBrush T_Tracer_Common03 = LoadTexture(@"Content\Animation\BallisticsFX\CommonTracer\T_Tracer_Common03.png");
        private readonly ImageBrush T_Tracer_Common04 = LoadTexture(@"Content\Animation\BallisticsFX\CommonTracer\T_Tracer_Common04.png");


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
        #endregion

        private readonly ImageBrush Test_Enemy = LoadTexture(@"Content\Enemy\Test_Enemy.png");


        #region World
        private ImageBrush World_Color;
        private Mat World_Depth;

        private int World_Color_Width;
        private int World_Color_Height;

        private int World_Depth_Width;
        private int World_Depth_Height;

        public ImageBrush GetEnvironmentTexture()
        {
            return World_Color;
        }
        /// <summary>
        /// UPDATES WORLD TEXTURES. USE THE BASE NAME OF TEXTURE PAIR that is in @"Content\Maps" directory.   E.X. @"Content\Maps\Test_W.png" -> "Test"
        /// </summary>
        public void UpdateEnvironmentTexture(string basePath)
        {
            World_Color = LoadTexture(@"Content\Maps\" + basePath + "_W.png");

            if (World_Color.ImageSource is BitmapImage bmp)
            {
                World_Color_Width = bmp.PixelWidth;
                World_Color_Height = bmp.PixelHeight;
            }


            World_Depth = LoadCV(@"Content\Maps\" + basePath + "_D.png");
            World_Depth_Width = World_Depth.Width;
            World_Depth_Height = World_Depth.Height;
        } 
        #endregion

        // Enter texture coordinates of pixel to recieve its depth from the depth map
        public double GetDistanceAtPixel(int x, int y)
        {
            if (World_Depth == null || x < 0 || x >= World_Depth_Width || y >= World_Depth_Height)
                return 0;

            return (World_Depth.At<ushort>(y, x) / (double)ushort.MaxValue)
                   * Configuration.Instance.GameSettings.MaxSnipingDistance;
        }


        public (int X, int Y) GetTextureCoordinatesFromDirection(Vector3D direction)
        {
            direction.Normalize();

            double theta = Math.Atan2(direction.Z, direction.X);
            double phi = Math.Acos(direction.Y);

            if (theta < 0) theta += 2 * Math.PI;

            double u = theta / (2 * Math.PI);
            double v = phi / Math.PI;

            int pixelX = (int)(u * World_Depth_Width);
            int pixelY = (int)(v * World_Depth_Height);

            pixelX = Math.Max(0, Math.Min(World_Depth_Width - 1, pixelX));
            pixelY = Math.Max(0, Math.Min(World_Depth_Height - 1, pixelY));

            return (pixelX, pixelY);
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

        private static Mat LoadCV(string path) => Cv2.ImRead(path, ImreadModes.Unchanged);
    }
}
