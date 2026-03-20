using VLSShared.Interfaces;
using VLSShared.Models;

namespace VLSGame.Services
{
    public sealed class GameModeFactory
    {
        private static readonly Lazy<GameModeFactory> _instance =
        new(() => new GameModeFactory(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static GameModeFactory Instance => _instance.Value;



        public IGameMode CreateGameMode(GameMode mode)
        {
            return mode switch
            {
                GameMode.SinglePlayer => new SinglePlayerGameMode(),
                GameMode.MultiPlayer => new MultiPlayerGameMode(),
                _ => throw new ArgumentException($"Unknown game mode: {mode}")
            };
        }
    }


}