using VLSShared.Interfaces;

namespace VLSShared.Data.Entities
{
    public class MatchPanoramas : IEntity
    {
        public int Id { get; set; }

        public int Panorama1Fk { get; set; }
        public int Panorama2Fk { get; set; }

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
