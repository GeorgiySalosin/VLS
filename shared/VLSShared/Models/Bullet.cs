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

        private const float S = (float)(Math.PI * (D / 2 * (D / 2))); // Площадь поперечного сечения пули, м2
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
                // Здесь можно сохранить координаты попадания для эффектов
            }

            FlightTime += dt;
        }
        internal bool IsLanded { get; private set; } = false;
        private float ComputeCd(double V)
        {
            float M = (float)(V / 340.0); // Число Маха
            // Примитивная аппроксимация Cd_G1(M) (для эталонной пули)
            float Cd_G1;
            if (M < 0.8) Cd_G1 = 0.15f;
            else if (M < 0.95) Cd_G1 = 0.25f;
            else if (M < 1.1) Cd_G1 = 0.35f;
            else if (M < 1.5) Cd_G1 = 0.45f;
            else Cd_G1 = 0.38f;
            return FormFactor * Cd_G1;
        }
    }
}
