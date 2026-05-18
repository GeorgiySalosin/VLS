using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using VLSGame.Commands;
using VLSGame.Config;
using VLSGame.Services;
using VLSGame.Views;
using VLSShared.Data;
using VLSShared.Interfaces;

namespace VLSGame.ViewModels
{
    internal class LobbyViewModel : ViewModelBase
    {
        public ObservableCollection<MapButtonDataViewModel> MapViewModels { get; init; }

        internal event EventHandler? CloseRequested; // event for View closing

        #region View's properties

        private Visibility visibilityGridMode = Visibility.Hidden;
        public Visibility VisibilityGridMode {
            get => visibilityGridMode;
            private set => Set(ref visibilityGridMode, value);
        }

        private string mapText;
        public string MapText
        {
            get => mapText;
            private set => Set(ref mapText, value);
        }

        private string displayGamemode;
        public string DisplayGamemode
        {
            get => displayGamemode;
            private set => Set(ref displayGamemode, value);
        }

        private string selectModeImagePath;
        public string SelectModeImagePath
        {
            get => selectModeImagePath;
            private set => Set(ref selectModeImagePath, value);
        }
        private const string combinedSelectModeImagePath = "/Content/Lobby/T_MapPreview_Combined.png";

        #endregion

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
            FullReloadFromConfig();
            VisibilityGridMode = Visibility.Visible;
        }

        #endregion

        #region StartGame

        public ICommand StartGameCommand { get; }
        private bool CanStartGameCommandExecute(object p) => VisibilityGridMode != Visibility.Visible;
        private async void OnStartGameCommandExecuted(object p)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // We need to know what weather the user has selected
            int mapIndex = ChoiceRandomWeather();

            string ColorMapPath;
            string DepthMapPath = System.IO.Path.Combine(basePath, @"Content\Maps\Depth\W001.png");

            switch (mapIndex) {
                case 1:
                    ColorMapPath = System.IO.Path.Combine(basePath, @"Content\Maps\Sunny\W001.png");
                    break;
                case 2:
                    ColorMapPath = System.IO.Path.Combine(basePath, @"Content\Maps\Foggy\W001.png");
                    break;
                case 3:
                    ColorMapPath = System.IO.Path.Combine(basePath, @"Content\Maps\Sunset\W001.png");
                    break;
                default:
                    throw new Exception("A non-existent weather was selected");
            }

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

