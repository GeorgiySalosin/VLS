using VLSShared.Interfaces;

namespace VLSShared.Data.Entities.Singleplayer
{
    public class SingleTargets : IEntity
    {
        public int Id { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public int SinglePanoramaId { get; set; }

        public virtual SinglePanorama SinglePanorama { get; set; }
    }
}
