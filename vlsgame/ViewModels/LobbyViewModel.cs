using System.Windows.Media;
using VLSGame.Services;
using VLSShared.Interfaces;
using VLSShared.Models;

namespace VLSGame.ViewModels
{
    public class LobbyViewModel : ViewModelBase
    {
        private string serverIp = "192.168.0.106";
        private string serverPort = "8888";
        private string connectionStatus = "Disconnected";
        private Brush connectionStatusColor = Brushes.Red;
        private string lastResponse = "The server has not replied";
        private readonly List<string> messages = [];

        private IGameMode? currentGameMode;

        public event EventHandler<string>? MessageAdded;

        public LobbyViewModel()
        {
            NetworkService.Instance.MessageReceived += OnMessageReceived;
            NetworkService.Instance.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        public string ServerIp
        {
            get => serverIp;
            set => Set(ref serverIp, value);
        }

        public string ServerPort
        {
            get => serverPort;
            set => Set(ref serverPort, value);
        }

        public string ConnectionStatus
        {
            get => connectionStatus;
            private set => Set(ref connectionStatus, value);
        }

        public Brush ConnectionStatusColor
        {
            get => connectionStatusColor;
            private set => Set(ref connectionStatusColor, value);
        }

        public string LastResponse
        {
            get => lastResponse;
            private set => Set(ref lastResponse, value);
        }

        public IReadOnlyList<string> Messages => messages.AsReadOnly();

        public bool IsConnected => NetworkService.Instance.IsConnected;

        public IGameMode? CurrentGameMode => currentGameMode;


        /* The point where the singleplayer was assigned a panorama*/
        public async Task StartSinglePlayerAsync(string panoramaPath)
        {
            currentGameMode = new SinglePlayerGameMode();
            await currentGameMode.StartAsync();

            if (currentGameMode is SinglePlayerGameMode singlePlayer)
            {
                singlePlayer.SetPanoramaPath(panoramaPath);
            }
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
            {
                await DisconnectAsync();
                return;
            }

            if (!int.TryParse(ServerPort, out int port))
            {
                System.Windows.MessageBox.Show("Invalid port", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            try
            {
                AddMessage($"Connecting to {ServerIp}:{port}...");
                await NetworkService.Instance.ConnectAsync(ServerIp, port);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Connection error: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                AddMessage($"Error: {ex.Message}");
            }
        }
        public async Task DisconnectAsync()
        {
            await NetworkService.Instance.DisconnectAsync();
        }
        public void Disconnect()
        {
            NetworkService.Instance.DisconnectAsync().ConfigureAwait(false);
        }
        public async Task SendMouseClickAsync(double x, double y)
        {
            string clickData = $"X={x:F0}, Y={y:F0}";
            await NetworkService.Instance.SendMessageAsync("mouse_click", clickData);
        }
        private void OnConnectionStatusChanged(object? sender, bool isConnected)
        {
            ConnectionStatus = isConnected ? "Connected" : "Disconnected";
            ConnectionStatusColor = isConnected ? Brushes.Green : Brushes.Red;

            if (!isConnected)
            {
                AddMessage("Disconnected from server");
            }
        }
        private void OnMessageReceived(object? sender, ServerResponse response)
        {
            LastResponse = $"[{response.Timestamp:HH:mm:ss}] {response.Message}";
            AddMessage($"Response: {response.Message}");
        }
        private void AddMessage(string message)
        {
            messages.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            while (messages.Count > 50)
            {
                messages.RemoveAt(messages.Count - 1);
            }
            MessageAdded?.Invoke(this, message);
        }
    }
}