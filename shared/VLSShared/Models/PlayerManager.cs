// EnemyManager.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using VLSShared.Enums;

namespace VLSShared.Models
{
    public static class PlayerManager
    {
        private static readonly List<Player> players = [];
        private static readonly object _lock = new();


        public static event Action<Guid, Vector3>? OnPlayerSpawned;
        public static event Action<Guid, Vector3, Vector3, HitZone, float, float>? OnPlayerHit;
        public static event Action<Guid, Vector3>? OnPlayerDead;





        public static void AddPlayer(Player player)
        {
            lock (_lock)
            {
                players.Add(player);

                OnPlayerSpawned?.Invoke(player.Id, player.Direction);
            }
        }

        public static void CheckBulletCollision(Bullet bullet)
        {
            if (bullet.IsLanded) return;

            lock (_lock)
            {
                foreach (var player in players)
                {
                    Vector3 bulletDir = bullet.Direction;  
                    Vector3 enemyDir = player.Direction;     

                    
                    // Bullet has reached the enemy if the enemy distance is between bullet per-tick distances
                    if (player.Distance < bullet.DistancePrevious || player.Distance > bullet.Distance)
                        continue;

                    float dot = Vector3.Dot(bulletDir, enemyDir);
                    if (dot <= 0) continue;                 // bullet and emnemy are in diametrically different directions

                    // Dot of bullet ray collision with mesh plane 
                    double t = player.ViewportDistance / dot;  
                    Vector3 O = enemyDir * (float)player.ViewportDistance;
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

                    double halfS = player.Scale / 2;

                    if (Math.Abs(localX) > halfS || Math.Abs(localY) > halfS) continue;    // Bullet did not collide a mesh plane                         

                    // UVs 
                    float u = (float)((halfS - localX) / player.Scale);
                    float v = (float)((halfS - localY) / player.Scale);

                    HitZone zone = player.HitZoneChecker?.Invoke(u, v) ?? HitZone.None;

                    if (zone == HitZone.None) continue;                           // bullet did not collide the pixel w/ hitbox area

                    Vector3 hitPoint = O + right * localX + realUp * localY;

                    bullet.IsLanded = true;
                    OnPlayerHit?.Invoke(player.Id, bulletDir, hitPoint, zone, u, v);
                    break;                                  
                }
            }
        }
    }
}