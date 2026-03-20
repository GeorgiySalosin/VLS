using VLSShared.Interfaces;
using VLSShared.Models;

namespace VLSGame.Services
{
    public class InputService : IInputHandler
    {
        public event EventHandler<PlayerInput>? OnInput;

        public void HandleMouseClick(double x, double y, string button)
        {
            var input = new PlayerInput
            {
                Type = InputType.MouseClick,
                Data = new MouseClickData { X = x, Y = y, Button = button },
                Timestamp = DateTime.Now
            };
            
            OnInput?.Invoke(this, input);
        }

        public void HandleMouseMove(double deltaX, double deltaY)
        {
            var input = new PlayerInput
            {
                Type = InputType.MouseMove,
                Data = new MouseMoveData { DeltaX = deltaX, DeltaY = deltaY },
                Timestamp = DateTime.Now
            };
            
            OnInput?.Invoke(this, input);
        }

        public void HandleKeyPress(string key)
        {
            // Реализация для клавиатуры
        }

        public void HandleKeyRelease(string key)
        {
            // Реализация для клавиатуры
        }
    }
}