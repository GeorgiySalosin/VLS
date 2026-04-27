using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VLSGame.Rendering.Content2D
{
    public class Texture : INotifyPropertyChanged
    {
        private double translateX;
        private double translateY;
        private double scale = 1.0;
        private bool isVisible;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; }
        public Image Image { get; private set; }

        public double PixelWidth { get; private set; }
        public double PixelHeight { get; private set; }

        
        private readonly double defaultTranslateX;
        private readonly double defaultTranslateY;
        private readonly double defaultScale = 1.0;
        private readonly bool defaultVisibility;


        public Texture(string name, double defaultTranslateX = 0, double defaultTranslateY = 0,
                       double defaultScale = 1.0, bool defaultVisibility = false)
        {
            Name = name;
            Image = new Image
            {
                IsHitTestVisible = false,
                Stretch = Stretch.None,
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = double.NaN,
                Height = double.NaN,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            // Default values
            this.defaultTranslateX = defaultTranslateX;
            this.defaultTranslateY = defaultTranslateY;
            this.defaultScale = defaultScale;
            this.defaultVisibility = defaultVisibility;

            
            translateX = defaultTranslateX;
            translateY = defaultTranslateY;
            scale = defaultScale;
            isVisible = defaultVisibility;
        }

        #region Properties with change notification

        public double TranslateX
        {
            get => translateX;
            set
            {
                if (Math.Abs(translateX - value) > 0)
                {
                    translateX = value;
                    OnPropertyChanged();
                    UpdateTransform();
                }
            }
        }

        public double TranslateY
        {
            get => translateY;
            set
            {
                if (Math.Abs(translateY - value) > 0)
                {
                    translateY = value;
                    OnPropertyChanged();
                    UpdateTransform();
                }
            }
        }

        public double Scale
        {
            get => scale;
            set
            {
                if (Math.Abs(scale - value) > 0)
                {
                    scale = value;
                    OnPropertyChanged();
                    UpdateTransform();
                }
            }
        }


        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    OnPropertyChanged();
                    UpdateVisibility();
                }
            }
        }

        #endregion

        #region Public Methods

        public void LoadFromFile(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                var convertedBitmap = ConvertTo96Dpi(bitmap);

                PixelWidth = convertedBitmap.PixelWidth;
                PixelHeight = convertedBitmap.PixelHeight;

                Image.Source = convertedBitmap;
                //Image.Width = PixelWidth;
                //Image.Height = PixelHeight;
                //Image.Width = 2560;
                //Image.Height = 1440;

                // Apply current transform values
                UpdateTransform();
                UpdateVisibility();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading texture: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets position with given coordinates
        /// </summary>
        public void SetPosition(double x, double y)
        {
            TranslateX = x;
            TranslateY = y;
        }

        /// <summary>
        /// Adds offset to current position x, y
        /// </summary>
        public void Move(double deltaX, double deltaY)
        {
            TranslateX += deltaX;
            TranslateY += deltaY;
        }

        /// <summary>
        /// Sets scale with given value
        /// </summary>
        public void SetScale(double scale)
        {
            Scale = scale;
        }

        /// <summary>
        /// Moves an object to its default position
        /// </summary>
        public void ResetPosition()
        {
            TranslateX = defaultTranslateX;
            TranslateY = defaultTranslateY;
        }

        /// <summary>
        /// Resizes object to its default scale
        /// </summary>
        public void ResetScale()
        {
            Scale = defaultScale;
        }


        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;


        public void SetScale(double width, double height)
        {
            Image.Width = width;
            Image.Height = height;
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// bypasses WPF shitty dpi recalculation to render texture at its actual pixel width, height.
        /// </summary>
        private static BitmapSource ConvertTo96Dpi(BitmapSource source)
        {
            const double targetDpi = 96.0;

            if (Math.Abs(source.DpiX - targetDpi) < 0.01 &&
                Math.Abs(source.DpiY - targetDpi) < 0.01)
            {
                return source;
            }

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawImage(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));
            }

            var renderBitmap = new RenderTargetBitmap(
                source.PixelWidth,
                source.PixelHeight,
                targetDpi,
                targetDpi,
                PixelFormats.Pbgra32);

            renderBitmap.Render(visual);
            renderBitmap.Freeze();

            return renderBitmap;
        }

        /// <summary>
        /// Called when a texture is either moving or resizing
        /// </summary>
        private void UpdateTransform()
        {
            if (Math.Abs(Scale - 1.0) < 0)
            {
                if (Math.Abs(TranslateX) > 0 || Math.Abs(TranslateY) > 0)
                {
                    Image.RenderTransform = new TranslateTransform(TranslateX, TranslateY);
                }
                else
                {
                    Image.RenderTransform = null;
                }
            }
            else
            {
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform(Scale, Scale));
                transformGroup.Children.Add(new TranslateTransform(TranslateX, TranslateY));
                Image.RenderTransform = transformGroup;
            }
        }
        
        /// <summary>
        /// Toggle tex visibility
        /// </summary>
        private void UpdateVisibility()
        {
            Image.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}