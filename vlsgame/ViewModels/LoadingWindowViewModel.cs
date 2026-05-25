namespace VLSGame.ViewModels
{
    internal class LoadingWindowViewModel : ViewModelBase
    {
        private int progressBarValue = 1;
        public int ProgressBarValue
        {
            get => progressBarValue;
            private set => Set(ref progressBarValue, value);
        }
        internal void UpdateProgress(int percent, string currentFile)
        {
            ProgressBarValue = percent;
        }
    }
}
