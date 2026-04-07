using VLSGame.Models;

namespace VLSShared.Models
{
    public class Bullet
    {
        //private const double M = 0.0113; // Масса, кг
        private const int V = 800; // Начальная скорость, м/сек
        //private const double G1 = 0.445; // Баллистический коэффицент
        //private const double P = 1.225; // Плотность воздуха, кг/м3
        //private const double D = 0.00792; // Диаметр пули, м

        // todo: Разобраться, какие константы нужны
        //private const double Cd = 0.674; // todo: Коэффициент лобового сопротивления
        //private const double S = Math.PI * (D / 2 * (D / 2)); // Площадь поперечного сечения пули, м2
        //private const double F_air = 0.5 * P * V * V * Cd * S; // Сила сопротивления воздуха
        //private const double G = 9.81; // Ускорение свободного падения, м/с2
        //private const double I = M * 2.2 / ((D * 39.37) * (D * 39.37) * G1); // Формула форм-фактора + перевод кг в фунты, м в дюймы

        private readonly int X;
        private int Y;
        private double Distance = 0;
        private readonly PanoramaData Panorama;

        public Bullet(int x, int y, PanoramaData panorama)
        {
            X = x;
            Y = y;
            Panorama = panorama;
        }

        internal void Process(int tickHz = 100)
        {
            // tickHz — частота тиков в секунду (например, 100). Тогда шаг времени dt = 1.0 / tickHz
            double dt = 1.0 / tickHz;
            Distance += V * dt;
            // todo
        }
        internal bool IsLanded => Panorama.GetDistanceAtPixel(X, Y) <= Distance;
    }
}
