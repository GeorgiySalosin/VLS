namespace VLSGame.Models
{
    internal class MapButtonData
    {
        private const string checkmark = "/Content/checkmark.png";
        public string Checkmark => checkmark;
        public string Title { get; init; }
        public string Subtitle { get; init; }
        public string MapBackgroundImage { get; init; }
    }
}
