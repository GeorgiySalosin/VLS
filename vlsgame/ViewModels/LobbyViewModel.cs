using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using VLSGame.Commands;
using VLSGame.Models;
using VLSGame.Services;
using VLSGame.Views;
using VLSShared.Interfaces;

namespace VLSGame.ViewModels
{
    internal class LobbyViewModel : ViewModelBase
    {
        public ObservableCollection<MapButtonDataViewModel> MapViewModels { get; init; }

        internal event EventHandler? CloseRequested; // event for View closing

        private Visibility visibilityGridMode = Visibility.Hidden;
        public Visibility VisibilityGridMode {
            get => visibilityGridMode;
            private set => Set(ref visibilityGridMode, value);
        }

        #region SinglePlayer

        private IGameMode? currentGameMode;
        public IGameMode? CurrentGameMode => currentGameMode;
        private async Task StartSinglePlayerAsync(string panoramaPath)
        {
            currentGameMode = new SinglePlayerGameMode();
            await currentGameMode.StartAsync();

            if (currentGameMode is SinglePlayerGameMode singlePlayer)
            {
                singlePlayer.SetPanoramaPath(panoramaPath);
            }
        }

        #endregion

        #region Commands

        // Change visibility of element
        private Visibility ToggleVisibility(Visibility visibility) =>
            visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

        #region ToggleModeGridCommand

        public ICommand ToggleModeGridCommand { get; }
        private bool CanToggleModeGridCommandExecute(object p) => true;
        private void OnToggleModeGridCommandExecuted(object p) =>
            VisibilityGridMode = ToggleVisibility(VisibilityGridMode);

        #endregion

        #region StartGame

        public ICommand StartGameCommand { get; }
        private bool CanStartGameCommandExecute(object p) => true; // todo
        private async void OnStartGameCommandExecuted(object p)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // currently we only use the test panorama

            string ColorMapPath = System.IO.Path.Combine(basePath, @"Content\Maps\Test_W.png");
            string DepthMapPath = System.IO.Path.Combine(basePath, @"Content\Maps\Test_D.png");

            if (!System.IO.File.Exists(ColorMapPath))
            {
                MessageBox.Show($"Color map was not found from {ColorMapPath}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.IO.File.Exists(DepthMapPath))
            {
                MessageBox.Show($"Depth map was not found from {DepthMapPath}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await StartSinglePlayerAsync(ColorMapPath);

            // Открываем окно Match с обоими путями
            var matchViewModel = new MatchViewModel(CurrentGameMode!, ColorMapPath, DepthMapPath);
            var matchWindow = new Match(matchViewModel);
            matchWindow.Show();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region ToggleMap

        public ICommand ToggleMapCommand { get; }
        private bool CanToggleMapCommandExecute(object p) => true;
        private void OnToggleMapCommandExecuted(object p)
        {
            if (p is MapButtonDataViewModel vm)
            {
                vm.CheckmarkVisibility = ToggleVisibility(vm.CheckmarkVisibility);
            }
        }

        #endregion

        #endregion

        internal LobbyViewModel()
        {
            // Initialize map buttons
            var models = new[]
            {
                new MapButtonData
                {
                    Title = "Sunny",
                    Subtitle = "common map",
                    MapBackgroundImage = "/Content/Lobby/T_MapPreview_Sun.png"
                },
                new MapButtonData
                {
                    Title = "Overcast",
                    Subtitle = "for nature lovers",
                    MapBackgroundImage = "/Content/Lobby/T_MapPreview_Fog.png"
                },
                new MapButtonData
                {
                    Title = "Dark",
                    Subtitle = "prove your skill!",
                    MapBackgroundImage = "/Content/Lobby/T_MapPreview_Sunset.png"
                }
            };
            MapViewModels = new ObservableCollection<MapButtonDataViewModel>(
                models.Select(m => new MapButtonDataViewModel(m))
            );

            #region Initialize commands
            ToggleModeGridCommand = new RelayCommand(OnToggleModeGridCommandExecuted, CanToggleModeGridCommandExecute);
            StartGameCommand = new RelayCommand(OnStartGameCommandExecuted, CanStartGameCommandExecute);
            ToggleMapCommand = new RelayCommand(OnToggleMapCommandExecuted, CanToggleMapCommandExecute);
            #endregion
        }
    }
}
