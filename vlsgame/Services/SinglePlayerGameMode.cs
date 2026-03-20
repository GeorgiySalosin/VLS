using VLSShared.Interfaces;
using VLSShared.Models;

namespace VLSGame.Services
{
    public class SinglePlayerGameMode : IGameMode
    {
        private string? _panoramaPath;

        public event EventHandler<PlayerInput>? InputReceived;
        public event EventHandler<string>? GameEvent;

        public Task StartAsync()
        {
            GameEvent?.Invoke(this, "Single player mode started");
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            GameEvent?.Invoke(this, "Single player mode stopped");
            return Task.CompletedTask;
        }

        public Task ProcessInputAsync(PlayerInput input)
        {
            // Логика обработки ввода в одиночном режиме
            Console.WriteLine($"Single player input: {input.Type}");
            return Task.CompletedTask;
        }

        public Task<PanoramaInfo> GetPanoramaAsync()
        {
            return Task.FromResult(new PanoramaInfo
            {
                ImagePath = _panoramaPath
            });
        }

        public void SetPanoramaPath(string path)
        {
            _panoramaPath = path;
        }
    }
}