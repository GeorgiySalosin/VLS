using OpenCvSharp;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using VLSGame.Config;
using VLSGame.Models;
using VLSShared.Interfaces;
using VLSShared.Models;

namespace VLSGame.ViewModels
{
    public class MatchViewModel : ViewModelBase
    {
        // Timer
        private DispatcherTimer _gameTimer;
        private const int tickHz = 100;

        private readonly IGameMode gameMode;
        internal PanoramaData panoramaData; // private readonly
        public CameraProperties CameraProperties { get; private set; } = new();     // A functionality of ViewModel that was Extracted into Camera Properties

        private BitmapSource? colorMapTexture;
        private string distanceText = "";
        private string pixelCoordinates = "";

        // Cached texture data
        private int lastPixelX = -1;
        private int lastPixelY = -1;
        private double cachedDistance = 0;

        public MatchViewModel(IGameMode gameMode, string colorMapPath, string depthMapPath)
        {
            this.gameMode = gameMode;
            panoramaData = new PanoramaData();
            panoramaData.LoadTextures(colorMapPath, depthMapPath);
            colorMapTexture = ConvertMatToBitmap(panoramaData.ColorMat);
            StartGameLoop();
        }

        private void StartGameLoop()
        {
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(1.0 / tickHz);
            _gameTimer.Tick += OnGameTick;
            _gameTimer.Start();
        }

        private void OnGameTick(object? sender, EventArgs e)
        {
            BulletManager.UpdateBullets();

            // Здесь можно обновить другие игровые логики
            // Например, перерисовать прицел или обновить отображаемую дистанцию
            // UpdateCenterDistance();
        }

        public BitmapSource? ColorMapTexture
        {
            get => colorMapTexture;
            private set => Set(ref  colorMapTexture, value);
        }

        public string DistanceText
        {
            get => distanceText;
            set => Set(ref distanceText, value);
        }

        public string PixelCoordinates
        {
            get => pixelCoordinates;
            set => Set(ref pixelCoordinates, value);
        }

        public (int X, int Y) GetTextureCoordinatesFromDirection(Vector3D direction)
        {
            direction.Normalize();

            double theta = Math.Atan2(direction.Z, direction.X);
            double phi = Math.Acos(direction.Y);

            if (theta < 0) theta += 2 * Math.PI;

            double u = theta / (2 * Math.PI);
            double v = phi / Math.PI;

            int pixelX = (int)(u * panoramaData.DepthWidth);
            int pixelY = (int)(v * panoramaData.DepthHeight);

            pixelX = Math.Max(0, Math.Min(panoramaData.DepthWidth - 1, pixelX));
            pixelY = Math.Max(0, Math.Min(panoramaData.DepthHeight - 1, pixelY));

            return (pixelX, pixelY);
        }

        //public double GetDistancePixel(int x, int y)
        //{

        //}
        public double GetCenterDistance()
        {
            var (pixelX, pixelY) = GetTextureCoordinatesFromDirection(CameraProperties.LookDirection);

            if (pixelX != lastPixelX || pixelY != lastPixelY)
            {
                lastPixelX = pixelX;
                lastPixelY = pixelY;

                cachedDistance = panoramaData.GetDistanceAtPixel(pixelX, pixelY);

                if (cachedDistance > Configuration.Instance.GameSettings.MaxSnipingDistance - Configuration.Instance.GameSettings.MaxSnipingDistanceThresold)
                {
                    cachedDistance = Configuration.Instance.GameSettings.MaxSnipingDistance;
                    DistanceText = $"Distance: > {cachedDistance:F0} м";
                }
                else
                    DistanceText = $"Distance: {cachedDistance:F1} m";

                PixelCoordinates = $"Texture coordinates: ({pixelX}, {pixelY})";
            }
            return cachedDistance;
        }

        #region PANORAMA MESH, MATERIALS, TEXTURE SETTINGS 
        public ModelVisual3D CreatePanoramaSphere()
        {
            var mesh = CreateSphereMesh(phiSegments: 128, thetaSegments: 256);
            var material = CreatePanoramaMaterial(ColorMapTexture);
            var geometryModel = new GeometryModel3D(mesh, material);

            var sphereVisual = new ModelVisual3D { Content = geometryModel };
            return sphereVisual;
        }

        private static MeshGeometry3D CreateSphereMesh(int phiSegments, int thetaSegments)
        {
            var mesh = new MeshGeometry3D();

            for (int i = 0; i <= phiSegments; i++)
            {
                double phi = Math.PI * i / phiSegments;

                for (int j = 0; j <= thetaSegments; j++)
                {
                    double theta = 2 * Math.PI * j / thetaSegments;

                    double x = Math.Sin(phi) * Math.Cos(theta);
                    double y = Math.Cos(phi);
                    double z = Math.Sin(phi) * Math.Sin(theta);

                    mesh.Positions.Add(new Point3D(x, y, z));

                    double u = theta / (2 * Math.PI);
                    double v = phi / Math.PI;
                    mesh.TextureCoordinates.Add(new System.Windows.Point(u, v));
                }
            }

            for (int i = 0; i < phiSegments; i++)
            {
                for (int j = 0; j < thetaSegments; j++)
                {
                    int p0 = i * (thetaSegments + 1) + j;
                    int p1 = i * (thetaSegments + 1) + j + 1;
                    int p2 = (i + 1) * (thetaSegments + 1) + j;
                    int p3 = (i + 1) * (thetaSegments + 1) + j + 1;

                    mesh.TriangleIndices.Add(p0);
                    mesh.TriangleIndices.Add(p2);
                    mesh.TriangleIndices.Add(p1);

                    mesh.TriangleIndices.Add(p1);
                    mesh.TriangleIndices.Add(p2);
                    mesh.TriangleIndices.Add(p3);
                }
            }

            return mesh;
        }

        private static DiffuseMaterial CreatePanoramaMaterial(ImageSource? texture)
        {
            var brush = new ImageBrush(texture)
            {
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.None,
                Stretch = Stretch.Fill
            };

            return new DiffuseMaterial(brush);
        }

        /// <summary> Converts raw opencv data to WPF-frieldly bitmap to use it as a texture</summary>
        private WriteableBitmap? ConvertMatToBitmap(Mat? mat)
        {
            if (mat == null || mat.Empty())
                return null;

            try
            {
                int width = mat.Width;
                int height = mat.Height;
                int stride = width * mat.Channels();

                // BGR (3 channels) === PixelFormats.Bgr24
                var pixelFormat = mat.Channels() == 3 ? PixelFormats.Bgr24 : PixelFormats.Bgr32;
                var bitmap = new WriteableBitmap(width, height, 96, 96, pixelFormat, null);

                bitmap.Lock();
                try
                {
                    unsafe
                    {
                        byte* source = (byte*)mat.DataPointer;
                        byte* target = (byte*)bitmap.BackBuffer;
                        int totalBytes = stride * height;

                        for (int i = 0; i < totalBytes; i++)
                            target[i] = source[i];
                    }
                    bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
                }
                finally
                {
                    bitmap.Unlock();
                }

                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting Mat to BitmapSource: {ex.Message}");
                return null;
            }
        }
        #endregion

        public void Dispose()
        {
            panoramaData.Dispose();
        }
    }
}