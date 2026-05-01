// Enemy.cs
using System.Numerics;
using VLSShared.Enums;

namespace VLSShared.Models
{
    public class Player
    {
        public Guid Id { get; }

        internal int Hp { get; private set; } = 100;
        private static readonly Dictionary<HitZone, int> BaseDamage = new()
        {
            [HitZone.Head] = 100,
            [HitZone.Body] = 50,
            [HitZone.Limb] = 25,
            [HitZone.None] = 0
        };

        public Vector3 Direction { get; init; }   // The direction determines where the enemy is spawned. Static.
        public double Distance { get; init; }      // The distance determines how far enemy is supposed to be located (will be used to re-scale the character). Enemy distance is static.

        public double ViewportDistance { get; init; } // the distance from coordinates center to the 3d object (plane) center

        public double Scale { get; init; } = 0.01;
        public Func<float, float, HitZone>? HitZoneChecker { get; set; }

        public Player(Vector3 direction, double distance)
        {
            Id = Guid.NewGuid();
            Direction = Vector3.Normalize(direction);
            Distance = distance;
        }

        internal void ApplyDamage(HitZone hitZone) => 
            Hp -= BaseDamage[hitZone];
    }
}