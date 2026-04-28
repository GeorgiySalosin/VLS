using System.Windows;
using System.Windows.Media;
using VLSGame.Input;
using VLSGame.Rendering;
using VLSGame.Rendering.Content2D.HUD;
using VLSGame.ViewModels;

namespace VLSGame.Views
{
    public partial class Match : Window
    {
        private readonly MatchViewModel viewModel;
        private readonly MatchInput inputHandler = MatchInput.Instance;
        
        public Match(MatchViewModel viewModel)
        {
            InitializeComponent();

            this.viewModel = viewModel;
            DataContext = viewModel;

            viewModel.Viewport = MainViewport;
            viewModel.Hud = HudPanel;

            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            inputHandler.Initialize(viewModel, this);
            viewModel.OnViewLoaded();                       //   Since we gave ViewModel these refs for our viewport & panel,  now we notify it can work with em 
        }


        private void OnClosed(object? sender, EventArgs e)
        {
            inputHandler.UnsubscribeEvents();
            //viewModel.Dispose();
        }
    }
}