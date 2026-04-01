using VLSShared.Models;

namespace VLSShared.Interfaces
{
 /*Input handler that will be used by the server (multiplayer) and the client (singleplayer)*/
    public interface IInputHandler
    {
        event EventHandler<PlayerInput>? OnInput;
        void HandleMouseClick(double x, double y, string button); // MouseClickData?
        void HandleMouseMove(double deltaX, double deltaY); // MouseMoveData?
        void HandleKeyPress(string key);
        void HandleKeyRelease(string key);
    }
}