using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using VLSGame.Config;
using VLSGame.Config.GameConfig;
using VLSGame.Models;
using VLSGame.Rendering;
using VLSGame.Rendering.Content2D;
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

        private bool isGameLoopStarted = false;

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

        private int tickCounter;
        private DateTime lastFpsTime = DateTime.Now;
        private DateTime lastTickTime = DateTime.Now;

        private readonly List<Vector3> _targetPositions;   // list of all items from TargetsConfig
        private readonly List<int> _availableIndices;      // indexes that haven't been used yet
        private readonly Random _random = new();


        private RifleState rifleState = new();
        public RifleState RifleState => rifleState;



        public MatchViewModel(IGameMode gameMode, string colorMapPath, string depthMapPath)
        {
            this.gameMode = gameMode;

            animationController = new CameraAnimationController(CameraProperties);

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

            PlayerManager.OnPlayerDead += (Guid id) =>
            {
                Renderer3D.Instance.RemoveObject(id);
                SpawnTarget();
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

            // Loading target positions from the config
            if (!TargetsConfig.Instance.Load())
            {
                throw new InvalidOperationException("Failed to load targets configuration. Check Targets.json file.");
            }
            _targetPositions = TargetsConfig.Instance.Settings.Targets
                    .Select(tp => tp.ToVector3())
                    .ToList();
            _availableIndices = Enumerable.Range(0, _targetPositions.Count).ToList();
        }


        internal async Task LoadTexturesAsync(
            IProgress<LoadingProgress> progress,
            CancellationToken token)
        {
            Debug.WriteLine("LoadTexturesAsync started");
            await renderManager.InitializeAsync(viewport, hud, rifleState, colorMapPath, depthMapPath, progress, token);
            Debug.WriteLine("InitializeAsync completed");


            token.ThrowIfCancellationRequested();
            renderManager.CreateEnvironmentObject3D();
            renderManager.SetLight();

            SpawnTarget();

            Debug.WriteLine("LoadTexturesAsync finished");
        }

        internal void Initialize2D() => renderManager.Initialize2D(CameraProperties, rifleState);

        #region GAME EVENTS 
        internal void StartGameLoop()
        {
            if (isGameLoopStarted) return;
            isGameLoopStarted = true;
            gameTimer = new DispatcherTimer(DispatcherPriority.Input, Application.Current.Dispatcher);
            gameTimer.Interval = TimeSpan.FromSeconds(deltaTime);
            gameTimer.Tick += OnGameTick;
            gameTimer.Start();
        }



        private void OnGameTick(object? sender, EventArgs e)
        {
            // 1. FOV
            CameraProperties.UpdateCameraFOV(deltaTime, (float)Configuration.Instance.Settings.ZoomSpeedAuto);

            // 2. Анимации (меняют AnimationRotationX/Y -> устанавливают флаг dirty)
            animationController.Update(deltaTime);

            // 3. Применяем все изменения к ViewModel (пересчёт RotationX/Y, LookDirection, уведомления)
            CameraProperties.UpdateCameraRotation();

            // 4. Обновление пуль и рендер
            BulletManager.UpdateBullets(deltaTime);
            renderManager.Render();
            GetCenterDistance();

            tickCounter++;
            if ((DateTime.Now - lastFpsTime).TotalSeconds >= 1.0)
            {
                int fps = tickCounter;
                tickCounter = 0;
                lastFpsTime = DateTime.Now;
                Debug.WriteLine($"[GameLoop] FPS: {fps}");
            }
        }




        internal void Shoot()
        {
            if (rifleState.State == ERifleState.Reloading) return;
            if (!rifleState.HasAmmo) return;


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

            rifleState.HasAmmo = false;
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

                if (cachedDistance > Configuration.Instance.Settings.MaxSnipingDistance - Configuration.Instance.Settings.MaxSnipingDistanceThresold)
                {
                    cachedDistance = Configuration.Instance.Settings.MaxSnipingDistance;
                    DistanceText = $"Distance: > {cachedDistance:F0} м";
                }
                else
                    DistanceText = $"Distance: {cachedDistance:F1} m";

                PixelCoordinates = $"Texture coordinates: ({pixelX}, {pixelY})";
            }
        }

        #region Singleplayer Spawn

        /// <summary>
        /// Returns the next random target position, without repeating the already used ones.
        /// When all positions are used, the list is reset and a new round begins.
        /// </summary>
        /// 
        private Vector3 GetNextTargetPosition()
        {
            if (_availableIndices.Count == 0)
            {
                // Reset: re-populate with all indexes
                _availableIndices.AddRange(Enumerable.Range(0, _targetPositions.Count));
            }

            // Choose a random index from the available ones
            int randomIndex = _random.Next(_availableIndices.Count);
            int selectedIndex = _availableIndices[randomIndex];
            _availableIndices.RemoveAt(randomIndex);

            return _targetPositions[selectedIndex];
        }

        private void SpawnTarget()
        {
            Vector3 targetPos = GetNextTargetPosition();
            Player player = new(targetPos)
            {
                HitZoneChecker = (u, v) => MatchTexturePool.Instance.GetHitZoneFromUV(u, v)
            };
            PlayerManager.AddPlayer(player);
        }

        #endregion


        public void StartReload()
        {
            if (rifleState.State == ERifleState.Reloading) return;
            if (rifleState.HasAmmo) return;

            switch (rifleState.State)
            {
                case ERifleState.IdleZoom:
                    rifleState.State = ERifleState.ZoomingOut;
                    RenderManager.Instance.StartZoomOutAnimation(() =>
                    {
                        rifleState.State = ERifleState.Reloading;
                        RenderManager.Instance.StartReloadAnimation(OnReloadComplete);
                    });
                    break;

                case ERifleState.ZoomingIn:
                    rifleState.State = ERifleState.ZoomingOut;
                    RenderManager.Instance.StartZoomOutAnimation(() =>
                    {
                        rifleState.State = ERifleState.Reloading;
                        RenderManager.Instance.StartReloadAnimation(OnReloadComplete);
                    });
                    break;

                case ERifleState.ZoomingOut:
                    rifleState.State = ERifleState.Reloading;
                    RenderManager.Instance.SetOnZoomOutComplete(() =>
                    {
                        rifleState.State = ERifleState.Reloading;
                        RenderManager.Instance.StartReloadAnimation(OnReloadComplete);
                    });
                    break;

                default: // Idle
                    rifleState.State = ERifleState.Reloading;
                    RenderManager.Instance.StartReloadAnimation(OnReloadComplete);
                    break;
            }
        }

        private void OnReloadComplete()
        {
            rifleState.HasAmmo = true;
            rifleState.State = ERifleState.Idle;
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