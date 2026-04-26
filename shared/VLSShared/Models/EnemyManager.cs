// EnemyManager.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using VLSShared.Enums;

namespace VLSShared.Models
{
    public static class EnemyManager
    {
        private static readonly List<Enemy> enemies = [];
        private static readonly object _lock = new();


        public static event Action<Guid, Vector3, double, double, double>? OnEnemySpawned;
        public static event Action<Guid, Vector3, Vector3, HitZone, float, float>? OnEnemyHit;
        public static event Action<Guid, Vector3>? OnEnemyDead;





        public static void AddEnemy(Enemy enemy)
        {
            lock (_lock)
            {
                enemies.Add(enemy);

                OnEnemySpawned?.Invoke(enemy.Id, enemy.Direction, enemy.Distance, enemy.RenderDistance, enemy.Scale);
            }
        }

        public static void CheckBulletCollision(Bullet bullet)
        {
            if (bullet.IsLanded) return;

            lock (_lock)
            {
                foreach (var enemy in enemies)
                {
                    Vector3 bulletDir = bullet.Direction;   // нормированное направление к текущей позиции пули
                    Vector3 enemyDir = enemy.Direction;     // нормированное направление к врагу

                    // --- Условие по мнимой дистанции ---
                    // Пуля долетела до врага, если мнимое расстояние врага лежит между предыдущей и текущей мнимой дистанцией пули.
                    if (enemy.Distance < bullet.DistancePrevious || enemy.Distance > bullet.Distance)
                        continue;

                    float dot = Vector3.Dot(bulletDir, enemyDir);
                    if (dot <= 0) continue;                 // пуля и враг в противоположных полушариях

                    // Точка пересечения луча пули с физической плоскостью врага (на RenderDistance от камеры)
                    double t = enemy.RenderDistance / dot;   // расстояние вдоль луча до плоскости
                    Vector3 O = enemyDir * (float)enemy.RenderDistance;
                    Vector3 P = bulletDir * (float)t;
                    Vector3 diff = P - O;

                    // Локальные оси плоскости (как в CustomObject3D.LookAt)
                    Vector3 worldUp = new(0, 1, 0);
                    Vector3 right = Vector3.Cross(worldUp, enemyDir);
                    right = Vector3.Normalize(right);
                    Vector3 realUp = Vector3.Cross(enemyDir, right);
                    realUp = Vector3.Normalize(realUp);

                    float localX = Vector3.Dot(diff, right);
                    float localY = Vector3.Dot(diff, realUp);

                    double halfS = enemy.Scale / 2;

                    if (Math.Abs(localX) > halfS || Math.Abs(localY) > halfS)
                        continue;                           // мимо прямоугольника врага

                    // UV-координаты (соответствуют развёртке PlaneMesh)
                    float u = (float)((halfS - localX) / enemy.Scale);
                    float v = (float)((halfS - localY) / enemy.Scale);

                    HitZone zone = enemy.HitZoneChecker?.Invoke(u, v) ?? HitZone.None;
                    if (zone == HitZone.None)
                        continue;                           // попали в прозрачную область маски

                    Vector3 hitPoint = O + right * localX + realUp * localY;

                    bullet.IsLanded = true;
                    OnEnemyHit?.Invoke(enemy.Id, bulletDir, hitPoint, zone, u, v);
                    break;                                  // одна пуля — один враг
                }
            }
        }
    }
}