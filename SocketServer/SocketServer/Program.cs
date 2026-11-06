
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace SocketServer
{
    internal class Program
    {
        static Dictionary<string, List<Dictionary<string, int>>> data = new();

        static readonly byte[] aesKey =
    Convert.FromBase64String(
        Environment.GetEnvironmentVariable("SOCKET_AES_KEY")
        ?? throw new InvalidOperationException("AES key not configured."));

        static async Task Main(string[] args)
        {
            // Read JSON file
            string filePath = Path.Combine(
                AppContext.BaseDirectory, "Data.json");

            string json = await File.ReadAllTextAsync(filePath);

            data = JsonSerializer.Deserialize<
                Dictionary<string, List<Dictionary<string, int>>>>(json)
                ?? new();

            Console.WriteLine("JSON Data Loaded Successfully!");


            // Start TCP Server
            //int port = 5000;
            int port = int.TryParse(
    Environment.GetEnvironmentVariable("SOCKET_PORT"),
    out int configuredPort)
    ? configuredPort
    : 5000;

            TcpListener server = new TcpListener(
                IPAddress.Any, port);

            server.Start();

            Console.WriteLine("TCP Server Started...");
            Console.WriteLine($"Listening on port: {port}");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();

                Console.WriteLine("Client Connected!");

                _ = Task.Run(() => HandleClient(client));
            }
        }

        static async Task HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream))
                using (StreamWriter writer = new StreamWriter(stream)
                {
                    AutoFlush = true
                })
                {
                    string? request = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(request))
                    {
                        await writer.WriteLineAsync(
                            AesEncryption.Encrypt("EMPTY", aesKey));
                        return;
                    }

                    request = AesEncryption.Decrypt(request, aesKey);

                    Console.WriteLine($"Decrypted Request: {request}");

                    Console.WriteLine($"Received: {request}");

                    string[] parts = request.Split('-');

                    if (parts.Length != 2 ||
     !data.TryGetValue(parts[0], out var items))
                    {
                        await writer.WriteLineAsync(
                            AesEncryption.Encrypt("EMPTY", aesKey));
                        return;
                    }

                    if (items.Count == 0 ||
    !items[0].TryGetValue(parts[1], out int count) ||
    count <= 0)
                    {
                        Console.WriteLine("Sending EMPTY response");

                        await writer.WriteLineAsync(
                            AesEncryption.Encrypt("EMPTY", aesKey));

                        return;
                    }

                    Console.WriteLine($"Found value: {count}");

                    for (int i = 0; i < count; i++)
                    {
                        string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        string encryptedResponse = AesEncryption.Encrypt(dateTime, aesKey);

                        await writer.WriteLineAsync(encryptedResponse);

                        Console.WriteLine($"Sent: {dateTime}");

                        if (i < count - 1)
                        {
                            await Task.Delay(1000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client Error: {ex.Message}");
            }
        }
    }
}
