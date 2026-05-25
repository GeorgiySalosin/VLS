namespace VLSGame.Models
{
    public class LoadingProgress
    {
        public int Percent { get; set; }          // 0-100
        public string? CurrentFile { get; set; }  // имя файла, который загружается
        public long BytesLoaded { get; set; }
        public long TotalBytes { get; set; }
    }
}
