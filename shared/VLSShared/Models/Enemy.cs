// Enemy.cs
using System;
using System.Diagnostics;
using System.Numerics;

namespace VLSShared.Models
{
    public enum HitZone
    {
        None,
        Head,
        Body,
        Limb
    }

    public class Enemy
    {
        public Guid Id { get; }
        public Vector3 Direction { get; private set; }   // единичный вектор направления от камеры
        public double Distance { get; private set; }      // виртуальная дистанция (для расчётов)

        // Делегат для проверки попадания, реализуется в VLSGame
        private readonly Func<Vector3, (bool hit, HitZone zone)> checkHitFunc;

        public bool IsAlive { get; private set; } = true;

        public Enemy(Vector3 direction, double distance, Func<Vector3, (bool, HitZone)> hitChecker)
        {
            Id = Guid.NewGuid();
            Direction = Vector3.Normalize(direction);
            Distance = distance;
            checkHitFunc = hitChecker;
        }

        public HitZone CheckHit(Vector3 bulletDir)
        {
            if (!IsAlive) return HitZone.None;

            Debug.WriteLine($"[Enemy] CheckHit called for enemy {Id}, bulletDir=({bulletDir.X:F3},{bulletDir.Y:F3},{bulletDir.Z:F3})");
            var (hit, zone) = checkHitFunc(bulletDir);
            Debug.WriteLine($"[Enemy] checkHitFunc returned hit={hit}, zone={zone}");

            if (hit) IsAlive = false;
            return hit ? zone : HitZone.None;
        }
        /// <summary>
        /// Проверяет, попал ли луч (bulletDir) в этого врага.
        /// Возвращает зону попадания или None.
        /// </summary>

        public void UpdatePosition(Vector3 newDirection, double newDistance)
        {
            Direction = Vector3.Normalize(newDirection);
            Distance = newDistance;
        }
    }
}