using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace VLSGame.Rendering
{
    public abstract class RenderLayer : IRenderLayer
    {
        private bool _isVisible = true;
        protected readonly List<IRenderable> _renderables = new();
        
        public string Name { get; }
        public RenderOrder Order { get; }
        public bool IsVisible 
        { 
            get => _isVisible;
            set => _isVisible = value;
        }
        
        protected RenderLayer(string name, RenderOrder order)
        {
            Name = name;
            Order = order;
        }
        
        public virtual void AddRenderable(IRenderable renderable)
        {
            _renderables.Add(renderable);
        }
        
        public virtual void RemoveRenderable(string id)
        {
            _renderables.RemoveAll(r => r.Id == id);
        }
        
        public virtual void Update(double deltaTime)
        {
            foreach (var renderable in _renderables.Where(r => r.IsVisible))
            {
                renderable.Update(deltaTime);
            }
        }
        
        public virtual void Render(Viewport3D viewport)
        {
            if (!IsVisible) return;
            
            foreach (var renderable in _renderables.Where(r => r.IsVisible))
            {
                renderable.Render();
                // Добавление модели в viewport обрабатывается в RenderManager
            }
        }
        
        public virtual void Clear()
        {
            _renderables.Clear();
        }
    }
}