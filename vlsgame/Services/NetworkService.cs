using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using VLSShared.Models;

namespace VLSGame.Services
{
    public sealed class NetworkService
    {
        public static NetworkService Instance => _instance.Value;
        private static readonly Lazy<NetworkService> _instance =
        new(() => new NetworkService(), LazyThreadSafetyMode.ExecutionAndPublication);

        
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private readonly object _sendLock = new();
        private bool _isConnected;
        private CancellationTokenSource? _cts;

        public event EventHandler<ServerResponse>? MessageReceived;
        public event EventHandler<bool>? ConnectionStatusChanged;

        public bool IsConnected => _isConnected;


        public async Task ConnectAsync(string ip, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ip, port);
                _stream = _tcpClient.GetStream();
                
                _isConnected = true;
                _cts = new CancellationTokenSource();
                
                ConnectionStatusChanged?.Invoke(this, true);
                
                _ = ReceiveMessagesAsync(_cts.Token);
                await SendMessageAsync("ping", null);
            }
            catch
            {
                await DisconnectAsync();
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            _isConnected = false;
            _cts?.Cancel();
            
            _stream?.Close();
            _tcpClient?.Close();
            
            ConnectionStatusChanged?.Invoke(this, false);
        }

        public async Task SendMessageAsync(string type, object? data)
        {
            if (!_isConnected || _stream == null) return;

            try
            {
                var message = new ClientMessage
                {
                    Type = type,
                    Data = data
                };

                string jsonMessage = JsonSerializer.Serialize(message);
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonMessage);

                lock (_sendLock)
                {
                    _stream.Write(dataBytes, 0, dataBytes.Length);
                }
            }
            catch
            {
                await DisconnectAsync();
                throw;
            }
        }

        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (_isConnected && _tcpClient?.Connected == true && !token.IsCancellationRequested)
                {
                    int bytesRead = await _stream!.ReadAsync(buffer, 0, buffer.Length, token);
                    
                    if (bytesRead == 0) break;

                    string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    try
                    {
                        var response = JsonSerializer.Deserialize<ServerResponse>(responseJson);
                        if (response != null)
                        {
                            MessageReceived?.Invoke(this, response);
                        }
                    }
                    catch
                    {
                        // Handle raw response if needed
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch
            {
                if (_isConnected)
                {
                    await DisconnectAsync();
                }
            }
        }
    }
}