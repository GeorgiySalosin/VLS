using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSGame.Models;
using VLSGame.Rendering.Content2D;
using VLSGame.Rendering.Content2D.HUD;
using VLSGame.Rendering.Content3D;
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
        private static readonly MatchTexturePool texturePool = new();           // Pre-loading all in-game textures and reusing them!

        private RenderManager() { }

        private static bool isInitialized = false;

        public void Initialize(Viewport3D viewport, Panel hudPanel)
        {
            if (isInitialized) return;
            renderer3D.Initialize(viewport);


            RegisterLayer(new HudLayer(hudPanel));
            RegisterLayer(new HudLayer(hudPanel));


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

        /// <summary>
        ///  Creates a new custom 3d of environment sphere and adds it to viewport
        /// </summary>
        public void CreateEnvironmentObject3D(BitmapSource? mapTexture)
        {
            GeometryModel3D geometryModel = new(SphereMesh(radius: 10), TextureMaterial(mapTexture));
            ModelVisual3D sphereVisual = new() { Content = geometryModel };

            CustomObject3D environment = new(sphereVisual, tag: "environment");
            Add3D(environment);
        }


        /// <summary>
        ///  Creates a new custom 3d of bullet and adds it to viewport
        /// </summary>
        public void CreateBulletObject3D(Guid bulletId, Vector3D direction)
        {
            var mesh = PlaneMesh(.003, .003);

            var texture = texturePool.GetBulletTexture();

            var material = TextureMaterial(texture);
            var geometryModel = new GeometryModel3D(mesh, material);
            var bulletVisual = new ModelVisual3D { Content = geometryModel };

            
            var bullet = new CustomObject3D(bulletVisual, fixedDistance: 0.1, id: bulletId, tag: "bullet");
            bullet.UpdateOrbit(direction);
            Add3D(bullet);
        }

        /// <summary>
        ///  Updates a bullet 3d with new rotation transform and sets a new texture
        /// </summary>
        public void UpdateBulletObject3D(Guid bulletId, Vector3D direction)
        {
            var bullet = Get3D(bulletId);
            bullet.UpdateOrbit(direction);
            bullet.SetTexture(texturePool.GetBulletTexture());
        }

        /// <summary>
        ///  Creates and adds to viewport  a new white ambient light to represent a scene with its original colors
        /// </summary>
        public void SetLight()
        {
            var ambientLight = new ModelVisual3D
            {
                Content = new AmbientLight(Colors.White)
            };

            CustomObject3D ambientlight = new(ambientLight, tag: "light");

            Add3D(ambientlight);
        }

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




        #endregion


        public void Render() => renderer3D.Render();

    }
}