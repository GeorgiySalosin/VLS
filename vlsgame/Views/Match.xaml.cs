using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VLSGame.Config;
using VLSGame.Input;
using VLSGame.Rendering;
using VLSGame.Rendering.HUD;
using VLSGame.Rendering.Layers;
using VLSGame.ViewModels;

namespace VLSGame.Views
{
    public partial class Match : Window
    {
        private readonly MatchViewModel _viewModel;
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

            _viewModel = viewModel;
            DataContext = _viewModel;

            RenderManager.Instance.Initialize(MainViewport, HudPanel);

            MatchInput.Instance.Initialize(_viewModel, this);


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
            SetupHud();
        }

        private void CreateEnvironment()
        {
            var worldSphere = _viewModel.CreatePanoramaSphere();
            var panoramaLayer = RenderManager.Instance.GetLayer<BackgroundLayer>();
            panoramaLayer?.SetPanorama(MainViewport, worldSphere);
        }

        private void SetupHud()
        {
            var hudLayer = RenderManager.Instance.GetLayer<HudLayer>();
            var crosshair = new CrosshairElement("Crosshair");
            hudLayer.RegisterElement(crosshair);
            hudLayer.ShowAll();
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            RenderManager.Instance.Render();
            _viewModel.UpdateCenterDistance();
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering;
            _inputHandler.UnsubscribeEvents();
            _viewModel.Dispose();
        }
    }
}