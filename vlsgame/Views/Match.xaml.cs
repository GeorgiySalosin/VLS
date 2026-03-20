using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using VLSGame.ViewModels;

namespace VLSGame
{
    public partial class Match : Window
    {
        private readonly MatchViewModel _viewModel;
        private Point lastMousePosition;
        private DateTime _lastMoveTime;

        private double _rotationY;
        private double _rotationX;

        private const float MOUSE_SENSITIVITY = 0.005f;

        // Mouse adapting sensitivity, test
        private Queue<double> _speedBuffer = new Queue<double>();
        private const int SPEED_BUFFER_SIZE = 5;
        private const double MIN_SPEED_THRESHOLD = 2.0; // minimal sensitivity is applied if the mouse move speed <= THIS,
        private const double MAX_SPEED_THRESHOLD = 20.0; // maximal sensitivity is applied if the mouse move speed >= THIS,
        private const double MIN_SENSITIVITY_SCALE = 0.1; // sensibility for micro moves

        public Match(MatchViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;

            Loaded += OnLoaded;


            this.MouseDown += Match_MouseDown;
            this.MouseMove += Match_MouseMove;
            this.MouseUp += Match_MouseUp;
            this.MouseWheel += Match_MouseWheel;


            // timer to update color detection in the center of screen
            CompositionTarget.Rendering += UpdateCenterColor;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadPanorama();

            if (_viewModel.PanoramaImage != null)
            {
                CreatePanoramaSphere(_viewModel.PanoramaImage);
            }

            _rotationX = _viewModel.RotationX;
            _rotationY = _viewModel.RotationY;
            UpdateCameraDirection();
        }

        /* this is a test version to make sure everything works*/
        private void CreatePanoramaSphere(BitmapSource panoramaImage)
        {
            // rmv old sphere mesh
            MainViewport.Children.Clear();

            
            var light = new ModelVisual3D();
            light.Content = new AmbientLight(Colors.White);
            MainViewport.Children.Add(light);

            MeshGeometry3D mesh = new MeshGeometry3D();

            const int phiSegments = 128;
            const int thetaSegments = 256;


            //manual sphere craetion
            for (int i = 0; i <= phiSegments; i++)
            {
                double phi = Math.PI * i / phiSegments;

                for (int j = 0; j <= thetaSegments; j++)
                {
                    double theta = 2 * Math.PI * j / thetaSegments;

                    double x = Math.Sin(phi) * Math.Cos(theta);
                    double y = Math.Cos(phi);
                    double z = Math.Sin(phi) * Math.Sin(theta);

                    mesh.Positions.Add(new Point3D(x, y, z));

                    double u = theta / (2 * Math.PI);
                    double v = phi / Math.PI;

                    mesh.TextureCoordinates.Add(new Point(u, v));
                }
            }

            for (int i = 0; i < phiSegments; i++)
            {
                for (int j = 0; j < thetaSegments; j++)
                {
                    int p0 = i * (thetaSegments + 1) + j;
                    int p1 = i * (thetaSegments + 1) + j + 1;
                    int p2 = (i + 1) * (thetaSegments + 1) + j;
                    int p3 = (i + 1) * (thetaSegments + 1) + j + 1;

                    mesh.TriangleIndices.Add(p0);
                    mesh.TriangleIndices.Add(p2);
                    mesh.TriangleIndices.Add(p1);

                    mesh.TriangleIndices.Add(p1);
                    mesh.TriangleIndices.Add(p2);
                    mesh.TriangleIndices.Add(p3);
                }
            }

            
            ImageBrush brush = new ImageBrush(panoramaImage)
            {
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.None,
                Stretch = Stretch.Fill
            };

            DiffuseMaterial material = new DiffuseMaterial(brush);

            GeometryModel3D geometryModel = new GeometryModel3D(mesh, material);

            // transform the sphere (set the scale)
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new ScaleTransform3D(100, 100, 100));
            geometryModel.Transform = transformGroup;

