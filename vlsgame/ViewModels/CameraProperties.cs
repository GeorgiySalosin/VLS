
using System.Windows.Media.Media3D;
using VLSGame.Config;
using VLSGame.Config.GameConfig;

namespace VLSGame.ViewModels
{
    public class CameraProperties : ViewModelBase
    {
        private double userRotationX;
        private double userRotationY;
        private double animationRotationX;
        private double animationRotationY;
        private double fieldOfView = Configuration.Instance.Settings.DefaultFOV;
        private double targetFOV = Configuration.Instance.Settings.DefaultFOV;
        private Vector3D lookDirection = new (0, 0, 1);
        private bool isDirty = false;

        public double RotationX => userRotationX + animationRotationX;
        public double RotationY => userRotationY + animationRotationY;

        /// <summary> Total rotation accumulated by user mouse move  </summary>
        public double UserRotationX
        {
            get => userRotationX;
            set
            {
                if (Math.Abs(userRotationX - value) > 1e-9)
                {
                    userRotationX = value;
                    isDirty = true;
                }
            }
        }

        /// <summary> Total rotation accumulated by user mouse move  </summary>
        public double UserRotationY
        {
            get => userRotationY;
            set
            {
                if (Math.Abs(userRotationY - value) > 1e-9)
                {
                    userRotationY = value;
                    isDirty = true;
                }
            }
        }


        /// <summary> Total rotation accumulated by animation  </summary>
        public double AnimationRotationX
        {
            get => animationRotationX;
            set
            {
                if (Math.Abs(animationRotationX - value) > 1e-9)
                {
                    animationRotationX = value;
                    isDirty = true;
                }
            }
        }

        /// <summary> Total rotation accumulated by animation  </summary>
        public double AnimationRotationY
        {
            get => animationRotationY;
            set
            {
                if (Math.Abs(animationRotationY - value) > 1e-9)
                {
                    animationRotationY = value;
                    isDirty = true;
                }
            }
        }

        /// <summary> Current (realtime) lookdirection vector </summary>
        public Vector3D LookDirection
        {
            get => lookDirection;
            private set => Set(ref lookDirection, value);
        }

        /// <summary> Updates accumulated camera rotations. Called once per frame.</summary>
        public void UpdateCameraRotation()
        {
            if (!isDirty) return;

            isDirty = false;
            RecalcLookDirection();
        }

        private void RecalcLookDirection()
        {
            double x = Math.Cos(RotationX) * Math.Sin(RotationY);
            double y = Math.Sin(RotationX);
            double z = Math.Cos(RotationX) * Math.Cos(RotationY);
            var v = new Vector3D(x, y, z);
            v.Normalize();
            LookDirection = v;    // auto-call PropertyChanged for LookDirection
        }



        /// <summary> Current (realtime) fov value </summary>
        public double FieldOfView
        {
            get => fieldOfView;
            set => Set(ref fieldOfView, value);
        }

        /// <summary> Target for changing current fov value </summary>
        public double TargetFOV
        {
            get => targetFOV;
            set => Set(ref targetFOV, value);  
        }

        /// <summary> Smooth fov blend calculated each frame </summary>
        public void UpdateCameraFOV(float deltaTime, float zoomSpeed)
        {
            if (Math.Abs(FieldOfView - TargetFOV) < 0.01f)
                FieldOfView = TargetFOV;
            else
                FieldOfView += (TargetFOV - FieldOfView) * Math.Min(1f, zoomSpeed * deltaTime);
        }

    }
}