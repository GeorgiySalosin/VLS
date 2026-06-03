using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.ViewModels;

namespace VLSGame.Rendering.Content3D
{
    public sealed class CustomObject3D : ViewModelBase
    {
        /// <summary>
        /// An object that will be rendered nto viewport
        /// </summary>
        public readonly ModelVisual3D model;
        /// <summary>
        /// A material that is set into 3d object, allows to change texture dynamically
        /// </summary>
        private readonly DiffuseMaterial? material;
        /// <summary>
        /// An indentifier
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// A tag according to which we consider how to render/what to do with the object
        /// </summary>
        public CustomObject3DTags Tag { get; set; }

        /// <summary>
        /// An additional property for easy texture switch per frame
        /// </summary>
        public Animation Animation { get; init; } = new();

        private bool isVisible = true;
        /// <summary>
        /// A property that results in realtime visibility toggle
        /// </summary>
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (Set(ref isVisible, value))
                    OnPropertyChanged(nameof(IsVisible));
            }
        }

        private readonly Transform3DGroup transformGroup = new();

        /// <summary>
        /// A field for storing object rotation
        /// </summary>
        private MatrixTransform3D? rotateTransform = null;
        /// <summary>
        /// A field for storing object translation
        /// </summary>
        private TranslateTransform3D? translateTransform = null;
        /// <summary>
        /// A field for storing object scale
        /// </summary>
        private ScaleTransform3D? scaleTransform = null;


        public CustomObject3D(ModelVisual3D model, Guid id = default, CustomObject3DTags tag = 0)
        {
            this.model = model;
            Id = id == default ? Guid.NewGuid() : id;
            Tag = tag;

            // append initial scale transform
            scaleTransform = new ScaleTransform3D(1.0, 1.0, 1.0);
            transformGroup.Children.Add(scaleTransform);

            // append initial translate transform
            translateTransform = new TranslateTransform3D();
            transformGroup.Children.Add(translateTransform);

            model.Transform = transformGroup;

            if (model.Content is GeometryModel3D geometryModel && geometryModel.Material is DiffuseMaterial diffMaterial)
                material = diffMaterial;
        }

        /// <summary> Sets 3d object position relative to world center </summary>
        public void SetWorldPosition(Vector3D position)
        {
            if (translateTransform != null)
            {
                translateTransform.OffsetX = position.X;
                translateTransform.OffsetY = position.Y;
                translateTransform.OffsetZ = position.Z;
            }

            Vector3D toCamera = new(-position.X, -position.Y, -position.Z);
            toCamera.Normalize();
            LookAt(toCamera);
        }

        /// <summary> Sets 3d object proportional scale (x=y=z)</summary>
        public void SetScale(double scale)
        {
            if (scaleTransform != null)
            {
                scaleTransform.ScaleX = scale;
                scaleTransform.ScaleY = scale;
                scaleTransform.ScaleZ = scale;
            }
        }

        /// <summary> Orients 3d object normally to given vector </summary>
        private void LookAt(Vector3D targetDir)
        {
            targetDir.Normalize();
            targetDir = -targetDir;
            Vector3D up = new(0, 1, 0);

            if (Math.Abs(Vector3D.DotProduct(targetDir, up)) > 0.9999)
                up = new(1, 0, 0);

            Vector3D right = Vector3D.CrossProduct(up, targetDir);
            right.Normalize();
            Vector3D realUp = Vector3D.CrossProduct(targetDir, right);
            realUp.Normalize();

            Matrix3D matrix = new(
                right.X, right.Y, right.Z, 0,
                realUp.X, realUp.Y, realUp.Z, 0,
                targetDir.X, targetDir.Y, targetDir.Z, 0,
                0, 0, 0, 1);

            if (rotateTransform != null)
                transformGroup.Children.Remove(rotateTransform);

            rotateTransform = new MatrixTransform3D(matrix);
            transformGroup.Children.Insert(1, rotateTransform);
        }

        /// <summary> Changes 3d object texture </summary>
        public void SetTexture(ImageBrush brush)
        {
            material?.Brush = brush;
        }
    }
}