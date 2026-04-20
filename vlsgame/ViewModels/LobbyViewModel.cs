using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using VLSGame.Commands;
using VLSGame.Models;

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
        }
    }
}