            var sphereVisual = new ModelVisual3D();
            sphereVisual.Content = geometryModel;
            MainViewport.Children.Add(sphereVisual);
        }

        private void UpdateCameraDirection()
        {
            double x = Math.Cos(_rotationX) * Math.Sin(_rotationY);
            double y = Math.Sin(_rotationX);
            double z = Math.Cos(_rotationX) * Math.Cos(_rotationY);

            Vector3D direction = new Vector3D(x, y, z);
            direction.Normalize();


            MainCamera.LookDirection = direction;
            _viewModel.RotationX = _rotationX;
            _viewModel.RotationY = _rotationY;
        }



        private void UpdateCenterColor(object? sender, EventArgs e)
        {
            _viewModel.UpdateCenterColor();
        }

        private void Match_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _viewModel.IsDragging = true;
                lastMousePosition = e.GetPosition(this);
                _lastMoveTime = DateTime.Now;
                _speedBuffer.Clear();
            }
        }

        private void Match_MouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel.IsDragging)
            {
                Point currentPosition = e.GetPosition(this);
                DateTime currentTime = DateTime.Now;

                // calculate the mouse speed
                double deltaX = currentPosition.X - lastMousePosition.X;
                double deltaY = currentPosition.Y - lastMousePosition.Y;
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                double timeDelta = (currentTime - _lastMoveTime).TotalMilliseconds;

                if (timeDelta > 0 && (Math.Abs(deltaX) > 0.1 || Math.Abs(deltaY) > 0.1))
                {
                    double speed = distance / timeDelta; // mouse speed v = pixels/ms

                    
                    _speedBuffer.Enqueue(speed);
                    if (_speedBuffer.Count > SPEED_BUFFER_SIZE)
                    {
                        _speedBuffer.Dequeue();
                    }

                    double smoothedSpeed = _speedBuffer.Average();

                    // define sensibility by a speed

                    double sensitivityScale;

                    if (smoothedSpeed <= MIN_SPEED_THRESHOLD)
                    {
                        
                        sensitivityScale = MIN_SENSITIVITY_SCALE;
                    }
                    else if (smoothedSpeed >= MAX_SPEED_THRESHOLD)
                    {
                        
                        sensitivityScale = 1.0;
                    }
                    else
                    {
                        
                        // square interpolation for smooth sens transition
                        double t = (smoothedSpeed - MIN_SPEED_THRESHOLD) /
                                  (MAX_SPEED_THRESHOLD - MIN_SPEED_THRESHOLD);
                        sensitivityScale = MIN_SENSITIVITY_SCALE +
                                         (1.0 - MIN_SENSITIVITY_SCALE) * (1 - Math.Pow(1 - t, 2));
                    }

                    
                    double adaptiveSensitivity = MOUSE_SENSITIVITY *
                                                (MainCamera.FieldOfView / 60.0) *
                                                sensitivityScale;


                    deltaX = Math.Max(-20, Math.Min(20, deltaX));
                    deltaY = Math.Max(-20, Math.Min(20, deltaY));

                    _rotationY -= deltaX * adaptiveSensitivity;
                    _rotationX -= deltaY * adaptiveSensitivity;

                    _rotationX = Math.Max(-Math.PI / 2 + 0.01,
                                         Math.Min(Math.PI / 2 - 0.01, _rotationX));

                    UpdateCameraDirection();
                }

                lastMousePosition = currentPosition;
                _lastMoveTime = currentTime;
            }
        }

        private void Match_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _viewModel.IsDragging = false;
                _speedBuffer.Clear();
            }
        }

        private void Match_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomSpeed = 0.1;
            MainCamera.FieldOfView -= e.Delta * zoomSpeed;
            MainCamera.FieldOfView = Math.Max(6, Math.Min(90, MainCamera.FieldOfView)); // Remember fov 11.25 (8x scope), 10 (9x scope), 6(15x scope)
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            CompositionTarget.Rendering -= UpdateCenterColor;
            base.OnClosed(e);
        }
    }
}