
using System;
using OpenCvSharp;

namespace VLSGame.Models
{
    public class PanoramaData : IDisposable
    {
        public Mat? ColorMat { get; private set; }
        public Mat? DepthMat { get; private set; }

        public int ColorWidth { get; private set; }
        public int ColorHeight { get; private set; }
        public int DepthWidth { get; private set; }
        public int DepthHeight { get; private set; }

        public byte[]? ColorData { get; private set; }
        public ushort[]? DepthData { get; private set; }

        public int ColorStride => ColorWidth * 3; // (BGR)

        private const double MAX_DISTANCE_METERS = 2000.0;
        private const ushort MAX_DEPTH_VALUE = 65535;

        public void LoadTextures(string colorMapPath, string depthMapPath)
        {
            ColorMat = Cv2.ImRead(colorMapPath, ImreadModes.Color);
            DepthMat = Cv2.ImRead(depthMapPath, ImreadModes.Unchanged);

            if (ColorMat == null || ColorMat.Empty() || DepthMat == null || DepthMat.Empty())
                throw new Exception("Error loading world texture maps");

            ColorWidth = ColorMat.Width;
            ColorHeight = ColorMat.Height;
            DepthWidth = DepthMat.Width;
            DepthHeight = DepthMat.Height;

            // Конвертируем в нужные форматы
            ConvertToRequiredFormats();

            // Кэшируем данные
            CacheData();
        }

        private void ConvertToRequiredFormats()
        {
            // Ensure this is 8-bit BGR w/ 3 channels
            if (ColorMat!.Type() != MatType.CV_8UC3)
            {
                if (ColorMat.Channels() == 4)
                {
                    // BGRA -> BGR case
                    Mat bgrMat = new Mat();
                    Cv2.CvtColor(ColorMat, bgrMat, ColorConversionCodes.BGRA2BGR);
                    ColorMat.Dispose();
                    ColorMat = bgrMat;
                }
                else
                {
                    ColorMat.ConvertTo(ColorMat, MatType.CV_8UC3);
                }
            }

            // Ensure this is 16-bit unsigned short (single channel)
            if (DepthMat!.Type() != MatType.CV_16UC1 && DepthMat.Channels() == 1)
            {
                DepthMat.ConvertTo(DepthMat, MatType.CV_16UC1);
            }
        }

        private void CacheData()
        {
            // Caching color map
            int totalColorBytes = ColorStride * ColorHeight;
            ColorData = new byte[totalColorBytes];

            unsafe
            {
                byte* colorSource = ColorMat!.DataPointer;
                fixed (byte* colorTarget = ColorData)
                {
                    for (int i = 0; i < totalColorBytes; i++)
                        colorTarget[i] = colorSource[i];
                }
            }

            // Caching depth map
            int totalDepthPixels = DepthWidth * DepthHeight;
            DepthData = new ushort[totalDepthPixels];

            unsafe
            {
                ushort* depthSource = (ushort*)DepthMat!.DataPointer;
                fixed (ushort* depthTarget = DepthData)
                {
                    for (int i = 0; i < totalDepthPixels; i++)
                        depthTarget[i] = depthSource[i];
                }
            }
        }

        public double GetDistanceAtPixel(int x, int y)
        {
            if (DepthData == null || x < 0 || x >= DepthWidth || y < 0 || y >= DepthHeight)
                return 0;

            int index = y * DepthWidth + x;
            if (index >= 0 && index < DepthData.Length)
            {
                ushort depthValue = DepthData[index];
                return (depthValue / (double)MAX_DEPTH_VALUE) * MAX_DISTANCE_METERS;
            }

            return 0;
        }

        public void Dispose()
        {
            ColorMat?.Dispose();
            DepthMat?.Dispose();
            ColorData = null;
            DepthData = null;
        }
    }
}