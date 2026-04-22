using System.Windows;
using VLSGame.Models;

namespace VLSGame.ViewModels
{
    // Wrapper for UI + implementation of INotifyPropertyChanged
    internal class MapButtonDataViewModel(MapButtonData data) : ViewModelBase
    {
        private readonly MapButtonData data = data;
        private Visibility checkmarkVisibility = Visibility.Visible;

        public string Title => data.Title;
        public string Subtitle => data.Subtitle;
        public string MapBackgroundImage => data.MapBackgroundImage;
        public string Checkmark => data.Checkmark;

        public Visibility CheckmarkVisibility
        {
            get => checkmarkVisibility;
            set => Set(ref checkmarkVisibility, value);
        }
    }
}
