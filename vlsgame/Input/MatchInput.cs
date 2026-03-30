using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;
using VLSGame.Config;
using VLSGame.Rendering;
using VLSGame.Rendering.Layers;
using VLSGame.ViewModels;

namespace VLSGame.Input
{
    /// <summary>
    /// A class for handling all things occuring by user input
    /// </summary>
    public sealed class MatchInput
    {
        private static readonly MatchInput _instance = new();
        public static MatchInput Instance => _instance;

        private MatchViewModel? _viewModel;
        private Window? _window;

        private Point _lastMousePosition;
        private DateTime _lastMoveTime;
        private Queue<double> _speedBuffer = new();


        private double _currentRotationX;
        private double _currentRotationY;

        // WinAPI 
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);

        private MatchInput() { }

        public void Initialize(MatchViewModel viewModel, Window window)
        {
            _viewModel = viewModel;
            _window = window;

            SubscribeEvents();
            ShowCursor(false);

            
        }

        private void SubscribeEvents()
        {
            if (_window == null) return;

            _window.Loaded += OnWindowLoaded;
            _window.MouseDown += OnMouseDown;
            _window.MouseMove += OnMouseMove;
            _window.MouseUp += OnMouseUp;
            _window.MouseWheel += OnMouseWheel;
            _window.KeyDown += OnKeyDown;
        }
        public void UnsubscribeEvents()
        {
            if (_window == null) return;

            _window.Loaded -= OnWindowLoaded;
            _window.MouseDown -= OnMouseDown;
            _window.MouseMove -= OnMouseMove;
            _window.MouseUp -= OnMouseUp;
            _window.MouseWheel -= OnMouseWheel;
            _window.KeyDown -= OnKeyDown;
        }

        //  Resolving mouse teleportation occuring on window loaded.
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (_window == null) return;

            Point centerInWindow = new Point(_window.ActualWidth / 2, _window.ActualHeight / 2);
            Point centerInScreen = _window.PointToScreen(centerInWindow);
            SetCursorPos((int)centerInScreen.X, (int)centerInScreen.Y);

            _lastMousePosition = centerInWindow;
            _lastMoveTime = DateTime.Now;
        }



        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Мир настолько очистился, что пока здесь ничего не выполняется.
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel == null || _window == null) return;

            Point currentPosition = e.GetPosition(_window);
            DateTime currentTime = DateTime.Now;

            // Get screen center
            Point centerInWindow = new Point(_window.ActualWidth / 2, _window.ActualHeight / 2);

            
            double deltaX = currentPosition.X - centerInWindow.X;
            double deltaY = currentPosition.Y - centerInWindow.Y;

            
            if (Math.Abs(deltaX) > 0 || Math.Abs(deltaY) > 0)
            {
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                double timeDelta = (currentTime - _lastMoveTime).TotalMilliseconds;

                
                double speed = distance / timeDelta;
                double adaptiveSensitivity = CalculateAdaptiveSensitivity(speed);

                
                _currentRotationY -= deltaX * adaptiveSensitivity;
                _currentRotationX -= deltaY * adaptiveSensitivity;

                //  Blocking camera view if looking too low/high
                _currentRotationX = Math.Max(-Math.PI / 2 + Configuration.Instance.GameSettings.ClampVRotationMin,
                                            Math.Min(Math.PI / 2 - Configuration.Instance.GameSettings.ClampVRotationMax,
                                                    _currentRotationX));


                // As for horizontal rotation - it's unrestricted
                _viewModel.CameraProperties.RotationX = _currentRotationX;
                _viewModel.CameraProperties.RotationY = _currentRotationY;


                // Return mouse to the center
                Point centerInScreen = _window.PointToScreen(centerInWindow);
                SetCursorPos((int)centerInScreen.X, (int)centerInScreen.Y);

                // Last pos is screen center again
                _lastMousePosition = centerInWindow;
                _lastMoveTime = currentTime;
            }
            else
            {
                _lastMousePosition = currentPosition;
                _lastMoveTime = currentTime;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            if (e.ChangedButton == MouseButton.Left)
            {
                _speedBuffer.Clear();

                _window?.ReleaseMouseCapture();
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_viewModel == null) return;

            _viewModel.CameraProperties.FieldOfView -= e.Delta * Configuration.Instance.GameSettings.ZoomSpeed;
            _viewModel.CameraProperties.FieldOfView = Math.Max(Configuration.Instance.GameSettings.MinFOV,
                                             Math.Min(Configuration.Instance.GameSettings.MaxFOV,
                                                     _viewModel.CameraProperties.FieldOfView));

            // Toggling HUD visibility
            var hudLayer = RenderManager.Instance.GetLayer<HudLayer>();
            if (_viewModel.CameraProperties.FieldOfView < Configuration.Instance.GameSettings.MaxFOV)
                hudLayer?.HideElement("Crosshair");
            else
                hudLayer?.ShowElement("Crosshair");
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _window?.Close();
            }
        }

        private double CalculateAdaptiveSensitivity(double speed)
        {
            _speedBuffer.Enqueue(speed);
            if (_speedBuffer.Count > Configuration.Instance.GameSettings.SpeedBufferSize)
                _speedBuffer.Dequeue();

            double smoothedSpeed = _speedBuffer.Average();
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

            return Configuration.Instance.GameSettings.MouseSensitivity * (_viewModel?.CameraProperties?.FieldOfView ?? 90.0)/90.0 * sensitivityScale;
        }
    }
}