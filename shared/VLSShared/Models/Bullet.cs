using System.Numerics;

namespace VLSShared.Models
{
    public class Bullet(Vector3 startPos, Vector3 cameraLook,
        Func<int, int, double> getDistanceAtPixel, Func<Vector3, (int X, int Y)> getTextureCoordinatesFromDirection)
    {
        private const float Mass = 0.0113f; // Масса, кг
        private const int V0 = 800; // Начальная скорость, м/сек
        private const float G1 = 0.445f; // Баллистический коэффицент
        private const float PAir = 1.225f; // Плотность воздуха, кг/м3
        private const float D = 0.00792f; // Диаметр пули, м

        private const float S = D * D; // Площадь поперечного сечения пули, м2
        private const float G = 9.81f; // Ускорение свободного падения, м/с2
        private const float FormFactor = (float)(Mass * 2.2 / ((D * 39.37) * (D * 39.37) * G1)); // Формула форм-фактора + перевод кг в фунты, м в дюймы

        public double DistancePrevious { get; set; } = 0;
        public double Distance { get; set; } = 0;
        private double FlightTime { get; set; } = 0;

        private readonly Func<int, int, double> GetDistanceAtPixel = getDistanceAtPixel;
        private readonly Func<Vector3, (int X, int Y)> GetPixelFromDirection = getTextureCoordinatesFromDirection;

        public Vector3 Position { get; private set; } = startPos; // мировые координаты
        private Vector3 Velocity = cameraLook * V0; // вектор скорости, м/с

        public Guid Id { get; } = Guid.NewGuid();
        public Vector3 Direction { get; private set; } = cameraLook;

        internal void Update(float dt)
        {
            float V = Velocity.Length();
            if (V < 0.1) IsLanded = true;
            if (IsLanded) return;

            // 1. Коэффициент лобового сопротивления
            float Cd = ComputeCd(V);

            // 2. Сила сопротивления воздуха
            Vector3 dragForce = (float)(-0.5 * PAir * V * V * Cd * S) * Vector3.Divide(Velocity, V);

            // 3. Сила тяжести
            Vector3 gravityForce = new(0, -Mass * G, 0);

            // 4. Ускорение
            Vector3 acceleration = (dragForce + gravityForce) / Mass;

            // 5. Интегрирование (метод Эйлера)
            Velocity += acceleration * dt;
            Position += Velocity * dt;

            // 6. Обновление направления (для ориентации объекта)
            Direction = Vector3.Normalize(Velocity);

            // 7. Проверка попадания по карте глубины
            Vector3 directionFromCamera = Vector3.Normalize(Position);
            var (pixelX, pixelY) = GetPixelFromDirection(directionFromCamera);
            double distance = Position.Length();
            double depth = GetDistanceAtPixel(pixelX, pixelY);

            if (distance >= depth)
            {
                IsLanded = true;
            }

            DistancePrevious = Distance;
            Distance = distance;
            FlightTime += dt;
            System.Diagnostics.Debug.WriteLine(
    $"[Bullet {Id.ToString().Substring(0, 8)}] Physics Pos: ({Position.X:F3}, {Position.Y:F3}, {Position.Z:F3}) Dist={Position.Length():F1}");
        }

        public bool IsLanded { get; set; } = false;

        private float ComputeCd(double V)
        {
            float M = (float)(V / 340.0);
            (float m, float cd)[] points = new (float, float)[]
            {
                (0.0f, 0.15f), (0.8f, 0.20f), (0.95f, 0.32f),
                (1.0f, 0.45f), (1.1f, 0.45f), (1.2f, 0.42f),
                (1.5f, 0.36f), (2.0f, 0.33f), (2.5f, 0.30f),
                (3.0f, 0.29f)
            };

            for (int i = 0; i < points.Length - 1; i++)
                if (M >= points[i].m && M <= points[i + 1].m)
                {
                    float t = (M - points[i].m) / (points[i + 1].m - points[i].m);
                    float cd = points[i].cd + t * (points[i + 1].cd - points[i].cd);
                    return FormFactor * cd;
                }

            return FormFactor * (M < points[0].m ? points[0].cd : points[^1].cd);
        }

        public override string ToString()
        {
            return $" Distance: {Distance:F1} m, FlightTime: {FlightTime:F2} s, " +
                   $"Position: ({Position.X:F2}, {Position.Y:F2}, {Position.Z:F2})";
        }
    }
}