using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using VLSShared.Models;

namespace VLSGame
{
    public partial class MainWindow : Window
    {
        private TcpClient? tcpClient;
        private NetworkStream? stream;
        private bool isConnected = false;
        private readonly object sendLock = new();

        public MainWindow()
        {
            InitializeComponent();
            AddMessage("App launched");
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected)
            {
                Disconnect();
                return;
            }

            string ip = ServerIpTextBox.Text;
            if (!int.TryParse(ServerPortTextBox.Text, out int port))
            {
                MessageBox.Show("Invalid port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                AddMessage($"Connecting to {ip}:{port}...");

                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(ip, port);
                stream = tcpClient.GetStream();

                isConnected = true;

                ConnectionStatusText.Text = "Connected";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Green;
                ConnectButton.Content = "Disconnect";

                AddMessage("Connection established");


                _ = ReceiveMessagesAsync();


                await SendMessageAsync("ping", null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                AddMessage($"Error: {ex.Message}");
            }
        }

        private void Disconnect()
        {
            isConnected = false;

            stream?.Close();
            tcpClient?.Close();

            ConnectionStatusText.Text = "Disconnected";
            ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Red;
            ConnectButton.Content = "Connect";

            AddMessage("Отключено от сервера");
        }


        private async void ClickArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isConnected)
            {
                MessageBox.Show("No server connection", "Warning",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var position = e.GetPosition(sender as IInputElement);
            string clickData = $"X={position.X:F0}, Y={position.Y:F0}";

            await SendMouseClickAsync(clickData);
        }

        private async Task SendMouseClickAsync(string clickData)
        {
            var message = new ClientMessage
            {
                Type = "mouse_click",
                Data = clickData
            };

            await SendMessageAsync(message.Type, message.Data);
        }

        private async Task SendMessageAsync(string type, object? data)
        {
            if (!isConnected || stream == null) return;

            try
            {
                var message = new ClientMessage
                {
                    Type = type,
                    Data = data
                };

                string jsonMessage = JsonSerializer.Serialize(message);
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonMessage);

                lock (sendLock)
                {
                    stream.Write(dataBytes, 0, dataBytes.Length);
                }

                AddMessage($"Sent: {type}");
            }
            catch (Exception ex)
            {
                AddMessage($"Error sending data: {ex.Message}");
                Disconnect();
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (isConnected && tcpClient != null && tcpClient.Connected)
                {
                    int bytesRead = await stream!.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0) break; // Server has closed the connection

                    string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Обрабатываем в UI потоке
                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            var response = JsonSerializer.Deserialize<ServerResponse>(responseJson);
                            if (response != null)
                            {
                                LastResponseText.Text = $"[{response.Timestamp:HH:mm:ss}] {response.Message}";
                                AddMessage($"Response: {response.Message}");
                            }
                        }
                        catch
                        {
                            LastResponseText.Text = $"Response: {responseJson}";
                            AddMessage($" (raw): {responseJson}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    AddMessage($"Error : {ex.Message}");
                    Disconnect();
                });
            }
        }

        private void AddMessage(string message)
        {
            MessagesListBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");

            // restrict message count
            while (MessagesListBox.Items.Count > 50)
            {
                MessagesListBox.Items.RemoveAt(MessagesListBox.Items.Count - 1);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Disconnect();
            base.OnClosed(e);
        }

        private void OpenPanoramaButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите HDRI панораму",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.hdr;*.exr|Все файлы|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                var viewerWindow = new Match(dialog.FileName);
                viewerWindow.Show();
                this.Close(); // Закрываем главное окно (опционально)
            }
        }
    }
}