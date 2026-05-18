using VLSShared.Interfaces;

namespace VLSShared.Data.Entities
{
    public abstract class Panorama : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
