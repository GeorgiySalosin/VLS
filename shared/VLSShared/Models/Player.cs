// Enemy.cs
using System.Numerics;
using VLSShared.Enums;

namespace VLSShared.Models
{
    public class Player(Vector3 direction)
    {
        public Guid Id { get; } = Guid.NewGuid();

        internal int Hp { get; private set; } = 100;

        public Vector3 Direction { get; init; } = Vector3.Normalize(direction);
        public double Distance { get; set; } 

        public double Scale { get; set; }
        public Func<float, float, HitZoneInfo>? HitZoneChecker { get; set; }

        internal void ApplyDamage(HitZoneInfo hitZone)
        {
            Hp -= hitZone.BaseDamage;
        }
    }
}