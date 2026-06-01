using OpenCvSharp;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSGame.Config.GameConfig;
using VLSShared.Models;
namespace VLSGame.Models
{
    public sealed class MatchTexturePool
    {
        public static MatchTexturePool Instance { get; } = new();

        private MatchTexturePool() { }

        private static readonly Random rnd = new ();



        #region 3D-rendered textures (ImageBrushes)

        private readonly ImageBrush emptyBrush = new();
        public ImageBrush GetEmptyTexture3D() => emptyBrush;



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

        #region Enemy 
        private readonly ImageBrush Test_Enemy = LoadTextureTransparent(@"Content\Enemy\Test_Enemy.png");

        public ImageBrush GetEnemyTexture() => Test_Enemy;

        private readonly Mat Test_Enemy_Coll = LoadCV(@"Content\Enemy\Test_Enemy_Coll.png");
        #endregion

        #region Blood FX 
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
                return GetEmptyTexture3D();
            }
            return Animation_Blood[frame];
        }
        #endregion

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
        /// Asynchronously loads a color map and a depth map with a combined progress report.
        /// </summary>
        internal async Task UpdateEnvironmentTextureAsync(
            string colorMapPath, string depthMapPath,
            IProgress<LoadingProgress>? progress, CancellationToken token)
        {
            // Calculate total size of both files
            var colorFileInfo = new FileInfo(colorMapPath);
            var depthFileInfo = new FileInfo(depthMapPath);
            long totalBytes = colorFileInfo.Length + depthFileInfo.Length;
            long totalBytesRead = 0;
            int lastReportedPercent = -1;

            // Local function to load a single file and update the combined progress
            async Task<byte[]> LoadFileWithCombinedProgress(string filePath, string description)
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
                var buffer = new byte[8192];
                var result = new MemoryStream();
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    token.ThrowIfCancellationRequested();
                    await result.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    int percent = (int)((double)totalBytesRead / totalBytes * 100);
                    if (percent != lastReportedPercent)
                    {
                        lastReportedPercent = percent;
                        progress?.Report(new LoadingProgress(percent, totalBytesRead, totalBytes, description));
                    }
                }
                return result.ToArray();
            }

            System.Diagnostics.Debug.WriteLine($"Loading color map: {colorMapPath}");
            byte[] colorData = await LoadFileWithCombinedProgress(colorMapPath, "Color map");
            System.Diagnostics.Debug.WriteLine($"Color map loaded, size: {colorData.Length} bytes");

            System.Diagnostics.Debug.WriteLine($"Loading depth map: {depthMapPath}");
            byte[] depthData = await LoadFileWithCombinedProgress(depthMapPath, "Depth map");
            System.Diagnostics.Debug.WriteLine($"Depth map loaded, size: {depthData.Length} bytes");

            // Needed to update the ProgressBar
            await Task.Delay(50);

            // Create UI objects on the dispatcher thread
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                token.ThrowIfCancellationRequested();
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

        #region Utils 
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
        #endregion

        #endregion



        #region 2D-rendered textures (ImageSource)

        private readonly ImageSource emptySource = new BitmapImage();
        public ImageSource GetEmptyTexture2D() => emptySource;



        #region HUD stuff 
        private ImageSource crosshairTexture = LoadTextureFixedDpi(@"Content/ui/T_CrossAIM.png");
        #endregion

        #region RifleAnimations 

        private readonly List<ImageSource> Animation_SVLK14S_Zooming =
            [
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_000.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_001.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_002.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_003.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_004.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_005.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_006.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_007.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_008.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_009.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_010.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_011.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_012.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_013.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_014.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_015.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_016.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_017.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_018.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_019.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_020.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_021.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_022.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_023.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_024.png"),
            LoadTextureAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zooming/A_Zooming_025.png")
            ];
        public ImageSource? GetSVLK14SZoomingTexture(ref int frame)
        {
            if (frame >= Animation_SVLK14S_Zooming.Count)
            {
                frame = -1;
                return GetEmptyTexture2D();
            }
            return Animation_SVLK14S_Zooming[frame];
        }
        public IReadOnlyList<ImageSource> GetSVLK14SZoomingFrames() => Animation_SVLK14S_Zooming;


        public ImageSource GetCrosshairTexture() => crosshairTexture;
        #endregion


        #region Utils 
        /// <summary>
        /// Load texture w/ forced DPI (idk but fucking wpf requires strictly 96 dpi otherwise it will smear a texture at its own wish)
        /// This Will make a texture stay same pixel size through different screen resolutions
        /// </summary>
        private static ImageSource LoadTextureFixedDpi(string path, double targetDpi = 96.0)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            // Matching dpi - no conversion
            if (Math.Abs(bitmap.DpiX - targetDpi) < 0.01 && Math.Abs(bitmap.DpiY - targetDpi) < 0.01)
                return bitmap;

            // re-render otherwise using correct dpi
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawImage(bitmap, new System.Windows.Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }

            var renderBitmap = new RenderTargetBitmap(
                bitmap.PixelWidth, bitmap.PixelHeight,
                targetDpi, targetDpi,
                PixelFormats.Pbgra32);

            renderBitmap.Render(visual);
            renderBitmap.Freeze();
            return renderBitmap;
        }


        /// <summary>
        /// Загружает текстуру с адаптивным размером: ширина = screenWidth * 16 / 15,
        /// высота пропорциональна исходному соотношению сторон.
        /// </summary>
        private static ImageSource LoadTextureAdaptive(string path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            int targetWidth = (int)(screenWidth * 16.0 / 15.0);
            int targetHeight = (int)(bitmap.PixelHeight * ((double)targetWidth / bitmap.PixelWidth));



            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                context.DrawImage(bitmap, new System.Windows.Rect(0, 0, targetWidth, targetHeight));
            }

            var renderBitmap = new RenderTargetBitmap(targetWidth, targetHeight, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);
            renderBitmap.Freeze();
            return renderBitmap;
        } 
        #endregion


        #endregion



        #region Shared Utils
        /// <summary>
        /// Get hitzone from U, V coordinates of Enemy plane mesh
        /// </summary>
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

        /// <summary>
        /// Enter texture coordinates of pixel to recieve its depth from the depth map
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

        #endregion
    }
}
