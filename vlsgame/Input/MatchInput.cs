using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using VLSGame.Config;
using VLSGame.Rendering;
using VLSGame.Rendering.Layers;
using VLSGame.ViewModels;
using VLSShared.Models;

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
            (int x, int y) = viewModel.GetTextureCoordinatesFromDirection(viewModel.CameraProperties.LookDirection);
            Bullet bullet = new Bullet(x, y, viewModel.panoramaData);
            BulletManager.AddBullet(bullet);
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
                currentRotationX = Math.Max(-Math.PI / 2 + Configuration.Instance.GameSettings.ClampVRotationMin,
                                            Math.Min(Math.PI / 2 - Configuration.Instance.GameSettings.ClampVRotationMax,
                                                    currentRotationX));

                // As for horizontal rotation - it's unrestricted
                viewModel.CameraProperties.RotationX = currentRotationX;
                viewModel.CameraProperties.RotationY = currentRotationY;

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
            if (viewModel == null) return;

            if (e.ChangedButton == MouseButton.Left)
            {
                speedBuffer.Clear();

                window?.ReleaseMouseCapture();
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (viewModel == null) return;

            viewModel.CameraProperties.FieldOfView -= e.Delta * Configuration.Instance.GameSettings.ZoomSpeed;
            viewModel.CameraProperties.FieldOfView = Math.Max(Configuration.Instance.GameSettings.MinFOV,
                                             Math.Min(Configuration.Instance.GameSettings.MaxFOV,
                                                     viewModel.CameraProperties.FieldOfView));

            // Toggling HUD visibility
            var hudLayer = RenderManager.Instance.GetLayer<HudLayer>();
            if (viewModel.CameraProperties.FieldOfView < Configuration.Instance.GameSettings.MaxFOV)
                hudLayer?.HideElement("Crosshair");
            else
                hudLayer?.ShowElement("Crosshair");
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

            return Configuration.Instance.GameSettings.MouseSensitivity * (viewModel?.CameraProperties?.FieldOfView ?? 90.0)/90.0 * sensitivityScale;
        }
    }
}