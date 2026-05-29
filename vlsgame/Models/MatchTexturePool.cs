using OpenCvSharp;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSGame.Config;
using VLSShared.Models;
namespace VLSGame.Models
{
    public sealed class MatchTexturePool
    {
        public static MatchTexturePool Instance { get; } = new();

        private MatchTexturePool() { }

        private static readonly Random rnd = new ();

        private readonly ImageBrush Empty = new();
        public ImageBrush GetEmptyTexture() => Empty;

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

        private readonly ImageBrush Test_Enemy = LoadTextureTransparent(@"Content\Enemy\Test_Enemy.png");

        public ImageBrush GetEnemyTexture() => Test_Enemy;

        private readonly Mat Test_Enemy_Coll = LoadCV(@"Content\Enemy\Test_Enemy_Coll.png");



        private readonly List<ImageBrush> Animation_Blood = 
        [
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud01.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud02.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud03.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud04.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud05.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud06.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud07.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud08.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud09.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud10.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud11.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud12.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud13.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud14.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud15.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud16.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud17.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud18.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud19.png"),
        LoadTextureTransparent(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud20.png")
        ];

        public ImageBrush? GetBloodFXTexture(ref int frame)
        {
            if (frame >= Animation_Blood.Count)
            {
                frame = -1;
                return Empty;
            }
            return Animation_Blood[frame];
        }

        public HitZoneInfo GetHitZoneFromUV(float u, float v)
        {
            if (Test_Enemy_Coll == null)
                return HitZoneInfo.None;

            int width = Test_Enemy_Coll.Width;
            int height = Test_Enemy_Coll.Height;

            int pixelX = (int)(u * (width - 1));
            int pixelY = (int)(v * (height - 1));

            pixelX = Math.Clamp(pixelX, 0, width - 1);
            pixelY = Math.Clamp(pixelY, 0, height - 1);

            Vec3b color = Test_Enemy_Coll.At<Vec3b>(pixelY, pixelX);
            // OpenCV: Vec3b -> Item0 = Blue, Item1 = Green, Item2 = Red

            if (color.Item2 > 128) return HitZoneInfo.Head;   // RED channel
            if (color.Item1 > 128) return HitZoneInfo.Body;   // GREEN channel
            if (color.Item0 > 128) return HitZoneInfo.Limb;  // BLUE channel

            return HitZoneInfo.None;
        }

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
        /// Asynchronously loads a color map and a depth map with a progress report.
        /// </summary>
        internal async Task UpdateEnvironmentTextureAsync(string colorMapPath, string depthMapPath, IProgress<LoadingProgress>? progress)
        {
            System.Diagnostics.Debug.WriteLine($"Loading color map: {colorMapPath}");
            byte[] colorData = await ReadFileWithProgressAsync(colorMapPath, progress, "Color map");
            System.Diagnostics.Debug.WriteLine($"Color map loaded, size: {colorData.Length} bytes");

            System.Diagnostics.Debug.WriteLine($"Loading depth map: {depthMapPath}");
            byte[] depthData = await ReadFileWithProgressAsync(depthMapPath, progress, "Depth map");
            System.Diagnostics.Debug.WriteLine($"Depth map loaded, size: {depthData.Length} bytes");

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                System.Diagnostics.Debug.WriteLine("Creating BitmapImage from color data...");
                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(colorData))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                World_Color = new ImageBrush(bitmap) { ViewportUnits = BrushMappingMode.Absolute, TileMode = TileMode.None, Stretch = Stretch.Fill };
                World_Color_Width = bitmap.PixelWidth;
                World_Color_Height = bitmap.PixelHeight;

                System.Diagnostics.Debug.WriteLine("Decoding depth map...");
                World_Depth = Cv2.ImDecode(depthData, ImreadModes.Unchanged);
                World_Depth_Width = World_Depth.Width;
                World_Depth_Height = World_Depth.Height;
                System.Diagnostics.Debug.WriteLine("Textures ready");
            });
        }
        #endregion

        private async Task<byte[]> ReadFileWithProgressAsync(string filePath, IProgress<LoadingProgress>? progress, string fileDescription)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
            long totalBytes = stream.Length;
            var buffer = new byte[8192];
            var result = new MemoryStream();
            long bytesReadTotal = 0;
            int lastReportedPercent = -1;

            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await result.WriteAsync(buffer, 0, bytesRead);
                bytesReadTotal += bytesRead;

                int percent = (int)((double)bytesReadTotal / totalBytes * 100);
                // We only inform you if the percentage has changed.
                if (percent != lastReportedPercent)
                {
                    lastReportedPercent = percent;
                    progress?.Report(new LoadingProgress(percent, bytesReadTotal, totalBytes, fileDescription));
                }
            }
            return result.ToArray();
        }

        // Enter texture coordinates of pixel to recieve its depth from the depth map
        public double GetDistanceAtPixel(int x, int y)
        {
            if (World_Depth == null || x < 0 || x >= World_Depth_Width || y >= World_Depth_Height)
                return 0;

            return (World_Depth.At<ushort>(y, x) / (double)ushort.MaxValue)
                   * Configuration.Instance.GameSettings.MaxSnipingDistance;
        }


        /// <summary>
        /// Takes a direction vector and converts it to pixel coordinates of a sphere mesh clamped by a depthmap resolution (used for getting a specified pixel of depth map)
        /// </summary>
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

        private static ImageBrush LoadTextureTransparent(string path, double opacity = 0.5)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();        // remove if changing transparency dynamically

            var brush = new ImageBrush(bitmap)
            {
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.None,
                Stretch = Stretch.Fill,
                Opacity = opacity   // дополнительное ослабление прозрачности
            };
            return brush;
        }

        private static Mat LoadCV(string path) => Cv2.ImRead(path, ImreadModes.Unchanged);
    }
}
