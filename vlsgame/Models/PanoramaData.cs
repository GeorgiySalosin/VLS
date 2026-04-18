using OpenCvSharp;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSGame.Config;

namespace VLSGame.Models
{
    public class PanoramaData()
    {
        public BitmapSource? ColorBitmap { get; private set; }
        public Mat? DepthMat { get; private set; }

        public int ColorWidth { get; private set; }
        public int ColorHeight { get; private set; }
        public int DepthWidth { get; private set; }
        public int DepthHeight { get; private set; }


        public void LoadTextures(string colorMapPath, string depthMapPath)
        {

            ColorBitmap = LoadBitmapSource(colorMapPath);   // Load color as BitmapSource because we want it to be a texture
            ColorWidth = ColorBitmap.PixelWidth;
            ColorHeight = ColorBitmap.PixelHeight;


            DepthMat = Cv2.ImRead(depthMapPath, ImreadModes.Unchanged);     // Load depth in CV2 Image formt, allowing the quicker access to raw data
            if (DepthMat == null || DepthMat.Empty())
                throw new Exception("Error loading depth map");

            DepthWidth = DepthMat.Width;
            DepthHeight = DepthMat.Height;


            //CacheDepthData();
        }

        public BitmapImage? LoadBitmapSource(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading bitmap: {ex.Message}");
                return null;
            }
        }


        // Enter texture coordinates of pixel to recieve its depth from the depth map
        public double GetDistanceAtPixel(int x, int y)
        {
            if (DepthMat == null || x < 0 || x >= DepthWidth || y >= DepthHeight)
                return 0;

            return (DepthMat.At<ushort>(y, x) / (double)ushort.MaxValue)
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

            int pixelX = (int)(u * DepthWidth);
            int pixelY = (int)(v * DepthHeight);

            pixelX = Math.Max(0, Math.Min(DepthWidth - 1, pixelX));
            pixelY = Math.Max(0, Math.Min(DepthHeight - 1, pixelY));

            return (pixelX, pixelY);
        }
    }
}