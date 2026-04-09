using System.Numerics;

namespace VLSShared.Models
{
    public class Bullet
    {
        private const float Mass = 0.0113f; // Масса, кг
        private const int V0 = 800; // Начальная скорость, м/сек
        private const float G1 = 0.445f; // Баллистический коэффицент
        private const float PAir = 1.225f; // Плотность воздуха, кг/м3
        private const float D = 0.00792f; // Диаметр пули, м

        private const float S = D * D; // Площадь поперечного сечения пули, м2
        private const float G = 9.81f; // Ускорение свободного падения, м/с2
        private const float FormFactor = (float)(Mass * 2.2 / ((D * 39.37) * (D * 39.37) * G1)); // Формула форм-фактора + перевод кг в фунты, м в дюймы

        internal int X { get; private set; }
        internal int Y { get; private set; }
        internal double Distance { get; private set; } = 0;
        internal double FlightTime { get; private set; } = 0;

        private readonly Func<int, int, double> GetDistanceAtPixel;
        private readonly Func<Vector3, (int X, int Y)> GetPixelFromDirection;

        private Vector3 Position; // мировые координаты (камера в (0,0,0))
        private Vector3 Velocity; // вектор скорости, м/с

        public Bullet(Vector3 startPos, Vector3 cameraLook,
            Func<int, int, double> getDistanceAtPixel, Func<Vector3, (int X, int Y)> getTextureCoordinatesFromDirection)
        {
            Position = startPos;
            Velocity = cameraLook * V0;
            GetDistanceAtPixel = getDistanceAtPixel;
            GetPixelFromDirection = getTextureCoordinatesFromDirection;
        }

        internal void Update(float dt)
        {
            float V = Velocity.Length();
            if (V < 0.1) IsLanded = true;
            if (IsLanded) return;

            // 1. Коэффициент лобового сопротивления (зависит от скорости)
            float Cd = ComputeCd(V);

            // 2. Сила сопротивления воздуха
            Vector3 dragForce = (float)(-0.5 * PAir * V * V * Cd * S) * Vector3.Divide(Velocity, (float)V);

            // 3. Сила тяжести
            Vector3 gravityForce = new Vector3(0, -Mass * G, 0);

            // 4. Ускорение
            Vector3 acceleration = (dragForce + gravityForce) / Mass;

            // 5. Интегрирование (метод Эйлера)
            Velocity += acceleration * dt;
            Position += Velocity * dt;

            // 6. Проверка попадания по карте глубины
            Vector3 direction = Position;
            direction = Vector3.Normalize(direction);
            var (pixelX, pixelY) = GetPixelFromDirection(direction);
            double distance = Position.Length();
            double depth = GetDistanceAtPixel(pixelX, pixelY);

            if (distance >= depth)
            {
                IsLanded = true;
                X = pixelX;
                Y = pixelY;
                Distance = distance;
            }

            FlightTime += dt;
        }
        internal bool IsLanded { get; private set; } = false;
        private float ComputeCd(double V)
        {
            float M = (float)(V / 340.0);
            // предопределённые точки (M, Cd_G1)
            (float m, float cd)[] points = new (float, float)[]
            {
                (0.0f, 0.15f), (0.8f, 0.20f), (0.95f, 0.32f),
                (1.0f, 0.45f), (1.1f, 0.45f), (1.2f, 0.42f),
                (1.5f, 0.36f), (2.0f, 0.33f), (2.5f, 0.30f),
                (3.0f, 0.29f)
            };
            // линейная интерполяция
            for (int i = 0; i < points.Length - 1; i++)
                if (M >= points[i].m && M <= points[i + 1].m)
                {
                    float t = (M - points[i].m) / (points[i + 1].m - points[i].m);
                    float cd = points[i].cd + t * (points[i + 1].cd - points[i].cd);
                    return FormFactor * cd;
                }
            // за пределами таблицы
            return FormFactor * (M < points[0].m ? points[0].cd : points[^1].cd);
        }
    }
}
