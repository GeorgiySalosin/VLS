using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using VLSGame.Config;
using VLSGame.Rendering;
using VLSGame.ViewModels;

namespace VLSGame.Input
{
    public sealed class MatchInput
    {
        private static readonly MatchInput instance = new();
        public static MatchInput Instance => instance;

        private MatchViewModel? viewModel;
        private Window? window;

        private readonly Stopwatch stopwatch = Stopwatch.StartNew();
        private long lastTimestamp;
        private double smoothedSpeed;
        private bool isAiming;

        // Защита от рекурсии
        private bool isMovingCursor;
        private const double MIN_MOVE_THRESHOLD = 0.5;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        private MatchInput() { }

        public void Initialize(MatchViewModel viewModel, Window window)
        {
            this.viewModel = viewModel;
            this.window = window;
            SubscribeEvents();
            ShowCursor(false);
        }

        private void SubscribeEvents()
        {
            if (window == null) return;
            window.Loaded += OnWindowLoaded;
            window.MouseDown += OnMouseDown;
            window.MouseMove += OnMouseMove;
            window.MouseUp += OnMouseUp;
            window.MouseWheel += OnMouseWheel;
            window.KeyDown += OnKeyDown;
        }

        public void UnsubscribeEvents()
        {
            if (window == null) return;
            window.Loaded -= OnWindowLoaded;
            window.MouseDown -= OnMouseDown;
            window.MouseMove -= OnMouseMove;
            window.MouseUp -= OnMouseUp;
            window.MouseWheel -= OnMouseWheel;
            window.KeyDown -= OnKeyDown;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (window == null) return;
            Point center = new(window.ActualWidth / 2, window.ActualHeight / 2);
            Point screenCenter = window.PointToScreen(center);
            SetCursorPos((int)screenCenter.X, (int)screenCenter.Y);
            lastTimestamp = stopwatch.Elapsed.Ticks;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (viewModel == null) return;
            if (e.ChangedButton == MouseButton.Left) viewModel.Shoot();
            else if (e.ChangedButton == MouseButton.Right)
            {
                isAiming = true;
                viewModel.CameraProperties.TargetFOV = Configuration.Instance.CameraAnimationSettings.AimingFOV;
                RenderManager.Instance.StartScopeAnimationForward();
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (viewModel == null) return;
            if (e.ChangedButton == MouseButton.Right)
            {
                isAiming = false;
                viewModel.CameraProperties.TargetFOV = Configuration.Instance.CameraAnimationSettings.DefaultFOV;
                RenderManager.Instance.StartScopeAnimationBackward();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // Защита от рекурсии
            if (isMovingCursor || viewModel == null || window == null) return;

            Point currentPos = e.GetPosition(window);
            Point center = new(window.ActualWidth / 2, window.ActualHeight / 2);

            double dx = currentPos.X - center.X;
            double dy = currentPos.Y - center.Y;

            // Игнорируем микро‑смещения после SetCursorPos
            if (Math.Abs(dx) < MIN_MOVE_THRESHOLD && Math.Abs(dy) < MIN_MOVE_THRESHOLD)
                return;

            // === Ваша оригинальная адаптивная чувствительность ===
            long currentTicks = stopwatch.Elapsed.Ticks;
            long tickDelta = currentTicks - lastTimestamp;
            if (tickDelta <= 0) tickDelta = 1;
            double timeDeltaMs = tickDelta / (double)Stopwatch.Frequency * 1000.0;
            lastTimestamp = currentTicks;

            double distance = Math.Sqrt(dx * dx + dy * dy);
            double speed = distance / timeDeltaMs;

            const double SmoothingFactor = 0.2;
            smoothedSpeed += (speed - smoothedSpeed) * SmoothingFactor;

            double sensitivityScale = CalculateAdaptiveSensitivity(smoothedSpeed);
            double baseSens = Configuration.Instance.GameSettings.MouseSensitivity *
                              (viewModel.CameraProperties.FieldOfView / 90.0);
            double sensitivity = baseSens * sensitivityScale;

            double newRotY = viewModel.CameraProperties.UserRotationY - dx * sensitivity;
            double newRotX = viewModel.CameraProperties.UserRotationX - dy * sensitivity;

            // Вертикальный clamp с учётом анимации
            double animX = viewModel.CameraProperties.AnimationRotationX;
            var cfg = Configuration.Instance.GameSettings;
            double totalMin = -Math.PI / 2 + cfg.ClampVRotationMin;
            double totalMax = Math.PI / 2 - cfg.ClampVRotationMax;
            double userMin = totalMin - animX;
            double userMax = totalMax - animX;
            newRotX = Math.Max(userMin, Math.Min(userMax, newRotX));

            viewModel.CameraProperties.UserRotationX = newRotX;
            viewModel.CameraProperties.UserRotationY = newRotY;
            // === конец математики ===

            // --- Возврат курсора с отпиской от события ---
            isMovingCursor = true;
            window.MouseMove -= OnMouseMove;          // убираем обработчик

            Point screenCenter = window.PointToScreen(center);
            SetCursorPos((int)screenCenter.X, (int)screenCenter.Y);

            window.MouseMove += OnMouseMove;          // возвращаем обработчик
            isMovingCursor = false;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (viewModel == null || !isAiming) return;
            double delta = e.Delta * Configuration.Instance.CameraAnimationSettings.ZoomSpeedManual / 120.0;
            double newTarget = viewModel.CameraProperties.TargetFOV - delta;
            newTarget = Math.Max(Configuration.Instance.GameSettings.MinFOVScope,
                                 Math.Min(Configuration.Instance.GameSettings.MaxFOVScope, newTarget));
            viewModel.CameraProperties.TargetFOV = newTarget;
            Configuration.Instance.CameraAnimationSettings.AimingFOV = newTarget;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) window?.Close();
        }

        private double CalculateAdaptiveSensitivity(double speed)
        {
            var cfg = Configuration.Instance.GameSettings;
            if (speed <= cfg.MinSpeedThreshold)
                return cfg.MinSensitivityScale;
            if (speed >= cfg.MaxSpeedThreshold)
                return 1.0;

            double t = (speed - cfg.MinSpeedThreshold) /
                       (cfg.MaxSpeedThreshold - cfg.MinSpeedThreshold);
            return cfg.MinSensitivityScale +
                   (1.0 - cfg.MinSensitivityScale) * (1 - Math.Pow(1 - t, 2));
        }
    }
}