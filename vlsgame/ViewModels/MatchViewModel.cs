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

        private BitmapSource? mapTexture;
        //public BitmapSource? MapTexture { get => mapTexture; private set => Set(ref mapTexture, value); }

        private string distanceText = "";
        private string pixelCoordinates = "";
        private string lastBullet = ""; // info about last bullet

        // Cached texture data
        private int lastPixelX = -1;
        private int lastPixelY = -1;
        private double cachedDistance = 0;

        public MatchViewModel(IGameMode gameMode, string colorMapPath, string depthMapPath)
        {
            this.gameMode = gameMode;


            BulletManager.LastBulletInfoChanged += info => LastBullet = info;
            BulletManager.BulletCreated += (id, direction) => renderManager.CreateBulletObject3D(id, new Vector3D(direction.X, direction.Y, direction.Z));
            BulletManager.BulletUpdated += (id, direction) => renderManager.UpdateBulletObject3D(id, new Vector3D(direction.X, direction.Y, direction.Z));
            BulletManager.BulletRemoved += (id) => renderManager.Remove3D(id);

            PlayerManager.OnPlayerSpawned += (id, direction, distance, renderDistance, scale) => renderManager.CreatePlayerObject3D(id, new Vector3D(direction.X, direction.Y, direction.Z), distance, renderDistance, scale);

            PlayerManager.OnPlayerHit += (enemyId, bulletDir, hitPoint, zone, u, v) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"HIT: Enemy {enemyId.ToString("N")[..8]}, Zone = {zone}, " +
                    $"UV = ({u:F3}, {v:F3}), " +
                    $"HitPoint = ({hitPoint.X:F4}, {hitPoint.Y:F4}, {hitPoint.Z:F4})");
            };

            BulletManager.BulletLanded += (x, y, distance, flightTime) =>
                LastBullet = $"Hit at ({x}, {y}), distance {distance:F1} m, time {flightTime:F2} s";

            // It's necessary for updating FormattedLookDirection
            CameraProperties.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CameraProperties.LookDirection))
                    OnPropertyChanged(nameof(FormattedLookDirection));
            };
        }

        public void OnViewLoaded()
        {
            renderManager.Initialize(viewport, hud);

            renderManager.CreateEnvironmentObject3D();       // Create a world panorama
            renderManager.SetLight();
            
            SetupLayers();
            StartGameLoop();

            Vector3D cameraLook3D = CameraProperties.LookDirection;
            Vector3 cameraLook = new((float)cameraLook3D.X, (float)cameraLook3D.Y, (float)cameraLook3D.Z);

            Player player = new(new Vector3(-.98f, -.09f, .18f), 1000)
            {
                Scale = .001,
                ViewportDistance = 1.0,
                HitZoneChecker = (u, v) => MatchTexturePool.Instance.GetHitZoneFromUV(u, v)
            };
            PlayerManager.AddPlayer(player);
        }

        private void SetupLayers()
        {
            // HUD 
            var hudLayer = RenderManager.Instance.GetLayer<HudLayer>();

            hudLayer?.Initialize(this);
            var crosshair = new CrosshairTexture();
            hudLayer?.RegisterTexture(crosshair);
            hudLayer?.ShowTexture("Crosshair");

            var scope = new TestScopeTexture();
            hudLayer?.RegisterTexture(scope);
            //hudLayer?.ShowTexture("Scope");

        }


        #region GAME EVENTS 
        private void StartGameLoop()
        {
            gameTimer = new()
            {
                Interval = TimeSpan.FromSeconds(deltaTime)
            };
            gameTimer.Tick += OnGameTick;
            gameTimer.Start();
        }

        private void OnGameTick(object? sender, EventArgs e)
        {
            
            BulletManager.UpdateBullets(deltaTime);
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

                return renderManager.GetTextureCoordinatesFromDirection(dir3D);
            }

            Bullet bullet = new (startPos, cameraLook, renderManager.GetDistanceAtPixel, getPixelFromDirection);
            BulletManager.AddBullet(bullet);
        }
        #endregion


        #region Debug line (distance, texture coords, etc)

        public string DistanceText { get => distanceText; set => Set(ref distanceText, value); }

        public string PixelCoordinates { get => pixelCoordinates; set => Set(ref pixelCoordinates, value); }

        public string LastBullet { get => lastBullet; set => Set(ref lastBullet, value); }

        public string FormattedLookDirection => $"LookDirection: {CameraProperties.LookDirection.X:F2}, {CameraProperties.LookDirection.Y:F2}, {CameraProperties.LookDirection.Z:F2}";

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

    }
}