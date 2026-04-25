using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using VLSGame.Commands;
using VLSGame.Config;
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

        #region ToggleModeGrid

        public ICommand ToggleModeGridCommand { get; }
        private bool CanToggleModeGridCommandExecute(object p) => VisibilityGridMode != Visibility.Visible;
        private void OnToggleModeGridCommandExecuted(object p)
        {
            ReloadSelectedMapsFromConfig();
            VisibilityGridMode = Visibility.Visible;
        }

        #endregion

        #region StartGame

        public ICommand StartGameCommand { get; }
        private bool CanStartGameCommandExecute(object p) => VisibilityGridMode != Visibility.Visible;
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

        #region SaveMaps

        public ICommand SaveMapsCommand { get; }
        private bool CanSaveMapsCommandExecute(object p) =>
            // ModeGrid is opened && at least one map has been selected
            VisibilityGridMode != Visibility.Hidden && MapViewModels.Any(vm => vm.CheckmarkVisibility == Visibility.Visible);
        private void OnSaveMapsCommandExecuted(object p)
        {
            SaveSelectedMapsToConfig();
            VisibilityGridMode = Visibility.Hidden;
        }

        #endregion

        #region ExitChoiceMaps

        public ICommand ExitChoiceMapsCommand { get; }
        private bool CanExitChoiceMapsCommandExecute(object p) => VisibilityGridMode != Visibility.Hidden;
        private void OnExitChoiceMapsCommandExecuted(object p) => VisibilityGridMode = Visibility.Hidden;

        #endregion

        #endregion

        internal LobbyViewModel()
        {
            #region Initialize models

            var models = new[]
            {
                new MapButtonData { Id = 1, Title = "Sunny", Subtitle = "common map", MapBackgroundImagePath = "/Content/Lobby/T_MapPreview_Sun.png" },
                new MapButtonData { Id = 2, Title = "Overcast", Subtitle = "for nature lovers", MapBackgroundImagePath = "/Content/Lobby/T_MapPreview_Fog.png" },
                new MapButtonData { Id = 3, Title = "Dark", Subtitle = "prove your skill!", MapBackgroundImagePath = "/Content/Lobby/T_MapPreview_Sunset.png" }
            };

            MapViewModels = new ObservableCollection<MapButtonDataViewModel>(
                models.Select(m => new MapButtonDataViewModel(m))
            );

            #endregion

            #region Import config

            if (!Configuration.Instance.LoadConfiguration())
            {
                MessageBox.Show("File error: GameSettings.json", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            #endregion

            #region Initialize commands
            ToggleModeGridCommand = new RelayCommand(OnToggleModeGridCommandExecuted, CanToggleModeGridCommandExecute);
            StartGameCommand = new RelayCommand(OnStartGameCommandExecuted, CanStartGameCommandExecute);
            ToggleMapCommand = new RelayCommand(OnToggleMapCommandExecuted, CanToggleMapCommandExecute);
            SaveMapsCommand = new RelayCommand(OnSaveMapsCommandExecuted, CanSaveMapsCommandExecute);
            ExitChoiceMapsCommand = new RelayCommand(OnExitChoiceMapsCommandExecuted, CanExitChoiceMapsCommandExecute);
            #endregion
        }

        #region Config funcs
        private void ReloadSelectedMapsFromConfig()
        {
            // Restoring the selected maps from the configuration
            var settings = Configuration.Instance.GameSettings;
            if (settings != null && settings.SelectedMapIds != null && settings.SelectedMapIds.Any())
            {
                // Only those whose Id is in the list are selected
                foreach (var vm in MapViewModels)
                    vm.CheckmarkVisibility = settings.SelectedMapIds.Contains(vm.Id) ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                // Fallback: select all maps
                foreach (var vm in MapViewModels)
                    vm.CheckmarkVisibility = Visibility.Visible;
                // Save this state back to config
                SaveSelectedMapsToConfig();
            }
        }

        private void SaveSelectedMapsToConfig()
        {
            var selectedIds = MapViewModels
                .Where(vm => vm.CheckmarkVisibility == Visibility.Visible)
                .Select(vm => vm.Id)
                .ToList();

            var config = Configuration.Instance;
            if (config.GameSettings == null) return;

            // Compare sequences
            if (config.GameSettings.SelectedMapIds != null &&
                config.GameSettings.SelectedMapIds.SequenceEqual(selectedIds))
                return; // nothing changed

            config.GameSettings.SelectedMapIds = selectedIds;
            config.SaveConfiguration();
        }
        #endregion
    }
}
