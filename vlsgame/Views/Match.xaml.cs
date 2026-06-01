using System.Windows;
using VLSGame.Input;
using VLSGame.Rendering;
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

            Loaded += (s, e) =>
            {
                inputHandler.Initialize(viewModel, this);
                viewModel.StartGameLoop();
            };

            Closed += OnClosed;
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            inputHandler.UnsubscribeEvents();
            //viewModel.Dispose();
        }
    }
}