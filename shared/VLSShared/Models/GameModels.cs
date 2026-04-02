using VLSShared.Enums;

namespace VLSShared.Models
{
    public class PlayerInput
    {
        public InputType Type { get; set; }
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MouseClickData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string? Button { get; set; }
    }

    public class MouseMoveData
    {
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
    }

    public class PanoramaInfo
    {
        public string? ImagePath { get; set; }
    }
}