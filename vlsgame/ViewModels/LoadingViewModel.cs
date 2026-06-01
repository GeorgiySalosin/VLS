using System.Windows;

namespace VLSGame.ViewModels
{
    internal sealed class LoadingViewModel : ViewModelBase
    {
        #region View's properties

        private Visibility visibility = Visibility.Collapsed;
        public Visibility Visibility
        {
            get => visibility;
            set => Set(ref visibility, value);
        }

        private int progressBarValue = 0;
        public int ProgressBarValue
        {
            get => progressBarValue;
            private set => Set(ref progressBarValue, value);
        }

        private string? loadingDescription;
        public string? LoadingDescription
        {
            get => loadingDescription;
            private set => Set(ref loadingDescription, "Loading: " + value);
        }

        #endregion

        internal void UpdateProgress(int percent, string? currentFile = null)
        {
            System.Diagnostics.Debug.WriteLine($"Progress: {percent}% - {currentFile}");
            ProgressBarValue = percent;
            LoadingDescription = currentFile;
        }
    }
}
