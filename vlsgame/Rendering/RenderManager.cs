using System.Formats.Asn1;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.Config;
using VLSGame.Models;
using VLSGame.Rendering.Content2D;
using VLSGame.Rendering.Content3D;
using static VLSGame.Rendering.Content3D.Material;
using static VLSGame.Rendering.Content3D.Mesh;

namespace VLSGame.Rendering
{
    /// <summary>
    /// Contains all rendering logics using Renderer3D, REnderer2D
    /// </summary>
    public sealed class RenderManager
    {

        #region Initialization  

        public static RenderManager Instance { get; } = new();
        private static readonly Renderer3D renderer3D = Renderer3D.Instance;    // A tool responsible for rendering of all 3D. Renders stuff to Viewport3D
        private static readonly Renderer2D renderer2D = Renderer2D.Instance;    // A tool responsible for rendering of all 2D. Renders stuff to Panel
        private static readonly MatchTexturePool texturePool = MatchTexturePool.Instance;           // Pre-loading all in-game textures and reusing them!

        private RenderManager() { }

        private static bool isInitialized = false;

        internal async Task InitializeAsync(Viewport3D viewport, Panel hudPanel,
            string colorMapPath, string depthMapPath,
            IProgress<LoadingProgress>? progress, CancellationToken token)
        {
            if (isInitialized) return;
            System.Diagnostics.Debug.WriteLine("RenderManager.InitializeAsync: initializing 3D renderer");
            renderer3D.Initialize(viewport);
            System.Diagnostics.Debug.WriteLine("RenderManager.InitializeAsync: initializing 2D renderer");
            renderer2D.Initialize(hudPanel);

            System.Diagnostics.Debug.WriteLine("RenderManager.InitializeAsync: updating environment textures...");
            await texturePool.UpdateEnvironmentTextureAsync(colorMapPath, depthMapPath, progress, token); // Loads new environment textures
            System.Diagnostics.Debug.WriteLine("RenderManager.InitializeAsync: textures loaded");

            isInitialized = true;
        }
        #endregion

        #region 2D 

        private CustomObject2D? crosshair2D;
        private CustomObject2D? scope2D;
        private CameraProperties? cameraProperties;

        public void Initialize2D(CameraProperties cameraProperties)
        {
            this.cameraProperties = cameraProperties;
            cameraProperties.PropertyChanged += OnCameraPropertyChanged;
            CreateCrosshair2D();
            CreateScope2D();
            Update2DVisibility();
        }

