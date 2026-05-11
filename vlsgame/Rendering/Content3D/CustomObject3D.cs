using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.Rendering.Content3D;
using VLSGame.ViewModels;
using VLSShared.Models;

public sealed class CustomObject3D : ViewModelBase
{
    public readonly ModelVisual3D model;
    private readonly DiffuseMaterial? material;
    public Guid Id { get; }

    public CustomObject3DTags Tag { get; set; }

    public Animation Animation { get; init; } = new();

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
    private TranslateTransform3D? translateTransform = null;
    private ScaleTransform3D? scaleTransform = null;

    public List<CustomObject3D> Children { get; set; } = [];    // allows us to store related objects e.x. Enemy and its particle meshes for blood hit effect

    public CustomObject3D(ModelVisual3D model, Guid id = default, CustomObject3DTags tag = 0)
    {
        this.model = model;
        Id = id == default ? Guid.NewGuid() : id;
        Tag = tag;



        // Порядок: масштаб, поворот (матрица), перемещение
        scaleTransform = new ScaleTransform3D(1.0, 1.0, 1.0);
        transformGroup.Children.Add(scaleTransform);

        translateTransform = new TranslateTransform3D();
        transformGroup.Children.Add(translateTransform);

        model.Transform = transformGroup;

        if (model.Content is GeometryModel3D geometryModel && geometryModel.Material is DiffuseMaterial diffMaterial)
            material = diffMaterial;
    }

    /// <summary> Установка позиции в мировых координатах </summary>
    public void SetWorldPosition(Vector3D position)
    {
        if (translateTransform != null)
        {
            translateTransform.OffsetX = position.X;
            translateTransform.OffsetY = position.Y;
            translateTransform.OffsetZ = position.Z;
        }

        Vector3D toCamera = new (-position.X, -position.Y, -position.Z);
        toCamera.Normalize();
        LookAt(toCamera);
    }

    /// <summary> Установка равномерного масштаба </summary>
    public void SetScale(double scale)
    {
        if (scaleTransform != null)
        {
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;
            scaleTransform.ScaleZ = scale;
        }
    }

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

        if (currentMatrixTransform != null)
            transformGroup.Children.Remove(currentMatrixTransform);

        currentMatrixTransform = new MatrixTransform3D(matrix);
        transformGroup.Children.Insert(1, currentMatrixTransform);
    }

    public void SetTexture(ImageBrush brush)
    {
        if (material != null)
            material.Brush = brush;
    }
}