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

                OnEnemySpawned?.Invoke(enemy.Id, enemy.Direction, enemy.Distance, enemy.ViewportDistance, enemy.Scale);
            }
        }

        public static void CheckBulletCollision(Bullet bullet)
        {
            if (bullet.IsLanded) return;

            lock (_lock)
            {
                foreach (var enemy in enemies)
                {
                    Vector3 bulletDir = bullet.Direction;  
                    Vector3 enemyDir = enemy.Direction;     

                    
                    // Bullet has reached the enemy if the enemy distance is between bullet per-tick distances
                    if (enemy.Distance < bullet.DistancePrevious || enemy.Distance > bullet.Distance)
                        continue;

                    float dot = Vector3.Dot(bulletDir, enemyDir);
                    if (dot <= 0) continue;                 // bullet and emnemy are in diametrically different directions

                    // Dot of bullet ray collision with mesh plane 
                    double t = enemy.ViewportDistance / dot;  
                    Vector3 O = enemyDir * (float)enemy.ViewportDistance;
                    Vector3 P = bulletDir * (float)t;
                    Vector3 diff = P - O;

                    // Local axes
                    Vector3 worldUp = new(0, 1, 0);
                    Vector3 right = Vector3.Cross(worldUp, enemyDir);
                    right = Vector3.Normalize(right);
                    Vector3 realUp = Vector3.Cross(enemyDir, right);
                    realUp = Vector3.Normalize(realUp);

                    float localX = Vector3.Dot(diff, right);
                    float localY = Vector3.Dot(diff, realUp);

                    double halfS = enemy.Scale / 2;

                    if (Math.Abs(localX) > halfS || Math.Abs(localY) > halfS) continue;    // Bullet did not collide a mesh plane                         

                    // UVs 
                    float u = (float)((halfS - localX) / enemy.Scale);
                    float v = (float)((halfS - localY) / enemy.Scale);

                    HitZone zone = enemy.HitZoneChecker?.Invoke(u, v) ?? HitZone.None;

                    if (zone == HitZone.None) continue;                           // bullet did not collide the pixel w/ hitbox area

                    Vector3 hitPoint = O + right * localX + realUp * localY;

                    bullet.IsLanded = true;
                    OnEnemyHit?.Invoke(enemy.Id, bulletDir, hitPoint, zone, u, v);
                    break;                                  
                }
            }
        }
    }
}