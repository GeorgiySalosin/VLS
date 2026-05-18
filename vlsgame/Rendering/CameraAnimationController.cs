using System;
using VLSGame.Config;

namespace VLSGame.Rendering
{
    public class CameraAnimationController
    {
        // Заранее вычисленные константы
        private static readonly double DEG_TO_RAD = Math.PI / 180.0;
        private static readonly double TWO_PI = 2 * Math.PI;

        private readonly CameraProperties _camera;
        private readonly CameraAnimationSettings _settings;

        // Sway
        private double _swayPhaseX, _swayPhaseY;

        // Вертикальная отдача
        private double _verticalCurrent;
        private double _verticalTarget;
        private double _verticalRiseTarget;
        private bool _isRising;

        // Горизонтальная отдача
        private double _horizontalTarget;
        private double _horizontalCurrent;

        public CameraAnimationController(CameraProperties camera, CameraAnimationSettings settings)
        {
            _camera = camera;
            _settings = settings;
            _verticalTarget = 0;
            _verticalCurrent = 0;
        }

        public void Update(float deltaTime)
        {
            if (deltaTime <= 0) return;

            // ===== Sway =====
            double incX = _settings.SwayFrequencyX * TWO_PI * deltaTime;
            double incY = _settings.SwayFrequencyY * TWO_PI * deltaTime;
            _swayPhaseX += incX;
            _swayPhaseY += incY;
            if (_swayPhaseX > TWO_PI) _swayPhaseX -= TWO_PI;
            if (_swayPhaseY > TWO_PI) _swayPhaseY -= TWO_PI;

            double ampRad = _settings.SwayAmplitude * DEG_TO_RAD;
            double swayX = Math.Sin(_swayPhaseX) * ampRad;
            double swayY = Math.Cos(_swayPhaseY) * ampRad * 0.7;

            // ===== Подброс (rise) =====
            if (_isRising)
            {
                double riseRadPerSec = _settings.RecoilVerticalRiseSpeed * DEG_TO_RAD;
                double step = riseRadPerSec * deltaTime;
                if (_verticalCurrent < _verticalRiseTarget)
                {
                    _verticalCurrent += step;
                    if (_verticalCurrent >= _verticalRiseTarget)
                        _isRising = false;
                }
                else
                {
                    _isRising = false;
                }
            }

            // ===== Возврат с ease-out =====
            if (!_isRising && Math.Abs(_verticalCurrent - _verticalTarget) > 1e-6)
            {
                double recoveryRadPerSec = _settings.RecoilVerticalRecoverySpeed * DEG_TO_RAD;
                double maxStep = recoveryRadPerSec * deltaTime;
                double diff = _verticalTarget - _verticalCurrent;
                double distanceNorm = Math.Abs(diff) / (Math.Abs(_verticalTarget) + 0.001);
                double t = Math.Pow(distanceNorm, 1.0 / _settings.RecoilVerticalReturnEase);
                double step = maxStep * t;
                if (diff > 0)
                    _verticalCurrent = Math.Min(_verticalTarget, _verticalCurrent + step);
                else
                    _verticalCurrent = Math.Max(_verticalTarget, _verticalCurrent - step);
            }

            // ===== Горизонтальная отдача =====
            double diffH = _horizontalTarget - _horizontalCurrent;
            if (Math.Abs(diffH) > 1e-6)
            {
                double step = diffH * Math.Min(1.0, _settings.RecoilHorizontalInterpSpeed * deltaTime);
                _horizontalCurrent += step;
                if (Math.Abs(_horizontalTarget - _horizontalCurrent) < 0.0001)
                    _horizontalCurrent = _horizontalTarget;
            }

            // ===== Применяем итоговые углы =====
            _camera.AnimationRotationX = swayY + _verticalCurrent;
            _camera.AnimationRotationY = swayX + _horizontalCurrent;
        }

        public void TriggerRecoil()
        {
            double baseRad = _verticalCurrent;

            // Вертикаль
            double vertDeg = _settings.RecoilVerticalBase + Random.Shared.NextDouble() * _settings.RecoilVerticalRandom;
            double vertRad = vertDeg * DEG_TO_RAD;
            _verticalRiseTarget = baseRad + vertRad;
            _isRising = true;
            double recoverShiftRad = _settings.RecoilVerticalRecoverShift * DEG_TO_RAD;
            _verticalTarget = baseRad + recoverShiftRad;

            // Горизонталь
            double horizDeg = (Random.Shared.NextDouble() - 0.5) * _settings.RecoilHorizontalRange;
            double horizRad = horizDeg * DEG_TO_RAD;
            _horizontalTarget += horizRad;
            double maxHorizRad = _settings.RecoilHorizontalMaxDeg * DEG_TO_RAD;
            _horizontalTarget = Math.Clamp(_horizontalTarget, -maxHorizRad, maxHorizRad);
        }
    }
}