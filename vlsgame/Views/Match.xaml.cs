using System.Windows;
using System.Windows.Media;
using VLSGame.Config;
using VLSGame.Input;
using VLSGame.Rendering;
using VLSGame.Rendering.Content2D.HUD;
using VLSGame.Rendering.Content2D.Projectile;
using VLSGame.ViewModels;

namespace VLSGame.Views
{
    public partial class Match : Window
    {
        private readonly MatchViewModel viewModel;
        private readonly MatchInput _inputHandler = MatchInput.Instance;

        public Match(MatchViewModel viewModel)
        {
            InitializeComponent();

            #region Import config 

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
            #endregion

            this.viewModel = viewModel;
            DataContext = viewModel;

            RenderManager.Instance.Initialize(MainViewport, HudPanel);

            MatchInput.Instance.Initialize(viewModel, this);

            CreateEnvironment();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            Loaded += OnLoaded;
            CompositionTarget.Rendering += OnRendering;
            Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupLayers();
            var projectileLayer = RenderManager.Instance.GetLayer<ProjectileLayer>();
        }


        private void CreateEnvironment()
        {
            var worldSphere = viewModel.CreatePanoramaSphere();
            RenderManager.Instance?.SetBackground(worldSphere);
        }


        private void SetupLayers()
        {
            // HUD 
            var hudLayer = RenderManager.Instance.GetLayer<HudLayer>();
            hudLayer?.Initialize(viewModel);
            var crosshair = new CrosshairTexture();
            hudLayer?.RegisterTexture(crosshair);
            hudLayer?.ShowTexture("Crosshair");


        }

        // this 'd better moved into viewmodel.
        private void OnRendering(object? sender, EventArgs e)
        {
            RenderManager.Instance.Render();

            viewModel.GetCenterDistance();
        }


        private void OnClosed(object? sender, EventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering;
            _inputHandler.UnsubscribeEvents();
            viewModel.Dispose();
        }
    }
}