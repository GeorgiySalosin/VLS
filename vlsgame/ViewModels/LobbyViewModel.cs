using System.Collections.ObjectModel;
using System.Windows.Input;
using VLSGame.Models;

namespace VLSGame.ViewModels
{
    internal class LobbyViewModel : ViewModelBase
    {
        public ObservableCollection<MapButtonData> Maps { get; set; }
        public ICommand SelectMapCommand { get; }

        internal LobbyViewModel()
        {
            Maps = new ObservableCollection<MapButtonData>
            {
                new MapButtonData
                {
                    Title = "Sunny",
                    Subtitle = "common map",
                    MapBackgroundImage = "/Content/Maps/Test_W.png"
                },
                new MapButtonData
                {
                    Title = "Overcast",
                    Subtitle = "for nature lovers",
                    MapBackgroundImage = "/Content/Maps/Test_W.png"
                },
                new MapButtonData
                {
                    Title = "Dark",
                    Subtitle = "prove your skill!",
                    MapBackgroundImage = "/Content/Maps/Test_W.png"
                }
            };
            SelectMapCommand = new RelayCommand<MapButtonData>(OnSelectMap);
        }
        private void OnSelectMap(MapButtonData map)
        {
            // логика выбора карты
        }
    }
}
