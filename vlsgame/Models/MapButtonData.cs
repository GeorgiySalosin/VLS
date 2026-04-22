namespace VLSGame.Models
{
    // The same table will be in the database
    internal class MapButtonData
    {
        public int Id { get; init; }
        private const string checkmark = "/Content/checkmark.png";
        public string Checkmark => checkmark;
        public string Title { get; init; }
        public string Subtitle { get; init; }
        public string MapBackgroundImagePath { get; init; }
    }
}
