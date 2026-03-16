using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace VLSServer
{

    class Program
    {
        private static TcpListener? tcpListener;
        private static List<ClientHandler> connectedClients = new();
        private static readonly object lockObject = new();

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Starting the server ===");

            try
            {
                
                int port = 8888;
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();

                Console.WriteLine($"Server runs on {port} port");
                Console.WriteLine($"Local IP: {GetLocalIPAddress()}");
                Console.WriteLine($"Public IP: {await GetPublicIPAddress()}");
                Console.WriteLine("Waiting for client connection...\n");


                // append and handle new client
                while (true)
                {
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                    ClientHandler handler = new ClientHandler(tcpClient);

                    lock (lockObject)
                    {
                        connectedClients.Add(handler);
                    }

                    _ = Task.Run(() => handler.HandleClientAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "Not found";
        }

        private static async Task<string> GetPublicIPAddress()
        {
            try
            {
                using HttpClient client = new();
                return await client.GetStringAsync("https://api.ipify.org");
            }
            catch
            {
                return "Not defined";
            }
        }
    }



}