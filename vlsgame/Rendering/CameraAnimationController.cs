using System;
using VLSGame.Config;
using VLSGame.Config.GameConfig;

namespace VLSGame.Rendering
{

    /// <summary>
    /// Adds calculated sway, recoil offsets to camera rotation values in real time
    /// </summary>
    public class CameraAnimationController(CameraProperties camera)
    {
        // Trying to win back a little performance
        private static readonly double DEG_TO_RAD = Math.PI / 180.0;
        private static readonly double TWO_PI = 2 * Math.PI;

        private readonly CameraProperties camera = camera;

        // Sway animation local coordinates
        private double swayPhaseX, swayPhaseY;

        // Vertical recoil components
        private double verticalCurrent = 0;
        private double verticalTarget = 0;
        private double verticalRiseTarget;
        private bool isRising;

        // Horizontal recoil components
        private double horizontalTarget;
        private double horizontalCurrent;

        
        /// <summary>
        /// The core of Camera Movement animation
        /// </summary>
        public void Update(float deltaTime)
        {
            if (deltaTime <= 0) return;

            // ===== Sway additive =====
            double incX = Configuration.Instance.Settings.SwayFrequencyX * TWO_PI * deltaTime;
            double incY = Configuration.Instance.Settings.SwayFrequencyY * TWO_PI * deltaTime;
            swayPhaseX += incX;
            swayPhaseY += incY;
            if (swayPhaseX > TWO_PI) swayPhaseX -= TWO_PI;
            if (swayPhaseY > TWO_PI) swayPhaseY -= TWO_PI;

            double ampRad = Configuration.Instance.Settings.SwayAmplitude * DEG_TO_RAD;
            double swayX = Math.Sin(swayPhaseX) * ampRad;
            double swayY = Math.Cos(swayPhaseY) * ampRad * 0.7;

            // ===== Rising stage (right after the shot) additive =====
            if (isRising)
            {
                double riseRadPerSec = Configuration.Instance.Settings.RecoilVerticalRiseSpeed * DEG_TO_RAD;
                double step = riseRadPerSec * deltaTime;
                if (verticalCurrent < verticalRiseTarget)
                {
                    verticalCurrent += step;
                    if (verticalCurrent >= verticalRiseTarget)
                        isRising = false;
                }
                else
                {
                    isRising = false;
                }
            }

            // ===== Pulling back stage additive =====
            if (!isRising && Math.Abs(verticalCurrent - verticalTarget) > 1e-6)
            {
                double t = Math.Min(1.0, Configuration.Instance.Settings.RecoilVerticalRecoverySpeed * deltaTime);
                verticalCurrent += (verticalTarget - verticalCurrent) * t;
            }

            // ===== Horizontal offset additive =====
            double diffH = horizontalTarget - horizontalCurrent;
            if (Math.Abs(diffH) > 1e-6)
            {
                double step = diffH * Math.Min(1.0, Configuration.Instance.Settings.RecoilHorizontalInterpSpeed * deltaTime);
                horizontalCurrent += step;
                if (Math.Abs(horizontalTarget - horizontalCurrent) < 0.0001)
                    horizontalCurrent = horizontalTarget;
            }

            
            camera.AnimationRotationX = swayY + verticalCurrent;
            camera.AnimationRotationY = swayX + horizontalCurrent;
        }

        /// <summary>
        /// The beginning of recoil animation
        /// </summary>
        public void TriggerRecoil()
        {
            double baseRad = verticalCurrent;

            // VerticalRecoilComponent
            double vertDeg = Configuration.Instance.Settings.RecoilVerticalBase + Random.Shared.NextDouble() * Configuration.Instance.Settings.RecoilVerticalRandom;
            double vertRad = vertDeg * DEG_TO_RAD;
            verticalRiseTarget = baseRad + vertRad;
            isRising = true;
            double recoverShiftRad = Configuration.Instance.Settings.RecoilVerticalRecoverShift * DEG_TO_RAD;
            verticalTarget = baseRad + recoverShiftRad;

            // HorizontalRecoilComponent
            double horizDeg = (Random.Shared.NextDouble() - 0.5) * Configuration.Instance.Settings.RecoilHorizontalRange;
            double horizRad = horizDeg * DEG_TO_RAD;
            horizontalTarget += horizRad;
            double maxHorizRad = Configuration.Instance.Settings.RecoilHorizontalMaxDeg * DEG_TO_RAD;
            horizontalTarget = Math.Clamp(horizontalTarget, -maxHorizRad, maxHorizRad);
        }
    }
}