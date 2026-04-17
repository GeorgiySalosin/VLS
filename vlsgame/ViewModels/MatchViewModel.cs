using OpenCvSharp;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using VLSGame.Config;
using VLSGame.Models;
using VLSGame.Rendering;
using VLSGame.Rendering.Content2D.HUD;
using VLSGame.Rendering.Content3D;
using VLSShared.Interfaces;
using VLSShared.Models;
using static VLSGame.Rendering.Content3D.Material;
using static VLSGame.Rendering.Content3D.Mesh;

namespace VLSGame.ViewModels
{
    public class MatchViewModel : ViewModelBase
    {
        #region UI From View
        private Viewport3D viewport;     // MAIN 3D VIEWPORT,  set from Match View
        public Viewport3D Viewport
        {
            get => viewport;
            set
            {
                if (Set(ref viewport, value))
                    OnPropertyChanged(nameof(Viewport));
            }
        }
        private Panel hud;          // PANEL THAT CONTAINS 2D TEXTURE ELEMENTS, set from Match View
        public Panel Hud
        {
            get => hud;
            set
            {
                if (Set(ref hud, value))
                    OnPropertyChanged(nameof(Hud));
            }
        }  
        #endregion


        #region Timer settings

        private DispatcherTimer gameTimer;
        private const int tickHz = 60;

        #endregion

        public CameraProperties CameraProperties { get; private set; } = new();     // All stuff regarding "at the moment" camera properties (current vector, fov, etc)

        private readonly IGameMode gameMode;
        private readonly PanoramaData panoramaData;
        private readonly RenderManager renderManager = RenderManager.Instance;

        private BitmapSource? mapTexture;
        public BitmapSource? MapTexture { get => mapTexture; private set => Set(ref mapTexture, value); }

        private string distanceText = "";
        private string pixelCoordinates = "";
        private string lastBullet = ""; // info about last bullet

        // Cached texture data
        private int lastPixelX = -1;
        private int lastPixelY = -1;
        private double cachedDistance = 0;

        public MatchViewModel(IGameMode gameMode, string colorMapPath, string depthMapPath)
        {
            //PropertyChanged += OnPropertyChanged;
            this.gameMode = gameMode;

            panoramaData = new PanoramaData();
            panoramaData.LoadTextures(colorMapPath, depthMapPath);
            
            MapTexture = ConvertMatToBitmap(panoramaData.ColorMat);

            BulletManager.BulletLanded += (int x, int y, double distance, double flightTime) =>
                LastBullet = $"Hit at ({x}, {y}), distance {distance:F1} m, time {flightTime:F2} s";
            

            CameraProperties.RotationX = 0;
            CameraProperties.RotationY = 0;

        }

        /* Implement this in Renderer */

        //private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(MapTexture))   // here we autoupdate a MESH if texture was changed.
        //    {
        //        UpdateModelTexture();
        //    }
        //}
        public void OnViewLoaded()
        {
            if(viewport!= null && hud != null)
            {
                renderManager.Initialize(viewport, hud);
                renderManager.Add3D(CreateEnvironmentObject3D());
                renderManager.Add3D(CreateBulletObject3D());
                renderManager.SetLight();
                StartGameLoop();
            }
        }

        //private void SetupLayers()
        //{
        //    // HUD 
        //    var hudLayer = RenderManager.Instance.GetLayer<HudLayer>();

        //    hudLayer?.Initialize(viewModel);
        //    var crosshair = new CrosshairTexture();
        //    hudLayer?.RegisterTexture(crosshair);
        //    hudLayer?.ShowTexture("Crosshair");


        //}


        #region GAME EVENTS 
        private void StartGameLoop()
        {
            gameTimer = new()
            {
                Interval = TimeSpan.FromSeconds(1.0 / tickHz)
            };
            gameTimer.Tick += OnGameTick;
            gameTimer.Start();
        }

        private void OnGameTick(object? sender, EventArgs e)
        {
            BulletManager.UpdateBullets(tickHz);
            renderManager.Render();
            GetCenterDistance();
        }

        internal void Shoot()
        {
            Vector3 startPos = new(0, 0, 0);    // ??? Do we really need it here

            Vector3D cameraLook3D = CameraProperties.LookDirection;

            Vector3 cameraLook = new((float)cameraLook3D.X, (float)cameraLook3D.Y, (float)cameraLook3D.Z);


            (int X, int Y) getPixelFromDirection(Vector3 dir)       // Didnt know that we can declare local functions like this
            {
                // Vector3 → Vector3D
                var dir3D = new Vector3D(dir.X, dir.Y, dir.Z);

                return panoramaData.GetTextureCoordinatesFromDirection(dir3D);
            }

            Bullet bullet = new Bullet(startPos, cameraLook, panoramaData.GetDistanceAtPixel, getPixelFromDirection);
            BulletManager.AddBullet(bullet);
        }
        #endregion







        public string DistanceText { get => distanceText; set => Set(ref distanceText, value); }

        public string PixelCoordinates { get => pixelCoordinates; set => Set(ref pixelCoordinates, value); }

        public string LastBullet { get => lastBullet; set => Set(ref lastBullet, value); }




        public void GetCenterDistance()
        {
            var (pixelX, pixelY) = panoramaData.GetTextureCoordinatesFromDirection(CameraProperties.LookDirection);

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
        }


        #region MESH, MATERIALS CREATION

        public CustomObject3D CreateEnvironmentObject3D()
        {
            GeometryModel3D geometryModel = new(SphereMesh(radius: 10), TextureMaterial(MapTexture));
            ModelVisual3D sphereVisual = new() { Content = geometryModel };
            
            CustomObject3D environment = new(sphereVisual, tag:"environment");
            return environment;
        }


        public CustomObject3D CreateBulletObject3D(Guid bulletId = new())
        {
            var mesh = PlaneMesh();
            var material = RGBAMaterial(255,255,150);
            var geometryModel = new GeometryModel3D(mesh, material);
            var bulletVisual = new ModelVisual3D { Content = geometryModel };

            CustomObject3D bullet = new(bulletVisual, id: bulletId, tag: "bullet");
            return bullet;
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

                // BGR (3 channels) === PixelFormats.Bgr24; otherwise - w/alpha channel
                var pixelFormat = mat.Channels() == 3 ? PixelFormats.Bgr24 : PixelFormats.Bgr32;
                var bitmap = new WriteableBitmap(width, height, 96, 96, pixelFormat, null);

                bitmap.Lock();
                try
                {
                    unsafe
                    {
                        byte* source = mat.DataPointer;
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