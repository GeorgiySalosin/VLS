using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.ViewModels;

namespace VLSGame.Rendering.Content3D
{
    public sealed class CustomObject3D : ViewModelBase // todo: remove inheritance
    {
        public readonly ModelVisual3D model;
        private readonly DiffuseMaterial? material;
        public Guid Id { get; }
        public CustomObject3DTags Tag { get; set; }

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

        private readonly Transform3DGroup transformGroup = new();
        private MatrixTransform3D? currentMatrixTransform = null;
        private readonly double fixedDistance; // just a fixed distance from axis

        /// <summary>
        /// An Object that contains  ModelVisual3D (GeometryModel3D With its Material)   and some functionality over it   
        /// </summary>
        public CustomObject3D(ModelVisual3D model, double fixedDistance = 0, Guid id = default, CustomObject3DTags tag = 0)
        {
            this.model = model;
            this.fixedDistance = fixedDistance;
            Id = id == default ? Guid.NewGuid() : id;
            Tag = tag;
            model.Transform = transformGroup;

            // If our model contains GeometryModel3D automatically save its material to reuse it when loading a new texture

            if (model.Content is GeometryModel3D geometryModel && geometryModel.Material is DiffuseMaterial diffMaterial)
            {
                material = diffMaterial;
            }
            // For such objects as light we will end up with null material
        }

        /// <summary>
        /// Обновляет положение объекта на сфере фиксированного радиуса в заданном направлении.
        /// Вызывайте этот метод при изменении направления камеры.
        /// </summary>
        //public void UpdateOrbit(Vector3 dir)
        //{
        //    Vector3D direction = new(dir.X, dir.Y, dir.Z);
        //    direction.Normalize();
        //    // Позиция = direction * фиксированное расстояние
        //    Vector3D newPosition = direction * fixedDistance;

        //    // Обновляем трансформацию перемещения
        //    var translate = GetOrAddTransform<TranslateTransform3D>();
        //    translate.OffsetX = newPosition.X;
        //    translate.OffsetY = newPosition.Y;
        //    translate.OffsetZ = newPosition.Z;

        //    LookAt(direction);
        //}

        /// <summary>
        /// Rotates an object so that look vector is normal to it
        /// </summary>
        //private void LookAt(Vector3D targetDir)
        //{
        //    //Vector3D targetDir = new (dir.X, dir.Y, dir.Z);
        //    //targetDir.Normalize();

        //    // World vec "up"
        //    Vector3D up = new (0, 1, 0);

        //    // Right vector (_|_ targetDir, right)
        //    Vector3D right = Vector3D.CrossProduct(up, targetDir);
        //    right.Normalize();

        //    // Recalculate up vector to make it orthogonal to targetDir , right
        //    Vector3D realUp = Vector3D.CrossProduct(targetDir, right);
        //    realUp.Normalize();

        //    // Build rotation matrix: rows — right, realUp, targetDir (columns — X,Y,Z local space)
        //    Matrix3D matrix = new (
        //        right.X, right.Y, right.Z, 0,
        //        realUp.X, realUp.Y, realUp.Z, 0,
        //        targetDir.X, targetDir.Y, targetDir.Z, 0,
        //        0, 0, 0, 1);

        //    var matrixTransform = new MatrixTransform3D(matrix);

        //    // remove previous rotation transform and add a new one from matrix
        //    {
        //        var toRemove = transformGroup.Children.OfType<RotateTransform3D>().ToList();
        //        foreach (var rot in toRemove)
        //            transformGroup.Children.Remove(rot);
        //    }

        //    transformGroup.Children.Insert(0, matrixTransform);
        //}

        #region TRANSFORMATION 
        public void UpdateOrbit(Vector3D direction)
        {
            direction.Normalize();
            Vector3D newPosition = direction * fixedDistance;

            var translate = GetOrAddTransform<TranslateTransform3D>();
            translate.OffsetX = newPosition.X;
            translate.OffsetY = newPosition.Y;
            translate.OffsetZ = newPosition.Z;

            LookAt(direction);
        }

        private void LookAt(Vector3D targetDir)
        {
            Vector3D up = new(0, 1, 0);
            Vector3D right = Vector3D.CrossProduct(up, targetDir);
            right.Normalize();
            Vector3D realUp = Vector3D.CrossProduct(targetDir, right);
            realUp.Normalize();

            Matrix3D matrix = new(
                right.X, right.Y, right.Z, 0,
                realUp.X, realUp.Y, realUp.Z, 0,
                targetDir.X, targetDir.Y, targetDir.Z, 0,
                0, 0, 0, 1);

            if (currentMatrixTransform != null)
                transformGroup.Children.Remove(currentMatrixTransform);

            currentMatrixTransform = new MatrixTransform3D(matrix);
            transformGroup.Children.Insert(0, currentMatrixTransform);
        }


        private T GetOrAddTransform<T>() where T : Transform3D, new()
        {
            foreach (Transform3D t in transformGroup.Children)
                if (t is T existing)
                    return existing;

            T newTransform = new T();
            transformGroup.Children.Add(newTransform);
            return newTransform;
        }
        #endregion

        /// <summary>
        /// Dynamically set a texture by imagebrush
        /// </summary>
        public void SetTexture(ImageBrush brush)
        {
            material?.Brush = brush;
        }

        /// <summary>
        /// DEPRECATED
        /// </summary>
        //public void SetTexture(BitmapSource bitmap)
        //{
        //    var brush = new ImageBrush(bitmap)
        //    {
        //        ViewportUnits = BrushMappingMode.Absolute,
        //        TileMode = TileMode.None,
        //        Stretch = Stretch.Fill
        //    };
        //    brush.Freeze(); 
        //    material.Brush = brush;
        //}

    }
}