// EnemyManager.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace VLSShared.Models
{
    public static class EnemyManager
    {
        private static readonly List<Enemy> enemies = new();
        private static readonly object _lock = new();

        public static event Action<Enemy>? EnemyAdded;
        public static event Action<Guid>? EnemyRemoved;
        public static event Action<Guid, Vector3>? EnemyUpdated; // для анимации или перемещения
        public static event Action<Enemy, HitZone>? EnemyHit;

        public static IReadOnlyList<Enemy> Enemies => enemies.AsReadOnly();



        public static void RemoveEnemy(Guid id)
        {
            lock (_lock)
            {
                var enemy = enemies.FirstOrDefault(e => e.Id == id);
                if (enemy != null)
                {
                    enemies.Remove(enemy);
                    EnemyRemoved?.Invoke(id);
                }
            }
        }

        public static void UpdateEnemyPosition(Guid id, Vector3 newDirection, double newDistance)
        {
            lock (_lock)
            {
                var enemy = enemies.FirstOrDefault(e => e.Id == id);
                if (enemy != null)
                {
                    enemy.UpdatePosition(newDirection, newDistance);
                    EnemyUpdated?.Invoke(id, enemy.Direction);
                }
            }
        }

        /// <summary>
        /// Проверяет всех активных врагов на попадание луча (bulletDir).
        /// Возвращает первого поражённого врага (или null) и зону попадания.
        /// </summary>
        public static (Enemy? enemy, HitZone zone) CheckHit(Vector3 bulletDir)
        {
            lock (_lock)
            {
                Debug.WriteLine($"[EnemyManager] CheckHit: {enemies.Count(e => e.IsAlive)} alive enemies, bulletDir=({bulletDir.X:F2},{bulletDir.Y:F2},{bulletDir.Z:F2})");
                foreach (var enemy in enemies.Where(e => e.IsAlive))
                {
                    Debug.WriteLine($"[EnemyManager] Testing enemy {enemy.Id} at dist {enemy.Distance}");
                    var zone = enemy.CheckHit(bulletDir);
                    Debug.WriteLine($"[EnemyManager] enemy.CheckHit returned {zone}");
                    if (zone != HitZone.None)
                    {
                        EnemyHit?.Invoke(enemy, zone);
                        // Удаляем врага после попадания (можно оставить логику анимации смерти)
                        enemies.Remove(enemy);
                        EnemyRemoved?.Invoke(enemy.Id);
                        return (enemy, zone);
                    }
                }
            }
            return (null, HitZone.None);
        }

        public static void Clear()
        {
            lock (_lock)
            {
                foreach (var enemy in enemies)
                    EnemyRemoved?.Invoke(enemy.Id);
                enemies.Clear();
            }
        }

        public static Action<Enemy>? OnEnemyVisualCreate { get; set; }

        public static void AddEnemy(Enemy enemy)
        {
            lock (_lock)
            {
                enemies.Add(enemy);
                EnemyAdded?.Invoke(enemy);
                OnEnemyVisualCreate?.Invoke(enemy); // уведомляем UI‑слой
            }
        }
    }
}