using System;

namespace VLSGame.Config
{
    public class CameraAnimationSettings
    {
        // ----- Idle Sway (лёгкое шевеление) -----
        public double SwayAmplitude { get; set; } = 0.06;   // градусы
        public double SwayFrequencyX { get; set; } = 0.47793;  // Гц
        public double SwayFrequencyY { get; set; } = 0.659;  // Гц

        // ----- Recoil (отдача) -----
        public double RecoilVerticalBase { get; set; } = 2.0;     // градусы
        public double RecoilVerticalRandom { get; set; } = 0.4;
        public double RecoilVerticalMaxDeg { get; set; } = 4.0;

        // Скорость нарастания подброса (градусов в секунду)
        public double RecoilVerticalRiseSpeed { get; set; } = 45.0;
        public double RecoilVerticalRecoverySpeed { get; set; } = 12.0; // град/сек

        // Степень ease-out для возврата (1 – линейно, 2 – квадратично, 3 – кубическое замедление)
        public double RecoilVerticalReturnEase { get; set; } = 2.0;

        public double RecoilVerticalRecoverShift { get; set; } = 0.2;   // насколько прицел после выстрела будет выше, чем был до него

        public double RecoilHorizontalRange { get; set; } = 0.6;     // градусы
        public double RecoilHorizontalMaxDeg { get; set; } = 1.8;
        public double RecoilHorizontalInterpSpeed { get; set; } = 14.0; // ед/сек

        // ----- Zoom -----
        public double DefaultFOV { get; set; } = 90;
        public double AimingFOV { get; set; } = 45;
        public float ZoomSpeed { get; set; } = 12.0f;

        public double RecoilVerticalMaxRad => RecoilVerticalMaxDeg * Math.PI / 180.0;
    }
}