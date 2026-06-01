using System.Windows.Media.Media3D;
using VLSGame.ViewModels;

namespace VLSGame.Rendering
{
    public class CameraProperties : ViewModelBase
    {
        private double _userRotationX;
        private double _userRotationY;
        private double _animationRotationX;
        private double _animationRotationY;
        private double _fieldOfView = 90;
        private double _targetFOV = 90;
        private Vector3D _lookDirection = new Vector3D(0, 0, 1);
        private bool _isDirty = false;

        public double RotationX => _userRotationX + _animationRotationX;
        public double RotationY => _userRotationY + _animationRotationY;

        public double UserRotationX
        {
            get => _userRotationX;
            set
            {
                if (Math.Abs(_userRotationX - value) > 1e-9)
                {
                    _userRotationX = value;
                    _isDirty = true;            // не вызываем уведомления сразу
                }
            }
        }

        public double UserRotationY
        {
            get => _userRotationY;
            set
            {
                if (Math.Abs(_userRotationY - value) > 1e-9)
                {
                    _userRotationY = value;
                    _isDirty = true;
                }
            }
        }

        public double AnimationRotationX
        {
            get => _animationRotationX;
            set
            {
                if (Math.Abs(_animationRotationX - value) > 1e-9)
                {
                    _animationRotationX = value;
                    _isDirty = true;
                }
            }
        }

        public double AnimationRotationY
        {
            get => _animationRotationY;
            set
            {
                if (Math.Abs(_animationRotationY - value) > 1e-9)
                {
                    _animationRotationY = value;
                    _isDirty = true;
                }
            }
        }

        public Vector3D LookDirection
        {
            get => _lookDirection;
            private set => Set(ref _lookDirection, value);
        }

        /// <summary>Вызывается ОДИН раз за кадр перед рендером.</summary>
        public void ApplyPendingChanges()
        {
            if (!_isDirty) return;

            _isDirty = false;

            // Извещаем байндинги об изменении итоговых углов и LookDirection
            OnPropertyChanged(nameof(RotationX));
            OnPropertyChanged(nameof(RotationY));
            RecalcLookDirection();        // считает и присваивает LookDirection
                                          // Можно добавить OnPropertyChanged(nameof(LookDirection)) внутри RecalcLookDirection
        }

        private void RecalcLookDirection()
        {
            double x = Math.Cos(RotationX) * Math.Sin(RotationY);
            double y = Math.Sin(RotationX);
            double z = Math.Cos(RotationX) * Math.Cos(RotationY);
            var v = new Vector3D(x, y, z);
            v.Normalize();
            LookDirection = v;    // вызовет PropertyChanged для LookDirection
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

    }
}