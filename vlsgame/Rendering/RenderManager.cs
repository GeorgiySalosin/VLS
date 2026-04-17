using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.Rendering.Content2D;
using VLSGame.Rendering.Content2D.HUD;
using VLSGame.Rendering.Content3D;
using VLSShared.Models;

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



        private readonly SortedDictionary<RenderOrder, Layer> Layers = [];



        public void RegisterLayer(Layer layer)
        {
            if (!Layers.ContainsKey(layer.Order))
            {
                Layers.Add(layer.Order, layer);
            }
        }

        public void Add3D(CustomObject3D obj) => renderer3D.AddObject(obj);
        public void Remove3D(Guid id) => renderer3D.RemoveObject(id);

        public void SetLight() => renderer3D.SetupLighting();



        public T? GetLayer<T>() where T : Layer => Layers.Values.OfType<T>().FirstOrDefault();


        public void Render()
        {
            renderer3D.Render();

        }

    }
}