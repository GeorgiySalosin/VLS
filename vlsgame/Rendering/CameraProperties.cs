using System.Windows.Media.Media3D;
using VLSGame.ViewModels;

namespace VLSGame.Rendering
{
    public class CameraProperties : ViewModelBase
    {
        private double _userRotationX = 0;
        private double _userRotationY = 0;
        private double _animationRotationX = 0;
        private double _animationRotationY = 0;
        private double _fieldOfView = 90;
        private double _targetFOV = 90;

        // Итоговые углы для камеры (сумма пользовательского ввода и анимаций)
        public double RotationX => UserRotationX + AnimationRotationX;
        public double RotationY => UserRotationY + AnimationRotationY;

        // Пользовательские углы (ввод мыши)
        public double UserRotationX
        {
            get => _userRotationX;
            set
            {
                if (Set(ref _userRotationX, value))
                {
                    OnPropertyChanged(nameof(RotationX));
                    OnPropertyChanged(nameof(LookDirection));
                }
            }
        }

        public double UserRotationY
        {
            get => _userRotationY;
            set
            {
                if (Set(ref _userRotationY, value))
                {
                    OnPropertyChanged(nameof(RotationY));
                    OnPropertyChanged(nameof(LookDirection));
                }
            }
        }

        // Анимационные углы (sway, recoil и т.д.)
        public double AnimationRotationX
        {
            get => _animationRotationX;
            set
            {
                if (Set(ref _animationRotationX, value))
                {
                    OnPropertyChanged(nameof(RotationX));
                    OnPropertyChanged(nameof(LookDirection));
                }
            }
        }

        public double AnimationRotationY
        {
            get => _animationRotationY;
            set
            {
                if (Set(ref _animationRotationY, value))
                {
                    OnPropertyChanged(nameof(RotationY));
                    OnPropertyChanged(nameof(LookDirection));
                }
            }
        }

        // Текущее поле зрения (отображаемое)
        public double FieldOfView
        {
            get => _fieldOfView;
            set => Set(ref _fieldOfView, value);
        }

        // Целевое поле зрения (для плавного приближения)
        public double TargetFOV
        {
            get => _targetFOV;
            set => Set(ref _targetFOV, value);  // Set вызывает PropertyChanged
        }

        /// <summary>
        /// Плавно изменяет текущий FOV в сторону целевого.
        /// Вызывается каждый кадр из игрового цикла.
        /// </summary>
        /// <param name="deltaTime">Время, прошедшее с предыдущего кадра (в секундах)</param>
        /// <param name="zoomSpeed">Скорость интерполяции (чем выше, тем быстрее)</param>
        public void UpdateFOV(float deltaTime, float zoomSpeed)
        {
            if (Math.Abs(FieldOfView - TargetFOV) < 0.01f)
                FieldOfView = TargetFOV;
            else
                FieldOfView += (TargetFOV - FieldOfView) * Math.Min(1f, zoomSpeed * deltaTime);
        }

        // Направление взгляда, вычисляемое на основе итоговых углов RotationX/RotationY
        public Vector3D LookDirection
        {
            get
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
}