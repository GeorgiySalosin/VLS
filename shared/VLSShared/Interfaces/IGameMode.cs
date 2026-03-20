using VLSShared.Models;

namespace VLSShared.Interfaces
{
    public interface IGameMode
    {
        event EventHandler<PlayerInput>? InputReceived;
        event EventHandler<string>? GameEvent;
        
        Task StartAsync();
        Task StopAsync();
        Task ProcessInputAsync(PlayerInput input);
        Task<PanoramaInfo> GetPanoramaAsync();
    }
}