            await StartSinglePlayerAsync(ColorMapPath); // So far, we are launching strictly a singleplayer.

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
            FullSaveToConfig();
            VisibilityGridMode = Visibility.Hidden;
            FullReloadFromConfig(); // Update the UI
        }

        #endregion

        #region ExitChoiceMaps

        public ICommand ExitChoiceMapsCommand { get; }
        private bool CanExitChoiceMapsCommandExecute(object p) => VisibilityGridMode != Visibility.Hidden;
        private void OnExitChoiceMapsCommandExecuted(object p) => VisibilityGridMode = Visibility.Hidden;

        #endregion

        #region ActivateSingleplayer

        public ICommand ActivateSingleplayerCommand { get; }
        private bool CanActivateSingleplayerCommandExecute(object p) => true;
        private void OnActivateSingleplayerCommandExecuted(object p)
        {
            currentGameMode = new SinglePlayerGameMode(); // We update the state and update the UI when the settings are saved
        }

        #endregion

        #region ActivateMultiplayer

        public ICommand ActivateMultiplayerCommand { get; }
        private bool CanActivateMultiplayerCommandExecute(object p) => true;
        private void OnActivateMultiplayerCommandExecuted(object p)
        {
            currentGameMode = new MultiPlayerGameMode(); // We update the state and update the UI when the settings are saved
        }

        #endregion

        #endregion

        internal LobbyViewModel()
        {
            DatabaseInitializer.Initialize(); // Initialize a database

            // Downloading weathers from DB
            var weathers = DatabaseService.GetAllWeathers();
            MapViewModels = new ObservableCollection<MapButtonDataViewModel>(
                weathers.Select(w => new MapButtonDataViewModel
                {
                    Id = w.Id,
                    Title = w.Title,
                    Subtitle = w.Description,
                    MapBackgroundImage = w.PreviewPath
                }
                )
            );

            #region Import config

            if (!Configuration.Instance.LoadConfiguration())
            {
                MessageBox.Show("File error: GameSettings.json", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            #endregion

            FullReloadFromConfig();

            #region Initialize commands
            ToggleModeGridCommand = new RelayCommand(OnToggleModeGridCommandExecuted, CanToggleModeGridCommandExecute);
            StartGameCommand = new RelayCommand(OnStartGameCommandExecuted, CanStartGameCommandExecute);
            ToggleMapCommand = new RelayCommand(OnToggleMapCommandExecuted, CanToggleMapCommandExecute);
            SaveMapsCommand = new RelayCommand(OnSaveMapsCommandExecuted, CanSaveMapsCommandExecute);
            ExitChoiceMapsCommand = new RelayCommand(OnExitChoiceMapsCommandExecuted, CanExitChoiceMapsCommandExecute);
            ActivateSingleplayerCommand = new RelayCommand(OnActivateSingleplayerCommandExecuted, CanActivateSingleplayerCommandExecute);
            ActivateMultiplayerCommand = new RelayCommand(OnActivateMultiplayerCommandExecuted, CanActivateMultiplayerCommandExecute);
            #endregion
        }

        private void UpdateMapText()
        {
            var selectedMaps = MapViewModels.Where(vm => vm.CheckmarkVisibility == Visibility.Visible).ToList();
            int selectedCount = selectedMaps.Count;
            if (selectedCount == 1)
                MapText = $"Map: {selectedMaps[0].Title}";
            else
                MapText = $"Map: Random({selectedCount})";
        }

        private void ToggleGamemode(IGameMode gameMode)
        {
            currentGameMode = gameMode;
            DisplayGamemode = gameMode is SinglePlayerGameMode ? "Singleplayer" : "Multiplayer";
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
            UpdateMapText();

            // Defining the SelectModeImagePath
            var selectedMaps = MapViewModels
                .Where(vm => vm.CheckmarkVisibility == Visibility.Visible)
                .ToList();
            if (selectedMaps.Count == 1)
                SelectModeImagePath = selectedMaps[0].MapBackgroundImage;
            else
                SelectModeImagePath = combinedSelectModeImagePath;

        }

        private void ReloadSelectedGamemodeFromConfig()
        {
            // Restoring the selected gamemode from the configuration
            var settings = Configuration.Instance.GameSettings;
            if (settings != null && settings.SelectedGameMode != null)
            {
                ToggleGamemode(settings.SelectedGameMode);
            }
            else
            {
                IGameMode defaultMode = new GameSettings().SelectedGameMode;
                ToggleGamemode(defaultMode);
                SaveSelectedGamemodeToConfig();
            }
        }

        private void FullReloadFromConfig()
        {
            ReloadSelectedMapsFromConfig();
            ReloadSelectedGamemodeFromConfig();
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

        private void SaveSelectedGamemodeToConfig()
        {
            var config = Configuration.Instance;
            if (config.GameSettings == null) return;

            // Compare
            if (config.GameSettings.SelectedGameMode != null &&
                config.GameSettings.SelectedGameMode == currentGameMode)
                return; // nothing changed

            config.GameSettings.SelectedGameMode = currentGameMode;
            config.SaveConfiguration();
        }

        private void FullSaveToConfig()
        {
            SaveSelectedMapsToConfig();
            SaveSelectedGamemodeToConfig();
        }

        private int ChoiceRandomWeather()
        {
            var settings = Configuration.Instance.GameSettings;
            if (settings != null && settings.SelectedMapIds != null && settings.SelectedMapIds.Any())
            {
                Random rnd = new Random();
                int index = rnd.Next(settings.SelectedMapIds.Count);
                int mapIndex = settings.SelectedMapIds[index];
                return mapIndex;
            }
            else // It will need to be tested
            {
                var allIds = MapViewModels.Select(vm => vm.Id).ToList();

                var config = Configuration.Instance;
                if (config.GameSettings == null) throw new Exception("config.GameSettings is null");

                config.GameSettings.SelectedMapIds = allIds;
                config.SaveConfiguration();

                return ChoiceRandomWeather();
            }
        }

        #endregion
    }
}
