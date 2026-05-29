namespace VLSGame.ViewModels
{
    internal sealed class LoadingWindowViewModel : ViewModelBase
    {
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
        
        internal void UpdateProgress(int percent, string? currentFile = null)
        {
            System.Diagnostics.Debug.WriteLine($"Progress: {percent}% - {currentFile}");
            ProgressBarValue = percent;
            LoadingDescription = currentFile;
        }
    }
}
