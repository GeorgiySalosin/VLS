using VLSShared.Models;

namespace VLSShared.Interfaces
{
    public interface IInputHandler
    {
        event EventHandler<PlayerInput>? OnInput;
        void HandleMouseClick(double x, double y, string button);
        void HandleMouseMove(double deltaX, double deltaY);
        void HandleKeyPress(string key);
        void HandleKeyRelease(string key);
    }
}