using System.Windows.Media;
using VLSGame.Services;
using VLSShared.Interfaces;
using VLSShared.Models;

namespace VLSGame.ViewModels
{
    public class LobbyViewModel : ViewModelBase
    {

        private string _serverIp = "192.168.0.106";
        private string _serverPort = "8888";
        private string _connectionStatus = "Disconnected";
        private Brush _connectionStatusColor = Brushes.Red;
        private string _lastResponse = "The server has not replied";
        private List<string> _messages = new();

        private IGameMode? _currentGameMode;

        public event EventHandler<string>? MessageAdded;

        public LobbyViewModel()
        {
            NetworkService.Instance.MessageReceived += OnMessageReceived;
            NetworkService.Instance.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        public string ServerIp
        {
            get => _serverIp;
            set => Set(ref _serverIp, value);
        }

        public string ServerPort
        {
            get => _serverPort;
            set => Set(ref _serverPort, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            private set => Set(ref _connectionStatus, value);
        }

        public Brush ConnectionStatusColor
        {
            get => _connectionStatusColor;
            private set => Set(ref _connectionStatusColor, value);
        }

        public string LastResponse
        {
            get => _lastResponse;
            private set => Set(ref _lastResponse, value);
        }

        public IReadOnlyList<string> Messages => _messages.AsReadOnly();

        public bool IsConnected => NetworkService.Instance.IsConnected;

        public IGameMode? CurrentGameMode => _currentGameMode;


        /* The point where the singleplayer was assigned a panorama*/
        public async Task StartSinglePlayerAsync(string panoramaPath)
        {
            _currentGameMode = new SinglePlayerGameMode();
            await _currentGameMode.StartAsync();

            if (_currentGameMode is SinglePlayerGameMode singlePlayer)
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
            _messages.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            while (_messages.Count > 50)
            {
                _messages.RemoveAt(_messages.Count - 1);
            }
            MessageAdded?.Invoke(this, message);
        }
    }
}