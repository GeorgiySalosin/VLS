using System.Windows;

namespace VLSGame.ViewModels
{
    // Wrapper for UI + implementation of INotifyPropertyChanged
    internal class MapButtonDataViewModel : ViewModelBase
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string Subtitle { get; init; }
        public string MapBackgroundImage { get; init; }

        private const string checkmark = "/Content/checkmark.png";
        public string Checkmark => checkmark;

        private Visibility checkmarkVisibility = Visibility.Visible;
        public Visibility CheckmarkVisibility
        {
            get => checkmarkVisibility;
            set => Set(ref checkmarkVisibility, value);
        }
    }
}