        private void OnCameraPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CameraProperties.FieldOfView))
                Update2DVisibility();
        }

        private void Update2DVisibility()
        {
            if (cameraProperties == null) return;
            bool isScoped = cameraProperties.FieldOfView < Configuration.Instance.GameSettings.MaxFOV;
            crosshair2D?.IsVisible = !isScoped;
            
        }

        private void CreateCrosshair2D()
        {
            var tex = MatchTexturePool.Instance.GetCrosshairTexture();
            crosshair2D = new CustomObject2D(tex, tag: "Crosshair") { IsVisible = true };
            Add2D(crosshair2D);
        }

        private void CreateScope2D()
        {
            var frames = MatchTexturePool.Instance.GetSVLK14SZoomingFrames();
            if (frames == null || frames.Count == 0) return;
            var firstFrame = frames[0];
            scope2D = new CustomObject2D(firstFrame, tag: "Scope");
            // Добавляем с кадрами и коллбэком
            renderer2D.AddObject(scope2D, frames.ToList(), OnScopeAnimationComplete);
        }

        private void OnScopeAnimationComplete()
        {
            Update2DVisibility();
        }

        public void StartScopeAnimationForward()
        {
            if (scope2D == null) return;
            scope2D.Animation.PlayForward();
            Update2DVisibility();
        }

        public void StartScopeAnimationBackward()
        {
            if (scope2D == null) return;
            scope2D.Animation.PlayBackward();
            Update2DVisibility();
        }



        public void Add2D(CustomObject2D obj) => renderer2D.AddObject(obj);
        public void Remove2D(Guid id) => renderer2D.RemoveObject(id);
        public CustomObject2D? Get2D(Guid id) => renderer2D.GetObject(id);
        public CustomObject2D? Get2D(string tag) => renderer2D.GetObject(tag);
        #endregion


        #region 3D 

        #region MODELS CREATION 

        #region World 
        /// <summary>
        ///  Creates a new custom 3d of environment sphere and adds it to viewport
        /// </summary>
        public void CreateEnvironmentObject3D()
        {
            GeometryModel3D geometryModel = new(
                SphereMesh(radius: 2048),
                TextureMaterial(texturePool.GetEnvironmentTexture())
                );
            ModelVisual3D sphereVisual = new() { Content = geometryModel };

            CustomObject3D environment = new(sphereVisual, tag: CustomObject3DTags.World);
            Add3D(environment);
        }

        #endregion

        #region Bullet
        /// <summary>
        ///  Creates a new custom 3d of bullet and adds it to viewport
        /// </summary>
        // Изменить метод CreateBulletObject3D
        public void CreateBulletObject3D(Guid bulletId)
        {
            var mesh = PlaneMesh(1, 1); // базовый размер
            var texture = texturePool.GetBulletTexture();
            var material = TextureMaterial(texture);
            var geometryModel = new GeometryModel3D(mesh, material);
            var bulletVisual = new ModelVisual3D { Content = geometryModel };

            var bullet = new CustomObject3D(bulletVisual, id: bulletId,
                                            tag: CustomObject3DTags.Projectile);

            // Ставим на небольшую начальную дистанцию, чтобы сразу увидеть
            //bullet.SetWorldPosition(initialDirection * 0.5);
            Add3D(bullet);
        }

        /// <summary>
        /// updates a bulet position with new one from vec3 and does stuff
        /// </summary>
        public void UpdateBulletObject3D(Guid bulletId, Vector3 worldPos)
        {
            var bullet = Get3D(bulletId);
            if (bullet != null)
            {

                var pos3D = new Vector3D(worldPos.X, worldPos.Y, worldPos.Z);

                bullet.SetWorldPosition(pos3D);     // set a new pos in viewport

                double distance = pos3D.Length;
                double scale = Math.Max(0.1, distance * 0.0015 + 0.2);      // found hardcoded params with which bullet doesn't visually scale down too fast
                bullet.SetScale(scale);

                bullet.SetTexture(texturePool.GetBulletTexture());          // update a tracer texture w/ random one from pool
            }
        }

        #endregion

        #region Enemy
        /// <summary>
        ///  Creates a new custom 3d of ENEMY PLAYER and adds it to viewport. Returns a distance on which enemy was placed.
        /// </summary>
        public (double, double) CreatePlayerObject3D(Guid playerId, Vector3D initialDirection)
        {
            double scale = 3.5;                     // Assuming we have a man with standart height 1m 75cm, but we have to multiply by 2 cause texture uses only a half-space of the height.
            var mesh = PlaneMesh(scale, scale);     

            var texture = texturePool.GetEnemyTexture();
            var material = TextureMaterial(texture);
            var geometryModel = new GeometryModel3D(mesh, material);
            var playerVisual = new ModelVisual3D { Content = geometryModel };

            var (X, Y) = texturePool.GetTextureCoordinatesFromDirection(initialDirection);      // get a depth (distance) in which were gonna place our enemy avatar
            double distance = texturePool.GetDistanceAtPixel(X, Y);                             // get a depth (distance) in which were gonna place our enemy avatar

            var player = new CustomObject3D(playerVisual, id: playerId,
                                           tag: CustomObject3DTags.Enemy);

            var worldPosition = initialDirection * distance;                // place a character in a correct world position automatically 


            player.SetWorldPosition(worldPosition);    
            Add3D(player);
            

            return (distance, scale);
        }

        /// <summary>
        ///  Creates a plane for blood effect on PlayerHit event
        /// </summary>
        public void CreatePlayerFX(Vector3D worldPosition, double fxScale)
        {
            var mesh = PlaneMesh(fxScale);

            var texture = texturePool.GetEmptyTexture3D();
            var material = TextureMaterial(texture);
            var geometryModel = new GeometryModel3D(mesh, material);
            var bloodFXVisual = new ModelVisual3D { Content = geometryModel };


            var bloodFX = new CustomObject3D(bloodFXVisual,
                                           tag: CustomObject3DTags.FXAnimationSingle);


            bloodFX.SetWorldPosition(worldPosition * 0.95);                           // place an effect a bit closer than the character is to avoid mesh overlapping
            bloodFX.Animation.PlayForward();
            Add3D(bloodFX);
        }

        //public void PlayHitAnimation(Guid playerId, Vector3D worldPosition)
        //{

        //    Get3D(playerId).Children
        //}


        #endregion

        #region Lightind
        /// <summary>
        ///  Creates and adds to viewport  a new white ambient light to represent a scene with its original colors
        /// </summary>
        public void SetLight()
        {
            var ambientLight = new ModelVisual3D
            {
                Content = new AmbientLight(Colors.White)
            };

            CustomObject3D ambientlight = new(ambientLight, tag: CustomObject3DTags.AmbientLight);

            Add3D(ambientlight);
        } 
        #endregion

        #endregion

        /// <summary>
        ///  Adds a new custom 3d object to the scene
        /// </summary>
        public void Add3D(CustomObject3D obj) => renderer3D.AddObject(obj);
        /// <summary>
        ///  Removes custom 3d object from the scene by its id
        /// </summary>
        public void Remove3D(Guid id) => renderer3D.RemoveObject(id);
        /// <summary>
        ///  Gets scene custom 3d object reference by its id. PoSsible null reference!
        /// </summary>
        public CustomObject3D Get3D(Guid id) => renderer3D.GetObject(id);

        /// <summary>
        ///  Gets scene custom 3d object reference by its tag. PoSsible null reference! Use with uniquue objects like world.
        /// </summary>
        public CustomObject3D Get3D(CustomObject3DTags tag) => renderer3D.GetObject(tag);


        public (int X, int Y) GetTextureCoordinatesFromDirection(Vector3D direction) => texturePool.GetTextureCoordinatesFromDirection(direction);

        public double GetDistanceAtPixel(int x, int y) => texturePool.GetDistanceAtPixel(x, y);

        #endregion


        public void Render()
        {
            renderer2D.Render();
            renderer3D.Render();
        }

    }
}