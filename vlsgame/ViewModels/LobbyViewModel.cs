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
        public ObservableCollection<MapButtonData> Maps { get; set; }

        private Visibility visibilityGridMode = Visibility.Hidden;
        public Visibility VisibilityGridMode {
            get => visibilityGridMode;
            private set => Set(ref visibilityGridMode, value);
        }

        #region Commands

        #region ChangeVisibilityModeGrid

        public ICommand ToggleModeGridCommand { get; }
        private bool CanToggleModeGridCommandExecute(object p) => true;
        private void OnToggleModeGridCommandExecuted(object p)
        {
            if (VisibilityGridMode == Visibility.Hidden) VisibilityGridMode = Visibility.Visible;
            else VisibilityGridMode = Visibility.Hidden;
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

        #endregion

        #region StartGame

        public ICommand StartGame { get; }
        private bool CanStartGameExecute(object p) => true; // todo
        private async void OnStartGameExecuted(object p)
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
            //this.Close(); todo
        }

        #endregion

        #endregion

        internal LobbyViewModel()
        {
            Maps = new ObservableCollection<MapButtonData>
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

            ToggleModeGridCommand = new RelayCommand(OnToggleModeGridCommandExecuted, CanToggleModeGridCommandExecute);
            StartGame = new RelayCommand(OnStartGameExecuted, CanStartGameExecute);
        }
    }
}
