using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace VLSGame
{
    public partial class Match : Window
    {
        private Point lastMousePosition;
        private double rotationX = 0; // Вертикальное вращение (вверх-вниз) в радианах
        private double rotationY = 0; // Горизонтальное вращение (вокруг Y оси) в радианах
        private bool isDragging = false;
        private BitmapSource panoramaImage;
        private WriteableBitmap writableBitmap;
        private ModelVisual3D sphereVisual;

        // Чувствительность мыши
        private const double MOUSE_SENSITIVITY = 0.001;

        // Dependency Properties для отображения цвета и координат
        public static readonly DependencyProperty CenterColorProperty =
            DependencyProperty.Register("CenterColor", typeof(SolidColorBrush), typeof(Match),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public static readonly DependencyProperty CenterColorTextProperty =
            DependencyProperty.Register("CenterColorText", typeof(string), typeof(Match),
                new PropertyMetadata("RGB: ---"));

        public static readonly DependencyProperty PixelCoordinatesProperty =
            DependencyProperty.Register("PixelCoordinates", typeof(string), typeof(Match),
                new PropertyMetadata("Координаты: ---"));

        public SolidColorBrush CenterColor
        {
            get { return (SolidColorBrush)GetValue(CenterColorProperty); }
            set { SetValue(CenterColorProperty, value); }
        }

        public string CenterColorText
        {
            get { return (string)GetValue(CenterColorTextProperty); }
            set { SetValue(CenterColorTextProperty, value); }
        }

        public string PixelCoordinates
        {
            get { return (string)GetValue(PixelCoordinatesProperty); }
            set { SetValue(PixelCoordinatesProperty, value); }
        }

        public Match(string imagePath)
        {
            InitializeComponent();
            this.DataContext = this;

            Loaded += (s, e) => LoadPanorama(imagePath);

            // Подписка на события мыши
            this.MouseDown += Match_MouseDown;
            this.MouseMove += Match_MouseMove;
            this.MouseUp += Match_MouseUp;
            this.MouseWheel += Match_MouseWheel;

            // Таймер для обновления цвета в центре
            CompositionTarget.Rendering += UpdateCenterColor;

            // Устанавливаем начальное направление камеры
            rotationY = 0; // Смотрим вдоль оси Z
            rotationX = 0; // Смотрим прямо (горизонт)
            UpdateCameraDirection();
        }

        private void LoadPanorama(string imagePath)
        {
            try
            {
                // Загрузка изображения
                panoramaImage = new BitmapImage(new Uri(imagePath));

                // Создаем WriteableBitmap для доступа к пикселям
                writableBitmap = new WriteableBitmap(panoramaImage);

                // Создаем сферу с текстурой
                CreatePanoramaSphere();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки панорамы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void CreatePanoramaSphere()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            const int phiSegments = 128;
            const int thetaSegments = 256;

            for (int i = 0; i <= phiSegments; i++)
            {
                double phi = Math.PI * i / phiSegments; // 0 до PI (северный до южного полюса)

                for (int j = 0; j <= thetaSegments; j++)
                {
                    double theta = 2 * Math.PI * j / thetaSegments; // 0 до 2PI

                    // Стандартные сферические координаты (НЕ инвертируем Y)
                    double x = Math.Sin(phi) * Math.Cos(theta);
                    double y = Math.Cos(phi); // Убираем минус
                    double z = Math.Sin(phi) * Math.Sin(theta);

                    mesh.Positions.Add(new Point3D(x, y, z));

                    // Текстурные координаты
                    double u = theta / (2 * Math.PI);
                    double v = phi / Math.PI; // Убираем инверсию

                    mesh.TextureCoordinates.Add(new Point(u, v));
                }
            }

            // Индексы треугольников (без изменений)
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

            // Материал с текстурой
            ImageBrush brush = new ImageBrush(panoramaImage)
            {
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.None,
                Stretch = Stretch.Fill
            };

            DiffuseMaterial material = new DiffuseMaterial(brush);

            GeometryModel3D geometryModel = new GeometryModel3D(mesh, material);

            // Масштабируем сферу
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new ScaleTransform3D(100, 100, 100));

            // Убираем лишний поворот или оставляем только если нужно
            // transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)));

            geometryModel.Transform = transformGroup;

            sphereVisual = new ModelVisual3D();
            sphereVisual.Content = geometryModel;
            MainViewport.Children.Add(sphereVisual);
        }

        private void UpdateCenterColor(object sender, EventArgs e)
        {
            if (writableBitmap == null) return;

            try
            {
                Vector3D lookDir = MainCamera.LookDirection;
                lookDir.Normalize();

                // Конвертируем направление в сферические координаты
                double theta = Math.Atan2(lookDir.Z, lookDir.X); // Горизонтальный угол
                double phi = Math.Acos(lookDir.Y); // Вертикальный угол (убираем минус)

                if (theta < 0) theta += 2 * Math.PI;

                double u = theta / (2 * Math.PI);
                double v = phi / Math.PI;

                int x = (int)(u * writableBitmap.PixelWidth);
                int y = (int)(v * writableBitmap.PixelHeight);

                x = Math.Max(0, Math.Min(writableBitmap.PixelWidth - 1, x));
                y = Math.Max(0, Math.Min(writableBitmap.PixelHeight - 1, y));

                writableBitmap.Lock();
                unsafe
                {
                    byte* pixelData = (byte*)writableBitmap.BackBuffer;
                    int stride = writableBitmap.BackBufferStride;

                    byte b = pixelData[y * stride + x * 4 + 0];
                    byte g = pixelData[y * stride + x * 4 + 1];
                    byte r = pixelData[y * stride + x * 4 + 2];
                    byte a = pixelData[y * stride + x * 4 + 3];

                    Color color = Color.FromArgb(a, r, g, b);

                    Dispatcher.Invoke(() =>
                    {
                        CenterColor = new SolidColorBrush(color);
                        CenterColorText = $"RGB: {r}, {g}, {b}";
                        PixelCoordinates = $"Координаты: ({x}, {y})";
                    });
                }
                writableBitmap.Unlock();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка чтения пикселя: {ex.Message}");
            }
        }

        private void Match_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = true;
                lastMousePosition = e.GetPosition(this);
                this.Cursor = Cursors.Hand;
                Mouse.Capture(this);
            }
        }

        private void Match_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(this);

                // Вычисляем дельту в относительных координатах (относительно размера окна)
                double deltaX = (currentPosition.X - lastMousePosition.X) / this.ActualWidth;
                double deltaY = (currentPosition.Y - lastMousePosition.Y) / this.ActualHeight;

                // Обновляем углы с учетом чувствительности и поля зрения
                double fovFactor = MainCamera.FieldOfView / 90.0; // Компенсация при зуме
                rotationY -= deltaX * Math.PI * fovFactor; // Горизонтальное вращение
                rotationX -= deltaY * Math.PI * fovFactor * 0.5; // Вертикальное (ограничено)

                // clamp minimum and maximum  camera vertical rotation:  max param = 1.5, min = 0
                rotationX = Math.Max(-Math.PI / 2+1.0, Math.Min(Math.PI / 2-1.5, rotationX));

                // Обновляем направление камеры
                UpdateCameraDirection();

                lastMousePosition = currentPosition;
            }
        }

        private void Match_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = false;
                this.Cursor = Cursors.Arrow;
                Mouse.Capture(null);
            }
        }

        private void Match_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Плавное изменение поля зрения
            double zoomSpeed = 0.1;
            MainCamera.FieldOfView -= e.Delta * zoomSpeed;
            MainCamera.FieldOfView = Math.Max(30, Math.Min(120, MainCamera.FieldOfView));
        }

        private void UpdateCameraDirection()
        {
            // Вычисляем направление камеры
            double x = Math.Cos(rotationX) * Math.Sin(rotationY);
            double y = Math.Sin(rotationX);
            double z = Math.Cos(rotationX) * Math.Cos(rotationY);

            MainCamera.LookDirection = new Vector3D(x, y, z);
            MainCamera.UpDirection = new Vector3D(0, 1, 0);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}