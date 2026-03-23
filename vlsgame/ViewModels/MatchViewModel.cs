using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using OpenCvSharp;
using VLSShared.Interfaces;
using VLSShared.Models;

namespace VLSGame.ViewModels
{
    public class MatchViewModel : INotifyPropertyChanged
    {
        private readonly IGameMode _gameMode;
        private readonly string _panoramaPath;
        private readonly string _depthMapPath;

        private BitmapSource? _panoramaImage;
        private Mat? _depthMat; // OpenCV Mat для карты глубины
        private int _depthWidth;
        private int _depthHeight;

        // Оптимизация: используем кэшированные данные
        private ushort[]? _depthData; // Прямой доступ к данным глубины
        private byte[]? _panoramaData; // Кэшированные данные панорамы
        private int _panoramaWidth;
        private int _panoramaHeight;
        private int _panoramaStride;

        private SolidColorBrush _centerColor = new(Colors.Transparent);
        private string _centerColorText = "RGB: ---";
        private string _distanceText = "Дистанция: --- м";
        private string _pixelCoordinates = "Координаты: ---";

        private double _rotationX;
        private double _rotationY;
        private bool _isDragging;

        // Константы
        private const double MAX_DISTANCE_METERS = 2000.0;
        private const double MAX_DISTANCE_THRESHOLD = 30.0; // Исправлено название с THRESOLD на THRESHOLD
        private const ushort MAX_DEPTH_VALUE = 65535;

        // Для оптимизации - кэшируем последние координаты
        private int _lastPixelX = -1;
        private int _lastPixelY = -1;
        private double _cachedDistance = 0;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<Vector3D>? CameraDirectionChanged;

