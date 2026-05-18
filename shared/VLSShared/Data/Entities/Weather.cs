using VLSShared.Interfaces;

namespace VLSShared.Data.Entities
{
    public class Weather : IEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PreviewPath { get; set; }
    }
}
