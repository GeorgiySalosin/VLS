using OpenCvSharp;
using System.CodeDom;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using VLSGame.Config;
using VLSGame.Models;
using VLSGame.Rendering;
using VLSGame.Rendering.Content2D;
using VLSGame.Rendering.Content3D;
using VLSShared.Interfaces;
using VLSShared.Models;

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
        private const float deltaTime = 1f / tickHz;

        #endregion

        public CameraProperties CameraProperties { get; private set; } = new();     // All stuff regarding "at the moment" camera properties (current vector, fov, etc)

        private readonly IGameMode gameMode;
        private readonly RenderManager renderManager = RenderManager.Instance;
        //private readonly PanoramaData panoramaData;

        private CameraAnimationController animationController;

        private BitmapSource? mapTexture;
        //public BitmapSource? MapTexture { get => mapTexture; private set => Set(ref mapTexture, value); }

        private string distanceText = "";
        private string pixelCoordinates = "";
        private string lastBullet = ""; // info about last bullet

        // Cached texture data
        private int lastPixelX = -1;
        private int lastPixelY = -1;
        private double cachedDistance = 0;


        private CustomObject2D? crosshairObject;
        private CustomObject2D? scopeObject;

        private int _frameCounter;
        private readonly string colorMapPath;
        private readonly string depthMapPath;

        public MatchViewModel(IGameMode gameMode, string colorMapPath, string depthMapPath)
        {
            this.gameMode = gameMode;

            var animSettings = Configuration.Instance.CameraAnimationSettings; // добавляете в GameSettings
            animationController = new CameraAnimationController(CameraProperties, animSettings);

            BulletManager.LastBulletInfoChanged += info => LastBullet = info;

            BulletManager.BulletCreated += (id, direction) => renderManager.CreateBulletObject3D(id);
            BulletManager.BulletUpdated += (id, pos) => renderManager.UpdateBulletObject3D(id, pos);
            BulletManager.BulletRemoved += (id) => renderManager.Remove3D(id);

            PlayerManager.OnPlayerSpawned += (id, direction) =>
            {
                var (distance, scale) = renderManager.CreatePlayerObject3D(id, V3(direction));         // get actual 3d model parameters and write it to math model of a player
                PlayerManager.SetPlayerDistance(id, distance);
                PlayerManager.SetPlayerScale(id, scale);
            };

            PlayerManager.OnPlayerHit += (bulletLocation, hitZoneInfo) =>
            {
                renderManager.CreatePlayerFX(V3(bulletLocation), hitZoneInfo.FXScale);
            };

            BulletManager.BulletLanded += (distance, flightTime) =>
                LastBullet = $"Hit: distance {distance:F1} m, time {flightTime:F2} s";

            // It's necessary for updating FormattedLookDirection
            CameraProperties.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CameraProperties.LookDirection))
                    OnPropertyChanged(nameof(FormattedLookDirection));
            };

            this.colorMapPath = colorMapPath;
            this.depthMapPath = depthMapPath;
        }

        public void OnViewLoaded()
        {
            renderManager.Initialize(viewport, hud, colorMapPath, depthMapPath);
            renderManager.Initialize2D(CameraProperties);   

            renderManager.CreateEnvironmentObject3D();       // Create a world panorama
            renderManager.SetLight();
            StartGameLoop();

            Vector3D cameraLook3D = CameraProperties.LookDirection;
            Vector3 cameraLook = V3(cameraLook3D);

            Player player = new(new Vector3(.4608f, -.0027f, .8875f))
            {
                HitZoneChecker = (u, v) => MatchTexturePool.Instance.GetHitZoneFromUV(u, v)
            };
            PlayerManager.AddPlayer(player);
        }





        #region GAME EVENTS 
        private void StartGameLoop()
        {
            //gameTimer = new()
            //{
            //    Interval = TimeSpan.FromSeconds(deltaTime)
            //};
            //gameTimer.Tick += OnGameTick;
            //gameTimer.Start();

            gameTimer = new DispatcherTimer(DispatcherPriority.Input, Application.Current.Dispatcher);
            gameTimer.Interval = TimeSpan.FromSeconds(deltaTime);
            gameTimer.Tick += OnGameTick;
            gameTimer.Start();
        }

        private void OnGameTick(object? sender, EventArgs e)
        {
            // Обновляем FOV (плавное приближение/отдаление)
            CameraProperties.UpdateFOV(deltaTime, (float)Configuration.Instance.CameraAnimationSettings.ZoomSpeedAuto);

            animationController.Update(deltaTime);
            BulletManager.UpdateBullets(deltaTime);
            renderManager.Render();
            GetCenterDistance();
            _frameCounter++;
            if (_frameCounter % 60 == 0)
                System.Diagnostics.Debug.WriteLine($"FOV: {CameraProperties.FieldOfView:F2} / Target: {CameraProperties.TargetFOV:F2}");
        }

        internal void Shoot()
        {
            
            Vector3 startPos = new(0, 0, 0);    // ??? Do we really need it here

            Vector3D cameraLook3D = CameraProperties.LookDirection;

            Vector3 cameraLook = V3(cameraLook3D);


            (int X, int Y) getPixelFromDirection(Vector3 dir)       // Didnt know that we can declare local functions like this
            {
                // Vector3 → Vector3D
                var dir3D = V3(dir);

                return renderManager.GetTextureCoordinatesFromDirection(dir3D);
            }

            Bullet bullet = new (startPos, cameraLook, renderManager.GetDistanceAtPixel, getPixelFromDirection);
            BulletManager.AddBullet(bullet);
            animationController.TriggerRecoil();
        }
        #endregion


        #region Debug line (distance, texture coords, etc)

        public string DistanceText { get => distanceText; set => Set(ref distanceText, value); }

        public string PixelCoordinates { get => pixelCoordinates; set => Set(ref pixelCoordinates, value); }

        public string LastBullet { get => lastBullet; set => Set(ref lastBullet, value); }

        public string FormattedLookDirection => $"LookDirection: {CameraProperties.LookDirection.X:F4}, {CameraProperties.LookDirection.Y:F4}, {CameraProperties.LookDirection.Z:F4}";

        #endregion



        public void GetCenterDistance()
        {
            var (pixelX, pixelY) = renderManager.GetTextureCoordinatesFromDirection(CameraProperties.LookDirection);

            if (pixelX != lastPixelX || pixelY != lastPixelY)
            {
                lastPixelX = pixelX;
                lastPixelY = pixelY;

                cachedDistance = renderManager.GetDistanceAtPixel(pixelX, pixelY);

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


        /// <summary>
        /// UTILITIES for converting between System.Numerics.Vector3 and System.Windows.Media.Media3D.Vector3D
        /// </summary>
        private Vector3D V3(Vector3 vec) => new (vec.X, vec.Y, vec.Z);
        /// <summary>
        /// UTILITIES for converting between System.Numerics.Vector3 and System.Windows.Media.Media3D.Vector3D
        /// </summary>
        private Vector3 V3(Vector3D vec) => new((float)vec.X, (float)vec.Y, (float)vec.Z);

    }
}