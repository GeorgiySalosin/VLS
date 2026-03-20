using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSShared.Interfaces;
using VLSShared.Models;

namespace VLSGame.ViewModels
{
    public class MatchViewModel : INotifyPropertyChanged
    {
        private readonly IGameMode _gameMode;
        private readonly string _panoramaPath;

        private BitmapSource? _panoramaImage;
        private WriteableBitmap? _writableBitmap;
        private SolidColorBrush _centerColor = new(Colors.Transparent);
        private string _centerColorText = "RGB: ---";
        private string _pixelCoordinates = "Координаты: ---";
        private double _rotationX;
        private double _rotationY;
        private bool _isDragging;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<Vector3D>? CameraDirectionChanged;

        public MatchViewModel(IGameMode gameMode, string panoramaPath)
        {
            _gameMode = gameMode;
            _panoramaPath = panoramaPath;
        }

        public BitmapSource? PanoramaImage
        {
            get => _panoramaImage;
            private set
            {
                _panoramaImage = value;
                OnPropertyChanged();
            }
        }

        public SolidColorBrush CenterColor
        {
            get => _centerColor;
            set
            {
                _centerColor = value;
                OnPropertyChanged();
            }
        }

        public string CenterColorText
        {
            get => _centerColorText;
            set
            {
                _centerColorText = value;
                OnPropertyChanged();
            }
        }

        public string PixelCoordinates
        {
            get => _pixelCoordinates;
            set
            {
                _pixelCoordinates = value;
                OnPropertyChanged();
            }
        }

        public double RotationX
        {
            get => _rotationX;
            set
            {
                if (_rotationX != value)
                {
                    _rotationX = value;
                    OnPropertyChanged();
                    UpdateCameraDirection();
                }
            }
        }

        public double RotationY
        {
            get => _rotationY;
            set
            {
                if (_rotationY != value)
                {
                    _rotationY = value;
                    OnPropertyChanged();
                    UpdateCameraDirection();
                }
            }
        }

        public bool IsDragging
        {
            get => _isDragging;
            set
            {
                _isDragging = value;
                OnPropertyChanged();
            }
        }

        public void LoadPanorama()
        {
            try
            {
                PanoramaImage = new BitmapImage(new Uri(_panoramaPath));
                _writableBitmap = new WriteableBitmap(PanoramaImage);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки панорамы: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void UpdateCenterColor()
        {
            if (_writableBitmap == null) return;

            try
            {
                // Вычисляем направление камеры из углов поворота
                double x = Math.Cos(RotationX) * Math.Sin(RotationY);
                double y = Math.Sin(RotationX);
                double z = Math.Cos(RotationX) * Math.Cos(RotationY);

                Vector3D lookDir = new Vector3D(x, y, z);
                lookDir.Normalize();

                // Конвертируем направление в сферические координаты
                double theta = Math.Atan2(lookDir.Z, lookDir.X);
                double phi = Math.Acos(lookDir.Y);

                if (theta < 0) theta += 2 * Math.PI;

                double u = theta / (2 * Math.PI);
                double v = phi / Math.PI;

                int pixelX = (int)(u * _writableBitmap.PixelWidth);
                int pixelY = (int)(v * _writableBitmap.PixelHeight);

                pixelX = Math.Max(0, Math.Min(_writableBitmap.PixelWidth - 1, pixelX));
                pixelY = Math.Max(0, Math.Min(_writableBitmap.PixelHeight - 1, pixelY));

                _writableBitmap.Lock();
                unsafe
                {
                    byte* pixelData = (byte*)_writableBitmap.BackBuffer;
                    int stride = _writableBitmap.BackBufferStride;

                    byte b = pixelData[pixelY * stride + pixelX * 4 + 0];
                    byte g = pixelData[pixelY * stride + pixelX * 4 + 1];
                    byte r = pixelData[pixelY * stride + pixelX * 4 + 2];
                    byte a = pixelData[pixelY * stride + pixelX * 4 + 3];

                    Color color = Color.FromArgb(a, r, g, b);

                    CenterColor = new SolidColorBrush(color);
                    CenterColorText = $"RGB: {r}, {g}, {b}";
                    PixelCoordinates = $"Координаты: ({pixelX}, {pixelY})";
                }
                _writableBitmap.Unlock();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка чтения пикселя: {ex.Message}");
            }
        }

        private void UpdateCameraDirection()
        {
            double x = Math.Cos(RotationX) * Math.Sin(RotationY);
            double y = Math.Sin(RotationX);
            double z = Math.Cos(RotationX) * Math.Cos(RotationY);

            CameraDirectionChanged?.Invoke(this, new Vector3D(x, y, z));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}