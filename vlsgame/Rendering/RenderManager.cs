using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSGame.Rendering.Content2D;
using VLSGame.Rendering.Content2D.HUD;
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
        private static readonly Renderer3D renderer3D = Renderer3D.Instance;
        private RenderManager() { }

        private static bool isInitialized = false;

        public void Initialize(Viewport3D viewport, Panel hudPanel)
        {
            if (isInitialized) return;
            renderer3D.Initialize(viewport);
            //renderer3D.AddObject();


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

        #region 3D 

        #region MODELS CREATION 

        public CustomObject3D CreateEnvironmentObject3D(BitmapSource? mapTexture)
        {
            GeometryModel3D geometryModel = new(SphereMesh(radius: 10), TextureMaterial(mapTexture));
            ModelVisual3D sphereVisual = new() { Content = geometryModel };

            CustomObject3D environment = new(sphereVisual, tag: "environment");
            return environment;
        }

        public CustomObject3D CreateBulletObject3D(Guid bulletId = new())
        {
            var mesh = PlaneMesh(.001, .001);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(@"Content\Animation\BallisticsFX\BulletDebug.png", UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            ImageSource imageSource = bitmap;

            var material = TextureMaterial(imageSource);
            var geometryModel = new GeometryModel3D(mesh, material);
            var bulletVisual = new ModelVisual3D { Content = geometryModel };

            
            var bullet = new CustomObject3D(bulletVisual, fixedDistance: 0.2, id: bulletId, tag: "bullet");
            return bullet;
        }

        #endregion

        public void Add3D(CustomObject3D obj) => renderer3D.AddObject(obj);
        public void Remove3D(Guid id) => renderer3D.RemoveObject(id);


        public void SetLight() => renderer3D.SetupLighting();


        #endregion


        public void Render()
        {
            renderer3D.Render();

        }

    }
}