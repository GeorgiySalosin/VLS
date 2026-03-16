using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using VLSShared.Models;

public class ClientHandler
{
    private TcpClient tcpClient;
    private NetworkStream stream;
    private string clientId = Guid.NewGuid().ToString()[..8];

    public ClientHandler(TcpClient tcpClient)
    {
        this.tcpClient = tcpClient;
        this.stream = tcpClient.GetStream();
    }

    public async Task HandleClientAsync()
    {
        Console.WriteLine($"New client [{clientId}] has connected to the server");

        try
        {
            byte[] buffer = new byte[4096];

            while (tcpClient.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0) break; // Client has disconnected

                // Декодируем сообщение
                string messageJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"[{clientId}] Received: {messageJson}");

                try
                {

                    ClientMessage? clientMsg = JsonSerializer.Deserialize<ClientMessage>(messageJson);


                    ServerResponse response = ProcessMessage(clientMsg);


                    string responseJson = JsonSerializer.Serialize(response);
                    byte[] responseData = Encoding.UTF8.GetBytes(responseJson);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
                catch (JsonException)
                {
                    ServerResponse echoResponse = new()
                    {
                        Status = "echo",
                        Message = $"Echo: {messageJson}",
                        Timestamp = DateTime.Now
                    };

                    string responseJson = JsonSerializer.Serialize(echoResponse);
                    byte[] responseData = Encoding.UTF8.GetBytes(responseJson);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error on client [{clientId}] : {ex.Message}");
        }
        finally
        {
            Close();
        }
    }

    private ServerResponse ProcessMessage(ClientMessage? message)
    {
        if (message == null)
        {
            return new ServerResponse
            {
                Status = "error",
                Message = "The message was empty",
                Timestamp = DateTime.Now
            };
        }

        switch (message.Type)
        {
            case "mouse_click":
                return new ServerResponse
                {
                    Status = "success",
                    Message = $"Mouse click detected on following coordinates: {message.Data}",
                    Timestamp = DateTime.Now
                };

            case "ping":
                return new ServerResponse
                {
                    Status = "success",
                    Message = "Server replied",
                    Timestamp = DateTime.Now
                };

            default:
                return new ServerResponse
                {
                    Status = "unknown",
                    Message = $"Unknown message type: {message.Type}",
                    Timestamp = DateTime.Now
                };
        }
    }

    private void Close()
    {
        stream.Close();
        tcpClient.Close();
        Console.WriteLine($"[{clientId}] Client has disconnected");
    }
}