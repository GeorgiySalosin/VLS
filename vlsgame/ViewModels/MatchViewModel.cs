using OpenCvSharp;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSGame.Config;
using VLSGame.Models;
using VLSShared.Interfaces;

namespace VLSGame.ViewModels
{
    public class MatchViewModel : ViewModelBase
    {
        private readonly IGameMode _gameMode;
        private readonly PanoramaData _panoramaData;
        public CameraProperties CameraProperties { get; private set; } = new();     // A functionality of ViewModel that was Extracted into Camera Properties

        private BitmapSource? _colorMapTexture;
        private string _distanceText;
        private string _pixelCoordinates;
        private bool _isDragging;

        // Cached texture data
        private int _lastPixelX = -1;
        private int _lastPixelY = -1;
        private double _cachedDistance = 0;


        public MatchViewModel(IGameMode gameMode, string colorMapPath, string depthMapPath)
        {
            _gameMode = gameMode;
            _panoramaData = new PanoramaData();
            _panoramaData.LoadTextures(colorMapPath, depthMapPath);
            _colorMapTexture = ConvertMatToBitmapSource(_panoramaData.ColorMat);
        }

        public BitmapSource? ColorMapTexture
        {
            get => _colorMapTexture;
            private set => Set(ref  _colorMapTexture, value);
        }

        public string DistanceText
        {
            get => _distanceText;
            set => Set(ref _distanceText, value);
        }

        public string PixelCoordinates
        {
            get => _pixelCoordinates;
            set => Set(ref _pixelCoordinates, value);
        }


        public bool IsDragging
        {
            get => _isDragging;
            set => Set(ref _isDragging, value);
        }



        public (int X, int Y) GetTextureCoordinatesFromDirection(Vector3D direction)
        {
            direction.Normalize();

            double theta = Math.Atan2(direction.Z, direction.X);
            double phi = Math.Acos(direction.Y);

            if (theta < 0) theta += 2 * Math.PI;

            double u = theta / (2 * Math.PI);
            double v = phi / Math.PI;

            int pixelX = (int)(u * _panoramaData.DepthWidth);
            int pixelY = (int)(v * _panoramaData.DepthHeight);

            pixelX = Math.Max(0, Math.Min(_panoramaData.DepthWidth - 1, pixelX));
            pixelY = Math.Max(0, Math.Min(_panoramaData.DepthHeight - 1, pixelY));

            return (pixelX, pixelY);
        }

        public void UpdateCenterDistance()
        {
            var (pixelX, pixelY) = GetTextureCoordinatesFromDirection(CameraProperties.LookDirection);

            if (pixelX != _lastPixelX || pixelY != _lastPixelY)
            {
                _lastPixelX = pixelX;
                _lastPixelY = pixelY;

                _cachedDistance = _panoramaData.GetDistanceAtPixel(pixelX, pixelY);

                if (_cachedDistance > Configuration.Instance.GameSettings.MaxSnipingDistance - Configuration.Instance.GameSettings.MaxSnipingDistanceThresold)
                    DistanceText = $"Distance: > {Configuration.Instance.GameSettings.MaxSnipingDistance:F0} м";
                else 
                    DistanceText = $"Distance: {_cachedDistance:F1} m";

                PixelCoordinates = $"Texture coordinates: ({pixelX}, {pixelY})";
            }
        }


        public ModelVisual3D CreatePanoramaSphere()
        {
            var mesh = CreateSphereMesh(phiSegments: 128, thetaSegments: 256);
            var material = CreatePanoramaMaterial(ColorMapTexture);
            var geometryModel = new GeometryModel3D(mesh, material);

            var sphereVisual = new ModelVisual3D { Content = geometryModel };
            return sphereVisual;
        }

        private MeshGeometry3D CreateSphereMesh(int phiSegments, int thetaSegments)
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

        private DiffuseMaterial CreatePanoramaMaterial(ImageSource? texture)
        {
            var brush = new ImageBrush(texture)
            {
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.None,
                Stretch = Stretch.Fill
            };

            return new DiffuseMaterial(brush);
        }

        public void UpdateCameraRotation(double currentRotationX, double currentRotationY)
        {
            CameraProperties.RotationX = currentRotationX;
            CameraProperties.RotationY = currentRotationY;
        }

        private BitmapSource? ConvertMatToBitmapSource(Mat? mat)
        {
            if (mat == null || mat.Empty())
                return null;

            try
            {
                int width = mat.Width;
                int height = mat.Height;
                int stride = width * mat.Channels();

                // Для BGR (3 канала) используем PixelFormats.Bgr24
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

        public void Dispose()
        {
            _panoramaData.Dispose();
        }
    }
}