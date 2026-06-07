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

        private readonly List<ImageBrush> T_Tracer_Common = LoadTexture3DSequence(@"Content\Animation\BallisticsFX\CommonTracer\T_Tracer_Common{0:D2}.png", count: 4);


        public ImageBrush GetBulletTexture()
        {
            return T_Tracer_Common[rnd.Next(4)];
        }
        #endregion

        #region Enemy 
        private readonly ImageBrush Test_Enemy = LoadTexture3D(@"Content\Enemy\Test_Enemy.png");

        public ImageBrush GetEnemyTexture() => Test_Enemy;

        private readonly Mat Test_Enemy_Coll = LoadCV(@"Content\Enemy\Test_Enemy_Coll.png");
        #endregion

        #region Blood FX 
        private readonly List<ImageBrush> Animation_Blood = LoadTexture3DSequence(@"Content\Animation\PlayerFX\BloodHit\T_Hit_Cloud{0:D2}.png", count: 20, opacity: 0.7);

        public ImageBrush? GetBloodFXTexture(int? frame)
        {
            if (frame >= Animation_Blood.Count || frame == null) return GetEmptyTexture3D();
            return Animation_Blood[(int)frame];
        }
        #endregion

        // bobr: add comments
        #region World
        private ImageBrush worldColor;
        private Mat worldDepth;

        private int worldDepthWidth;
        private int worldDepthHeight;

        public ImageBrush GetEnvironmentTexture()
        {
            return worldColor;
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

            byte[] colorData = await LoadFileWithCombinedProgress(colorMapPath, "Color map");
            byte[] depthData = await LoadFileWithCombinedProgress(depthMapPath, "Depth map");

            // Needed to update the ProgressBar
            await Task.Delay(50);

            // Create UI objects on the dispatcher thread
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                token.ThrowIfCancellationRequested();

                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(colorData))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                worldColor = new ImageBrush(bitmap) { ViewportUnits = BrushMappingMode.Absolute, TileMode = TileMode.None, Stretch = Stretch.Fill };

                
                worldDepth = Cv2.ImDecode(depthData, ImreadModes.Unchanged);
                worldDepthWidth = worldDepth.Width;
                worldDepthHeight = worldDepth.Height;
            });
        }
        #endregion

        #region Utils 

        private static ImageBrush LoadTexture3D(string path, double opacity = 1.0)
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


        private static List<ImageBrush> LoadTexture3DSequence(string pathFormat, int count, double opacity = 1.0)
        {
            var result = new List<ImageBrush>(count);
            for (int i = 0; i < count; i++)
            {
                string path = string.Format(pathFormat, i);
                result.Add(LoadTexture3D(path, opacity));
            }
            return result;
        }


        private static Mat LoadCV(string path) => Cv2.ImRead(path, ImreadModes.Unchanged);  
        #endregion

        #endregion



        #region 2D-rendered textures (ImageSource)




        #region HUD stuff 
        private ImageSource crosshairTexture = LoadTexture2DFixedDpi(@"Content/ui/T_CrossAIM.png");
        #endregion

        #region RifleAnimations 

        private readonly List<ImageSource> Animation_SVLK14S_Zooming = LoadTexture2DSequenceAdaptive(@"Content/Animation/Rifle/SVLK14S/A_Zoom/A_Zooming_{0:D3}.png", count: 26);

        public IReadOnlyList<ImageSource> GetSVLK14SZoomingFrames() => Animation_SVLK14S_Zooming;


        public ImageSource GetCrosshairTexture() => crosshairTexture;
        #endregion


        #region Utils 


        /// <summary>
        /// Load texture w/ forced DPI (idk but fucking wpf requires strictly 96 dpi otherwise it will smear a texture at its own wish)
        /// This Will make a texture stay same pixel size through different screen resolutions
        /// </summary>
        private static ImageSource LoadTexture2DFixedDpi(string path, double targetDpi = 96.0)
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
        /// Loads texture with an adaprive size: Width = screenWidth * 16/15 by default, Fixed aspect ratio; <br></br>
        /// Suitable for square textures like rifle animation
        /// </summary>
        private static ImageSource LoadTexture2DAdaptive(string path, double scaleMultiplier = 16/15)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            int targetWidth = (int)(screenWidth * scaleMultiplier);
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



        /// <summary>
        /// Loads a sequence of textures with an adaptive size: Width = screenWidth * 16/15 by default, Fixed aspect ratio; <br></br>
        /// Suitable for square textures like rifle animation
        /// </summary>
        private static List<ImageSource> LoadTexture2DSequenceAdaptive(string pathFormat, int count, double scaleMultiplier = 16.0 / 15.0)
        {
            var result = new List<ImageSource>(count);
            for (int i = 0; i < count; i++)
            {
                string path = string.Format(pathFormat, i);
                result.Add(LoadTexture2DAdaptive(path, scaleMultiplier));
            }
            return result;
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
        /// </summary>
        public double GetDistanceAtPixel(int x, int y)
        {
            if (worldDepth == null || x < 0 || x >= worldDepthWidth || y >= worldDepthHeight)
                return 0;

            return (worldDepth.At<ushort>(y, x) / (double)ushort.MaxValue)
                   * Configuration.Instance.Settings.MaxSnipingDistance;
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

            int pixelX = (int)(u * worldDepthWidth);
            int pixelY = (int)(v * worldDepthHeight);

            pixelX = Math.Max(0, Math.Min(worldDepthWidth - 1, pixelX));
            pixelY = Math.Max(0, Math.Min(worldDepthHeight - 1, pixelY));

            return (pixelX, pixelY);
        }

        #endregion
    }
}
