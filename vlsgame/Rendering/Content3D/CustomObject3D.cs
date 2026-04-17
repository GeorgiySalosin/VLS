using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSGame.ViewModels;
namespace VLSGame.Rendering.Content3D
{
    /// <summary>
    /// An Object that contains  ModelVisual3D (GeometryModel3D With its Material)   and some functionality over it   
    /// </summary>
    public sealed class CustomObject3D(ModelVisual3D model, Guid id = new(), string tag = "default") : ViewModelBase      // we can either initialize and auto-create an ID
    {
        public readonly ModelVisual3D model = model;
        public Guid Id { get; } = id;
        public string Tag { get; set; } = tag;
        private bool isVisible = true;
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (Set(ref isVisible, value))
                    OnPropertyChanged(nameof(IsVisible));
            }
        }

        /// <summary>
        /// Sets a new texture for the mesh
        /// </summary>
        public void SetTexture(BitmapSource bitmap)
        {
            var mesh = (GeometryModel3D)model.Content;
            mesh.Material = Material.TextureMaterial(bitmap);
        }
        public void SetTransform(Transform3DGroup group)
        {
            //todo
        }
    }
}
