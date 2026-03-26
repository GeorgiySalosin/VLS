using VLSShared.Models;

namespace VLSShared.Interfaces
{
    /*A template constructor for creating different modes*/
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