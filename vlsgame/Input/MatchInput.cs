using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using VLSGame.Config;
using VLSGame.Rendering;
using VLSGame.Rendering.Layers;
using VLSGame.ViewModels;

namespace VLSGame.Input
{
    public sealed class MatchInput
    {
        private static readonly MatchInput _instance = new();
        public static MatchInput Instance => _instance;

        private MatchViewModel? _viewModel;
        private Window? _targetWindow;

        private Point _lastMousePosition;
        private DateTime _lastMoveTime;
        private Queue<double> _speedBuffer = new();

        private double _currentRotationX;
        private double _currentRotationY;

        private MatchInput() { }

        public void Initialize(MatchViewModel viewModel, Window targetWindow)
        {
            _viewModel = viewModel;
            _targetWindow = targetWindow;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (_targetWindow == null) return;

            _targetWindow.MouseDown += OnMouseDown;
            _targetWindow.MouseMove += OnMouseMove;
            _targetWindow.MouseUp += OnMouseUp;
            _targetWindow.MouseWheel += OnMouseWheel;
            _targetWindow.KeyDown += OnKeyDown;
        }

        public void UnsubscribeEvents()
        {
            if (_targetWindow == null) return;

            _targetWindow.MouseDown -= OnMouseDown;
            _targetWindow.MouseMove -= OnMouseMove;
            _targetWindow.MouseUp -= OnMouseUp;
            _targetWindow.MouseWheel -= OnMouseWheel;
            _targetWindow.KeyDown -= OnKeyDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null || _targetWindow == null) return;

            if (e.ChangedButton == MouseButton.Left)
            {
                _viewModel.IsDragging = true;
                _lastMousePosition = e.GetPosition(_targetWindow);
                _lastMoveTime = DateTime.Now;
                _speedBuffer.Clear();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel == null || _targetWindow == null || !_viewModel.IsDragging) return;

            Point currentPosition = e.GetPosition(_targetWindow);
            DateTime currentTime = DateTime.Now;

            double deltaX = currentPosition.X - _lastMousePosition.X;
            double deltaY = currentPosition.Y - _lastMousePosition.Y;
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            double timeDelta = (currentTime - _lastMoveTime).TotalMilliseconds;

            if (timeDelta > 0 && (Math.Abs(deltaX) > 0.1 || Math.Abs(deltaY) > 0.1))
            {
                double speed = distance / timeDelta;
                double adaptiveSensitivity = CalculateAdaptiveSensitivity(speed);

                _currentRotationY -= deltaX * adaptiveSensitivity;
                _currentRotationX -= deltaY * adaptiveSensitivity;

                // Blocking camera view if looking too low/high
                _currentRotationX = Math.Max(-Math.PI / 2 + Configuration.Instance.GameSettings.ClampVRotationMin,
                                            Math.Min(Math.PI / 2 - Configuration.Instance.GameSettings.ClampVRotationMax,
                                                    _currentRotationX));

                _viewModel.CameraProperties.RotationX = _currentRotationX;
                _viewModel.CameraProperties.RotationY = _currentRotationY;
            }

            _lastMousePosition = currentPosition;
            _lastMoveTime = currentTime;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            if (e.ChangedButton == MouseButton.Left)
            {
                _viewModel.IsDragging = false;
                _speedBuffer.Clear();
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
                _targetWindow?.Close();
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

            return Configuration.Instance.GameSettings.MouseSensitivity * (_viewModel.CameraProperties?.FieldOfView ?? 60.0) / 60.0 * sensitivityScale;
        }
    }
}