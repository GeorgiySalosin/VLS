// EnemyManager.cs
using System.Numerics;
//using VLSGame.Rendering;
using VLSShared.Enums;

namespace VLSShared.Models
{
    public static class PlayerManager
    {
        private static readonly List<Player> players = [];
        private static readonly object _lock = new();


        public static event Action<Guid, Vector3>? OnPlayerSpawned;
        /// <summary>
        /// params: last bullet location (world), hitzone (to determine an amount of blood)
        /// </summary>
        public static event Action<Vector3, HitZoneInfo>? OnPlayerHit;      
        public static event Action<Guid> OnPlayerDead;

        public static void AddPlayer(Player player)
        {
            lock (_lock)
            {
                players.Add(player);

                OnPlayerSpawned?.Invoke(player.Id, player.Direction);
            }
        }
         /// <summary>
         ///  used to specify a distance (calculated in rendermanager) after setting up 3d model
         /// </summary>
        public static void SetPlayerDistance(Guid playerId, double distance)
        {
            lock (_lock)
            {
                var player = players.FirstOrDefault(p => p.Id == playerId);
                    player?.Distance = distance;
            }
        }

        /// <summary>
        ///  used to specify a scale (calculated in rendermanager) after setting up 3d model
        /// </summary>
        public static void SetPlayerScale(Guid playerId, double scale)
        {
            lock (_lock)
            {
                var player = players.FirstOrDefault(p => p.Id == playerId);
                    player?.Scale = scale;
            }
        }

        public static bool CheckBulletCollision(Bullet bullet)
        {
            lock (_lock)
            {
                for (int i = players.Count - 1; i >= 0; i--) // It is better to do a reverse parting by index, as we are removing elements from the array
                {
                    Player player = players[i];
                    Vector3 bulletDir = bullet.Direction;
                    Vector3 enemyDir = player.Direction;


                    float dot = Vector3.Dot(bulletDir, enemyDir);
                    if (dot <= 0) continue;

                    // Расстояние до плоскости вдоль начального направления (не используем для отсечения, только для порядка)
                    double tPlane = player.Distance / dot;
                    if (tPlane < bullet.DistancePrevious || tPlane > bullet.Distance)
                        continue; // пуля ещё не долетела до плоскости или уже далеко за ней

                    // Теперь проверяем пересечение отрезка [камера, bullet.Position] с плоскостью
                    Vector3 camPos = Vector3.Zero;
                    Vector3 planeNormal = enemyDir;
                    Vector3 planePoint = enemyDir * (float)player.Distance;

                    Vector3 rayDir = bullet.Position - camPos;
                    float denom = Vector3.Dot(planeNormal, rayDir);
                    if (Math.Abs(denom) < 1e-6) continue;

                    float t = Vector3.Dot(planePoint - camPos, planeNormal) / denom;
                    if (t < 0 || t > 1) continue; // пересечение не на отрезке

                    Vector3 hitPoint = camPos + rayDir * t;
                    Vector3 diff = hitPoint - planePoint;

                    // Локальные оси (с учётом возможной вертикальности enemyDir)
                    Vector3 worldUp = new(0, 1, 0);
                    if (Math.Abs(Vector3.Dot(worldUp, enemyDir)) > 0.9999)
                        worldUp = new Vector3(1, 0, 0);
                    Vector3 right = Vector3.Normalize(Vector3.Cross(worldUp, enemyDir));
                    Vector3 realUp = Vector3.Normalize(Vector3.Cross(enemyDir, right));

                    float localX = Vector3.Dot(diff, right);
                    float localY = Vector3.Dot(diff, realUp);
                    double halfS = player.Scale / 2;

                    if (Math.Abs(localX) > halfS || Math.Abs(localY) > halfS) continue;

                    // Попадание!
                    float u = (float)((halfS - localX) / player.Scale);
                    float v = (float)((halfS - localY) / player.Scale);
                    HitZoneInfo zone = player.HitZoneChecker?.Invoke(u, v) ?? HitZoneInfo.None;
                    if (zone == HitZoneInfo.None) continue;

                    Vector3 hitPointWorld = planePoint + right * localX + realUp * localY;
                    player.ApplyDamage(zone);

                    OnPlayerHit?.Invoke(bullet.Position, zone);

                    if (player.Hp <= 0) 
                    {
                        players.RemoveAt(i);
                        OnPlayerDead?.Invoke(player.Id);
                    }
                    return true;
                }
                return false;
            }
        }
    }
}