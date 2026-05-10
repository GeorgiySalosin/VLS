using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSGame.Models;
using VLSGame.Rendering.Content2D;
using VLSGame.Rendering.Content2D.HUD;
using VLSGame.Rendering.Content3D;
using VLSShared.Enums;
using VLSShared.Models;
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
        private static readonly Renderer3D renderer3D = Renderer3D.Instance;    // A tool responsible for rendering of all 3D.
        private static readonly MatchTexturePool texturePool = MatchTexturePool.Instance;           // Pre-loading all in-game textures and reusing them!

        private RenderManager() { }

        private static bool isInitialized = false;

        public void Initialize(Viewport3D viewport, Panel hudPanel, string mapName = "Test")
        {
            if (isInitialized) return;
            renderer3D.Initialize(viewport);


            RegisterLayer(new HudLayer(hudPanel));
            RegisterLayer(new HudLayer(hudPanel));

            texturePool.UpdateEnvironmentTexture(mapName);      // Loads new environment textures

            isInitialized = true;
        }
        #endregion


        #region 2D 
        private readonly SortedDictionary<RenderOrder, Layer> Layers = [];

        public void RegisterLayer(Layer layer)
        {
            if (!Layers.ContainsKey(layer.Order))
            {
                Layers.Add(layer.Order, layer);
            }
        }

        public T? GetLayer<T>() where T : Layer => Layers.Values.OfType<T>().FirstOrDefault();



        #endregion
        //TODO: Create a new Renderer 2D Class, Move those into it.


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

        public void UpdateEnvironment(string path)
        {
            var environment = Get3D(CustomObject3DTags.World);
            texturePool.UpdateEnvironmentTexture(path);

        }

        #endregion

        #region Bullet
        /// <summary>
        ///  Creates a new custom 3d of bullet and adds it to viewport
        /// </summary>
        // Изменить метод CreateBulletObject3D
        public void CreateBulletObject3D(Guid bulletId, Vector3D initialDirection)
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
        ///  Creates a new custom 3d of ENEMY PLAYER and adds it to viewport
        /// </summary>
        public void CreatePlayerObject3D(Guid playerId, Vector3D initialDirection)
        {
            var mesh = PlaneMesh(3.5, 3.5);

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
            player.Children.Add(CreatePlayerFX(worldPosition));         // parenting FX plane to the Enemy model, thus we can have an access to it.

        }

        /// <summary>
        ///  Creates a plane for blood effect on PlayerHit event
        /// </summary>
        private CustomObject3D CreatePlayerFX(Vector3D worldPosition)
        {
            var mesh = PlaneMesh(7.0, 7.0);

            var texture = texturePool.GetBloodFXTexture();
            var material = TextureMaterial(texture);
            var geometryModel = new GeometryModel3D(mesh, material);
            var bloodFXVisual = new ModelVisual3D { Content = geometryModel };


            var bloodFX = new CustomObject3D(bloodFXVisual,
                                           tag: CustomObject3DTags.Enemy);


            bloodFX.SetWorldPosition(worldPosition * 0.999);                           // place an effect a bit closer than the character is to avoid mesh overlapping
            Add3D(bloodFX);
            return bloodFX;
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


        public void Render() => renderer3D.Render();

    }
}