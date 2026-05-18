using VLSShared.Interfaces;

namespace VLSShared.Data.Entities.Multiplayer
{
    public class MatchPanorama : IEntity
    {
        public int Id { get; set; }

        public int Panorama1Id { get; set; }
        public int Panorama2Id { get; set; }

        public float Enemy1X { get; set; }
        public float Enemy1Y { get; set; }
        public float Enemy1Z { get; set; }

        public float Enemy2X { get; set; }
        public float Enemy2Y { get; set; }
        public float Enemy2Z { get; set; }


        public virtual MultiPanorama Panorama1 { get; set; }
        public virtual MultiPanorama Panorama2 { get; set; }
    }
}
