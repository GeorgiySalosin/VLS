// Match.xaml.cs (исправленная версия)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VLSGame.Config;
using VLSGame.Rendering;
using VLSGame.Rendering.HUD;
using VLSGame.Rendering.Layers;
using VLSGame.ViewModels;

namespace VLSGame
{
    public partial class Match : Window
    {
        private readonly MatchViewModel _viewModel;

        private Point _lastMousePosition;
        private DateTime _lastMoveTime;
        private Queue<double> _speedBuffer = new Queue<double>();

        private double _currentRotationX;
        private double _currentRotationY;

        public Match(MatchViewModel viewModel)
        {
            InitializeComponent();

            
            string configPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"Config\GameSettings.json");

            if (!Configuration.Instance.LoadConfiguration(configPath))
            {
                MessageBox.Show("File error: GameSettings.json", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            _viewModel = viewModel;
            DataContext = _viewModel;

            RenderManager.Instance.Initialize(MainViewport, HudPanel);

            CreateEnvironment();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            Loaded += OnLoaded;
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            MouseWheel += OnMouseWheel;
            KeyDown += OnKeyDown;
            CompositionTarget.Rendering += OnRendering;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _currentRotationX = _viewModel.RotationX;
            _currentRotationY = _viewModel.RotationY;

            _viewModel.UpdateCameraRotation(MainCamera, _currentRotationX, _currentRotationY); // initial update
            SetupHud();
        }


        // Backrgound layer
        public void CreateEnvironment()
        {
            var worldSphere = _viewModel.CreatePanoramaSphere();
            var panoramaLayer = RenderManager.Instance.GetLayer<BackgroundLayer>();

            panoramaLayer?.SetPanorama(MainViewport, worldSphere);
        }

        // HUD layer
        private void SetupHud()
        {
            var hudLayer = RenderManager.Instance.GetLayer<HudLayer>();

            var crosshair = new CrosshairElement("Crosshair");
            hudLayer.RegisterElement(crosshair);
            hudLayer.ShowAll();
        }


        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _viewModel.IsDragging = true;
                _lastMousePosition = e.GetPosition(this);
                _lastMoveTime = DateTime.Now;
                _speedBuffer.Clear();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_viewModel.IsDragging) return;

            Point currentPosition = e.GetPosition(this);
            DateTime currentTime = DateTime.Now;

            double deltaX = currentPosition.X - _lastMousePosition.X;
            double deltaY = currentPosition.Y - _lastMousePosition.Y;
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            double timeDelta = (currentTime - _lastMoveTime).TotalMilliseconds;

            if (timeDelta > 0 && (Math.Abs(deltaX) > 0.1 || Math.Abs(deltaY) > 0.1))
            {
                double speed = distance / timeDelta;
                double adaptiveSensitivity = CalculateAdaptiveSensitivity(speed);

                // quite a shitty sensitivity handling; use FOV-based instead

                _currentRotationY -= deltaX * adaptiveSensitivity;
                _currentRotationX -= deltaY * adaptiveSensitivity;

                // (blocking camera from looking too low or too high)
                _currentRotationX = Math.Max(-Math.PI / 2 + Configuration.Instance.GameSettings.ClampVRotationMin,
                                            Math.Min(Math.PI / 2 - Configuration.Instance.GameSettings.ClampVRotationMax,
                                                    _currentRotationX));

                _viewModel.UpdateCameraRotation(MainCamera, _currentRotationX, _currentRotationY);
            }

            _lastMousePosition = currentPosition;
            _lastMoveTime = currentTime;
        }

        private double CalculateAdaptiveSensitivity(double speed)
        {
            // this sensitivity method will be changed 

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

            return Configuration.Instance.GameSettings.MouseSensitivity * (MainCamera.FieldOfView / 60.0) * sensitivityScale;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainCamera.FieldOfView -= e.Delta * Configuration.Instance.GameSettings.ZoomSpeed;
            MainCamera.FieldOfView = Math.Max(Configuration.Instance.GameSettings.MinFOV,
                                             Math.Min(Configuration.Instance.GameSettings.MaxFOV, MainCamera.FieldOfView));

            // we can also handle hud changes there

            var hudLayer = RenderManager.Instance.GetLayer<HudLayer>();
            if (MainCamera.FieldOfView < Configuration.Instance.GameSettings.MaxFOV)
                hudLayer?.HideElement("Crosshair");
            else
                hudLayer?.ShowElement("Crosshair");
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _viewModel.IsDragging = false;
                _speedBuffer.Clear();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            RenderManager.Instance.Render();
            _viewModel.UpdateCenterDistance();
        }

        //protected override void OnClosed(EventArgs e)
        //{
        //    CompositionTarget.Rendering -= OnRendering;
        //    _viewModel.Dispose();
        //    base.OnClosed(e);
        //}
    }
}