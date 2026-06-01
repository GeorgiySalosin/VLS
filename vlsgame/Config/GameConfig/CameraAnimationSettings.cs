using System;

namespace VLSGame.Config.GameConfig
{
    public class CameraAnimationSettings
    {
        // ----- Idle Sway  -----
        public double SwayAmplitude { get; set; } = 0.06;   // degree
        public double SwayFrequencyX { get; set; } = 0.47793;  // hertz
        public double SwayFrequencyY { get; set; } = 0.659;  // hertz

        // ----- Recoil -----
        /// <summary>
        /// the amount the camera jumps on shot (angular °)
        /// </summary>
        public double RecoilVerticalBase { get; set; } = 2.0;
        /// <summary>
        /// variation thresold of the amount the camera jumps on shot (angular °)
        /// </summary>
        public double RecoilVerticalRandom { get; set; } = 0.4;
        /// <summary>
        /// the max amount the camera jumps on shot (angular °)
        /// </summary>

        // the speed the camera jumps with on shot (angular °/ s)
        public double RecoilVerticalRiseSpeed { get; set; } = 60.0;
        /// <summary>
        /// the speed the camera is pulled back with after shot (angular °/ s)
        /// </summary>
        public double RecoilVerticalRecoverySpeed { get; set; } = 10.0;

        /// <summary>
        /// how much higher will the camera (target point) be after the shot than it was before it (angular °)
        /// </summary>
        public double RecoilVerticalRecoverShift { get; set; } = 0.2;

        /// <summary>
        /// the random thresold of the cameras horizontal offset on shot (angular °)
        /// </summary>
        public double RecoilHorizontalRange { get; set; } = 0.6;

        /// <summary>
        /// the max amount of the cameras horizontal offset on shot (angular °)
        /// </summary>
        public double RecoilHorizontalMaxDeg { get; set; } = 1.8;

        /// <summary>
        /// the speed of a horizontal component of recoil
        /// </summary>
        public double RecoilHorizontalInterpSpeed { get; set; } = 14.0;


        // ----- Zoom -----

        /// <summary>
        /// the default idle FOV (no zoom)
        /// </summary>
        public double DefaultFOV { get; set; } = 90;

        /// <summary>
        /// the target of zooming with RMB
        /// </summary>
        public double AimingFOV { get; set; } = 11.25;

        /// <summary>
        /// the step of changing fov automatically (when zooming in/out)
        /// </summary>
        public float ZoomSpeedAuto { get; set; } = 12.0f;

        /// <summary>
        /// the step of changing fov manually (with a wheel scroll)
        /// </summary>
        public float ZoomSpeedManual { get; set; } = 2.0f;

    }
}