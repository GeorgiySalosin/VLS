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
        private static readonly List<Enemy> enemies = [];
        private static readonly object _lock = new();

        public static event Action<Enemy>? EnemyAdded;
        public static event Action<Guid>? EnemyRemoved;

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

        //public static void UpdateEnemyPosition(Guid id, Vector3 newDirection, double newDistance)
        //{
        //    lock (_lock)
        //    {
        //        var enemy = enemies.FirstOrDefault(e => e.Id == id);
        //        if (enemy != null)
        //        {
        //            enemy.UpdatePosition(newDirection, newDistance);
        //            EnemyUpdated?.Invoke(id, enemy.Direction);
        //        }
        //    }
        //}


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