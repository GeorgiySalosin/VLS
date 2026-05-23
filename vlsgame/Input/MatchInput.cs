using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using VLSGame.Config;
using VLSGame.Rendering;
using VLSGame.ViewModels;

namespace VLSGame.Input
{
    /// <summary>
    /// A class for handling all things occuring by user input
    /// </summary>
    public sealed class MatchInput
    {
        private static readonly MatchInput instance = new();
        public static MatchInput Instance => instance;

        private MatchViewModel? viewModel;
        private Window? window;

        private Point lastMousePosition;
        private DateTime lastMoveTime;
        private readonly Queue<double> speedBuffer = new();



        private double accumulatedDeltaX;
        private double accumulatedDeltaY;
        private readonly object deltaLock = new();

        // Вместо Queue<double> speedBuffer:
        private double smoothedSpeed = 0.0;
        // Для расчёта реального времени между кадрами (если хотим точную скорость)
        private DateTime lastApplyTime = DateTime.Now;


        private bool isAiming = false;

        // WinAPI methods
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

        // This method is generally used for resolving mouse teleportation occuring on window loaded.
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (window == null) return;

            Point centerInWindow = new (window.ActualWidth / 2, window.ActualHeight / 2);
            Point centerInScreen = window.PointToScreen(centerInWindow);
            SetCursorPos((int)centerInScreen.X, (int)centerInScreen.Y);

            lastMousePosition = centerInWindow;
            lastMoveTime = DateTime.Now;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (viewModel == null) return;
            if (e.ChangedButton == MouseButton.Left)
            {
                viewModel.Shoot();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                isAiming = true;
                viewModel.CameraProperties.TargetFOV = Configuration.Instance.CameraAnimationSettings.AimingFOV;
                RenderManager.Instance.StartScopeAnimationForward();
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (viewModel == null) return;     // restrict zooming bypassing the RMB


            if (e.ChangedButton == MouseButton.Right)
            {
                isAiming = false;
                viewModel.CameraProperties.TargetFOV = Configuration.Instance.CameraAnimationSettings.DefaultFOV;
                RenderManager.Instance.StartScopeAnimationBackward();
            }
        }


        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (viewModel == null || window == null) return;

            Point currentPosition = e.GetPosition(window);
            Point centerInWindow = new(window.ActualWidth / 2, window.ActualHeight / 2);

            double dx = currentPosition.X - centerInWindow.X;
            double dy = currentPosition.Y - centerInWindow.Y;

            if (Math.Abs(dx) > 0 || Math.Abs(dy) > 0)
            {
                DateTime now = DateTime.Now;
                double timeDelta = (now - lastMoveTime).TotalMilliseconds;
                if (timeDelta <= 0) timeDelta = 1; // защита от нуля

                double distance = Math.Sqrt(dx * dx + dy * dy);
                double speed = distance / timeDelta;          // пикселей в миллисекунду

                // Экспоненциальное сглаживание (вместо очереди)
                const double SmoothingFactor = 0.2;
                smoothedSpeed = smoothedSpeed + (speed - smoothedSpeed) * SmoothingFactor;

                // Адаптивная чувствительность (точь-в-точь оригинал)
                double sensitivityScale = CalculateAdaptiveSensitivity(smoothedSpeed);

                double baseSens = Configuration.Instance.GameSettings.MouseSensitivity
                                * ((viewModel?.CameraProperties?.FieldOfView ?? 90.0) / 90.0);
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

                // Мгновенное присвоение без уведомлений (флаг dirty внутри CameraProperties)
                viewModel.CameraProperties.UserRotationX = newRotX;
                viewModel.CameraProperties.UserRotationY = newRotY;

                // Возврат курсора
                Point centerInScreen = window.PointToScreen(centerInWindow);
                SetCursorPos((int)centerInScreen.X, (int)centerInScreen.Y);
                lastMousePosition = centerInWindow;
                lastMoveTime = now;
            }
            else
            {
                lastMousePosition = currentPosition;
                lastMoveTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Вызывается ОДИН раз за кадр из игрового цикла.
        /// Применяет накопленное движение с адаптивной чувствительностью.
        /// </summary>
        public void ApplyMouseInput(CameraProperties camera, double frameDeltaTime)
        {
            double dx, dy;
            lock (deltaLock)
            {
                dx = accumulatedDeltaX;
                dy = accumulatedDeltaY;
                accumulatedDeltaX = 0;
                accumulatedDeltaY = 0;
            }

            if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
                return;

            // Реальная скорость мыши (пикселей в секунду)
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double instantSpeed = distance / frameDeltaTime; // frameDeltaTime > 0

            // Экспоненциальное сглаживание скорости (замена очереди)
            const double smoothing = 0.2;
            smoothedSpeed += (instantSpeed - smoothedSpeed) * smoothing;

            // Адаптивная чувствительность (ваш оригинальный алгоритм)
            double sensitivityScale = CalculateAdaptiveSensitivity(smoothedSpeed);

            double baseSens = Configuration.Instance.GameSettings.MouseSensitivity
                            * (camera.FieldOfView / 90.0);
            double sensitivity = baseSens * sensitivityScale;

            double newRotY = camera.UserRotationY - dx * sensitivity;
            double newRotX = camera.UserRotationX - dy * sensitivity;

            // Вертикальный clamp с учётом анимации
            double animX = camera.AnimationRotationX;
            var cfg = Configuration.Instance.GameSettings;
            double totalMin = -Math.PI / 2 + cfg.ClampVRotationMin;
            double totalMax = Math.PI / 2 - cfg.ClampVRotationMax;
            double userMin = totalMin - animX;
            double userMax = totalMax - animX;
            newRotX = Math.Max(userMin, Math.Min(userMax, newRotX));

            camera.UserRotationX = newRotX;
            camera.UserRotationY = newRotY;
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


        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (viewModel == null || !isAiming) return;     // restrict zooming in/out with mouse wheel
            double delta = e.Delta * Configuration.Instance.CameraAnimationSettings.ZoomSpeedManual / 120.0;        // why tf is divided by 120?     whatever.. works nice
            double newTarget = viewModel.CameraProperties.TargetFOV - delta;
            newTarget = Math.Max(Configuration.Instance.GameSettings.MinFOVScope,
                                 Math.Min(Configuration.Instance.GameSettings.MaxFOVScope, newTarget));
            viewModel.CameraProperties.TargetFOV = newTarget;
            Configuration.Instance.CameraAnimationSettings.AimingFOV = newTarget;       // the next time scope will auto zoom up to previously set level
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                window?.Close();
            }
        }

    }
}