// Enemy.cs
using System;
using System.Diagnostics;
using System.Numerics;
using VLSShared.Enums;

namespace VLSShared.Models
{


    public class Enemy
    {
        public Guid Id { get; }
        public Vector3 Direction { get; init; }   // The direction determines where the enemy is spawned. Enemy location is static.
        public double Distance { get; init; }      // The distance determines how far enemy is supposed to be located (will be used to re-scale the character). Enemy distance is static.

        public double RenderDistance { get; init; } = 0.1; // реальное расстояние плоскости от камеры

        public double Scale { get; init; } = 0.01;
        public Func<float, float, HitZone>? HitZoneChecker { get; set; }

        public Enemy(Vector3 direction, double distance)
        {
            Id = Guid.NewGuid();
            Direction = Vector3.Normalize(direction);
            Distance = distance;
        }
    }
}