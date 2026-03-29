using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace VLSGame.ViewModels
{
    public class CameraProperties : ViewModelBase
    {
        private double _fieldOfView = 90;
        private double _rotationX = 0;
        private double _rotationY = 0;

        public Vector3D LookDirection => CalculateLookDirection();


        public double FieldOfView
        {
            get => _fieldOfView;
            set => Set(ref _fieldOfView, value);
        }

        public double RotationX
        {
            get => _rotationX;
            set
            {
                if (Set(ref _rotationX, value))
                    OnPropertyChanged(nameof(LookDirection));
            }
        }

        public double RotationY
        {
            get => _rotationY;
            set
            {
                if (Set(ref _rotationY, Math.Clamp(value, -89, 89)))
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