        public MatchViewModel(IGameMode gameMode, string panoramaPath, string depthMapPath)
        {
            _gameMode = gameMode;
            _panoramaPath = panoramaPath;
            _depthMapPath = depthMapPath;
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

        public string DistanceText
        {
            get => _distanceText;
            set
            {
                _distanceText = value;
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
                if (Math.Abs(_rotationX - value) > 0.0001)
                {
                    _rotationX = value;
                    OnPropertyChanged();
                    UpdateCameraDirection();
                    UpdateCenterDistance();
                }
            }
        }

        public double RotationY
        {
            get => _rotationY;
            set
            {
                if (Math.Abs(_rotationY - value) > 0.0001)
                {
                    _rotationY = value;
                    OnPropertyChanged();
                    UpdateCameraDirection();
                    UpdateCenterDistance();
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
                // Загружаем основную панораму (WPF BitmapImage для отображения)
                var panorama = new BitmapImage();
                panorama.BeginInit();
                panorama.CacheOption = BitmapCacheOption.OnLoad;
                panorama.UriSource = new Uri(_panoramaPath);
                panorama.EndInit();
                panorama.Freeze();

                PanoramaImage = panorama;

                // Кэшируем данные панорамы для быстрого доступа
                CachePanoramaData(panorama);

                // Загружаем карту глубины с помощью OpenCV
                LoadDepthMapFast();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки текстур: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void CachePanoramaData(BitmapSource bitmapSource)
        {
            try
            {
                _panoramaWidth = bitmapSource.PixelWidth;
                _panoramaHeight = bitmapSource.PixelHeight;
                _panoramaStride = _panoramaWidth * 4; // 4 байта на пиксель (BGRA)

                // Создаем WriteableBitmap для доступа к пикселям
                var writable = new WriteableBitmap(bitmapSource);
                writable.Lock();

                try
                {
                    int totalBytes = _panoramaStride * _panoramaHeight;
                    _panoramaData = new byte[totalBytes];

                    // Копируем данные в managed массив
                    System.Runtime.InteropServices.Marshal.Copy(
                        writable.BackBuffer,
                        _panoramaData,
                        0,
                        totalBytes);
                }
                finally
                {
                    writable.Unlock();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка кэширования панорамы: {ex.Message}");
                _panoramaData = null;
            }
        }

        private void LoadDepthMapFast()
        {
            try
            {
                // Используем OpenCV для загрузки 16-битной PNG
                _depthMat = Cv2.ImRead(_depthMapPath, ImreadModes.Unchanged);

                if (_depthMat == null || _depthMat.Empty())
                {
                    throw new Exception("Не удалось загрузить карту глубины");
                }

                _depthWidth = _depthMat.Width;
                _depthHeight = _depthMat.Height;

                // Проверяем тип данных
                if (_depthMat.Type() != MatType.CV_16UC1)
                {
                    System.Windows.MessageBox.Show(
                        $"Карта глубины имеет неподдерживаемый тип: {_depthMat.Type()}\n" +
                        "Ожидается CV_16UC1 (16-бит unsigned short)",
                        "Предупреждение",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);

                    // Конвертируем если возможно
                    if (_depthMat.Channels() == 1)
                    {
                        _depthMat.ConvertTo(_depthMat, MatType.CV_16UC1);
                    }
                }

                // Кэшируем данные глубины в managed массив для быстрого доступа
                CacheDepthData();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки карты глубины: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                _depthMat?.Dispose();
                _depthMat = null;
            }
        }

        private void CacheDepthData()
        {
            if (_depthMat == null) return;

            try
            {
                int totalPixels = _depthWidth * _depthHeight;
                _depthData = new ushort[totalPixels];

                unsafe
                {
                    ushort* source = (ushort*)_depthMat.DataPointer;
                    fixed (ushort* target = _depthData)
                    {
                        // Копируем данные в managed массив
                        for (int i = 0; i < totalPixels; i++)
                        {
                            target[i] = source[i];
                        }
                    }
                }

                // После кэширования можно освободить Mat, если не нужен
                // _depthMat?.Dispose();
                // _depthMat = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка кэширования глубины: {ex.Message}");
                _depthData = null;
            }
        }

        public void UpdateCenterDistance()
        {
            if (_depthData == null) return;

            try
            {
                // Вычисляем направление камеры
                double x = Math.Cos(RotationX) * Math.Sin(RotationY);
                double y = Math.Sin(RotationX);
                double z = Math.Cos(RotationX) * Math.Cos(RotationY);

                Vector3D lookDir = new Vector3D(x, y, z);
                lookDir.Normalize();

                // Конвертируем в текстурные координаты
                double theta = Math.Atan2(lookDir.Z, lookDir.X);
                double phi = Math.Acos(lookDir.Y);

                if (theta < 0) theta += 2 * Math.PI;

                double u = theta / (2 * Math.PI);
                double v = phi / Math.PI;

                int pixelX = (int)(u * _depthWidth);
                int pixelY = (int)(v * _depthHeight);

                pixelX = Math.Max(0, Math.Min(_depthWidth - 1, pixelX));
                pixelY = Math.Max(0, Math.Min(_depthHeight - 1, pixelY));

                // Обновляем только если координаты изменились
                if (pixelX != _lastPixelX || pixelY != _lastPixelY)
                {
                    _lastPixelX = pixelX;
                    _lastPixelY = pixelY;

                    // Читаем значение глубины из кэша
                    _cachedDistance = ReadDepthAtPixelFast(pixelX, pixelY);

                    // Форматируем вывод с использованием порога
                    string distanceFormatted;
                    if (_cachedDistance < 0.1)
                    {
                        distanceFormatted = "< 0.1 м";
                    }
                    else if (_cachedDistance > MAX_DISTANCE_METERS - MAX_DISTANCE_THRESHOLD)
                    {
                        // Если расстояние больше, чем MAX_DISTANCE_METERS - MAX_DISTANCE_THRESHOLD,
                        // считаем его как "более 2000 м"
                        distanceFormatted = $"> {MAX_DISTANCE_METERS:F0} м";
                    }
                    else
                    {
                        distanceFormatted = $"{_cachedDistance:F1} м";
                    }

                    // Обновляем UI
                    DistanceText = $"Дистанция: {distanceFormatted}";
                    PixelCoordinates = $"Координаты: ({pixelX}, {pixelY})";

                    // Обновляем цвет
                    UpdateColorAtPixelFast(pixelX, pixelY);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка чтения глубины: {ex.Message}");
            }
        }

        private double ReadDepthAtPixelFast(int x, int y)
        {
            if (_depthData == null) return 0;

            int index = y * _depthWidth + x;
            if (index >= 0 && index < _depthData.Length)
            {
                ushort depthValue = _depthData[index];
                return (depthValue / (double)MAX_DEPTH_VALUE) * MAX_DISTANCE_METERS;
            }

            return 0;
        }

        private void UpdateColorAtPixelFast(int x, int y)
        {
            if (_panoramaData == null) return;

            try
            {
                // Масштабируем координаты для панорамы (могут отличаться по размеру)
                int panX = (int)((double)x / _depthWidth * _panoramaWidth);
                int panY = (int)((double)y / _depthHeight * _panoramaHeight);

                panX = Math.Max(0, Math.Min(_panoramaWidth - 1, panX));
                panY = Math.Max(0, Math.Min(_panoramaHeight - 1, panY));

                // Читаем из кэшированного массива (формат BGRA)
                int index = panY * _panoramaStride + panX * 4;

                if (index + 3 < _panoramaData.Length)
                {
                    byte b = _panoramaData[index + 0]; // Blue
                    byte g = _panoramaData[index + 1]; // Green
                    byte r = _panoramaData[index + 2]; // Red

                    Color color = Color.FromRgb(r, g, b);

                    // Обновляем UI (можно использовать Dispatcher, если нужно)
                    CenterColor = new SolidColorBrush(color);
                    CenterColorText = $"RGB: {r}, {g}, {b}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка чтения цвета: {ex.Message}");
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

        public void Dispose()
        {
            _depthMat?.Dispose();
            _depthData = null;
            _panoramaData = null;
        }
    }
}