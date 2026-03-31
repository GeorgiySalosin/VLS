using System.Windows.Media.Media3D;

namespace VLSGame.ViewModels
{
    public class CameraProperties : ViewModelBase
    {
        private double fieldOfView = 90;
        private double rotationX = 0;
        private double rotationY = 0;

        public Vector3D LookDirection => CalculateLookDirection();

        public double FieldOfView
        {
            get => fieldOfView;
            set => Set(ref fieldOfView, value);
        }

        public double RotationX
        {
            get => rotationX;
            set
            {
                if (Set(ref rotationX, value))
                    OnPropertyChanged(nameof(LookDirection));
            }
        }

        public double RotationY
        {
            get => rotationY;
            set
            {
                if (Set(ref rotationY, value))
                    OnPropertyChanged(nameof(LookDirection));
            }
        }


        private Vector3D CalculateLookDirection()
        {
            double x = Math.Cos(RotationX) * Math.Sin(RotationY);
            double y = Math.Sin(RotationX);
            double z = Math.Cos(RotationX) * Math.Cos(RotationY);

            Vector3D vec = new Vector3D(x, y, z);
            vec.Normalize();

            return vec;
        }
    }
}
