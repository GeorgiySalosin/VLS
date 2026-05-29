namespace VLSGame.Models
{
    internal class LoadingProgress
    {
        internal readonly int Percent;
        internal readonly long BytesLoaded;
        internal readonly long TotalBytes;
        internal readonly string? CurrentFile;

        internal LoadingProgress(int percent, long bytesLoaded, long totalBytes, string? currentFile = null)
        {
            if (!(percent >= 0 && percent <= 100))
                throw new ArgumentOutOfRangeException("The percentage is specified incorrectly");
            if (bytesLoaded > totalBytes)
                throw new ArgumentOutOfRangeException("More bytes have been uploaded than there are");

            Percent = percent;
            BytesLoaded = bytesLoaded;
            TotalBytes = totalBytes;
            CurrentFile = currentFile;
        }
    }
}
