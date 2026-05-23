using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using VLSGame.Config;
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

        private double currentRotationX;
        private double currentRotationY;

        private double smoothRotationX, smoothRotationY;
        private const double MouseSmoothing = 0.85; // 0 = нет сглаживания, 1 = максимальное

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
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (viewModel == null || window == null) return;



            Point currentPosition = e.GetPosition(window);
            DateTime currentTime = DateTime.Now;

            // Get screen center
            Point centerInWindow = new (window.ActualWidth / 2, window.ActualHeight / 2);
            
            double deltaX = currentPosition.X - centerInWindow.X;
            double deltaY = currentPosition.Y - centerInWindow.Y;
            
            if (Math.Abs(deltaX) > 0 || Math.Abs(deltaY) > 0)
            {
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                double timeDelta = (currentTime - lastMoveTime).TotalMilliseconds;
                
                double speed = distance / timeDelta;
                double adaptiveSensitivity = CalculateAdaptiveSensitivity(speed);
                
                currentRotationY -= deltaX * adaptiveSensitivity;
                currentRotationX -= deltaY * adaptiveSensitivity;

                //  Blocking camera view if looking too low/high
                //currentRotationX = Math.Max(-Math.PI / 2 + Configuration.Instance.GameSettings.ClampVRotationMin,
                //                            Math.Min(Math.PI / 2 - Configuration.Instance.GameSettings.ClampVRotationMax,
                //                                    currentRotationX));

                // Получаем текущее анимационное смещение (радианы)
                double animX = viewModel.CameraProperties.AnimationRotationX;

                // Общие границы для итогового угла
                double totalMin = -Math.PI / 2 + Configuration.Instance.GameSettings.ClampVRotationMin;
                double totalMax = Math.PI / 2 - Configuration.Instance.GameSettings.ClampVRotationMax;

                // Динамические границы для пользовательского угла
                double userMin = totalMin - animX;
                double userMax = totalMax - animX;

                currentRotationX = Math.Max(userMin, Math.Min(userMax, currentRotationX));

               

                // As for horizontal rotation - it's unrestricted
                viewModel.CameraProperties.UserRotationX = currentRotationX;
                viewModel.CameraProperties.UserRotationY = currentRotationY;



                // Return mouse to the center
                Point centerInScreen = window.PointToScreen(centerInWindow);
                SetCursorPos((int)centerInScreen.X, (int)centerInScreen.Y);

                // Last pos is screen center again
                lastMousePosition = centerInWindow;
                lastMoveTime = currentTime;
            }
            else
            {
                lastMousePosition = currentPosition;
                lastMoveTime = currentTime;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (viewModel == null) return;     // restrict zooming bypassing the RMB


            else if (e.ChangedButton == MouseButton.Right)
            {
                isAiming = false;
                viewModel.CameraProperties.TargetFOV = Configuration.Instance.CameraAnimationSettings.DefaultFOV;
            }
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

        private double CalculateAdaptiveSensitivity(double speed)
        {
            speedBuffer.Enqueue(speed);
            if (speedBuffer.Count > Configuration.Instance.GameSettings.SpeedBufferSize)
                speedBuffer.Dequeue();

            double smoothedSpeed = speedBuffer.Average();
            double sensitivityScale;

            if (smoothedSpeed <= Configuration.Instance.GameSettings.MinSpeedThreshold)
            {
                sensitivityScale = Configuration.Instance.GameSettings.MinSensitivityScale;
            }
            else if (smoothedSpeed >= Configuration.Instance.GameSettings.MaxSpeedThreshold)
            {
                sensitivityScale = 1.0;
            }
            else
            {
                double t = (smoothedSpeed - Configuration.Instance.GameSettings.MinSpeedThreshold) /
                          (Configuration.Instance.GameSettings.MaxSpeedThreshold - Configuration.Instance.GameSettings.MinSpeedThreshold);
                sensitivityScale = Configuration.Instance.GameSettings.MinSensitivityScale +
                                 (1.0 - Configuration.Instance.GameSettings.MinSensitivityScale) * (1 - Math.Pow(1 - t, 2));
            }

            return Configuration.Instance.GameSettings.MouseSensitivity * (viewModel?.CameraProperties?.FieldOfView ?? 90.0)/90.0 * sensitivityScale;       // if no data we use 90 as default fov
        }
    }
}