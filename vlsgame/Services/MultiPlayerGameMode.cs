using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLSShared.Interfaces;
using VLSShared.Models;

namespace VLSGame.Services
{
    public class MultiPlayerGameMode : IGameMode
    {
        public event EventHandler<PlayerInput>? InputReceived;
        public event EventHandler<string>? GameEvent;

        public async Task StartAsync()
        {
            GameEvent?.Invoke(this, "Multiplayer mode started");
            await NetworkService.Instance.SendMessageAsync("game_start", null);
        }

        public async Task StopAsync()
        {
            GameEvent?.Invoke(this, "Multiplayer mode stopped");
            await NetworkService.Instance.SendMessageAsync("game_stop", null);
        }

        public async Task ProcessInputAsync(PlayerInput input)
        {
            await NetworkService.Instance.SendMessageAsync("player_input", input);
        }

        public async Task<PanoramaInfo> GetPanoramaAsync()
        {
            // pass for receiving panorama from server
            return new PanoramaInfo();
        }
    }
}
