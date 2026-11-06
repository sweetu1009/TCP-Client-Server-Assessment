
using System.Net.Sockets;

namespace SocketClient
{
    internal class Program
    {
        static readonly byte[] aesKey =
     Convert.FromBase64String(
         Environment.GetEnvironmentVariable("SOCKET_AES_KEY")
         ?? throw new InvalidOperationException("AES key not configured."));
        static async Task Main(string[] args)
        {
            string serverIp =
    Environment.GetEnvironmentVariable("SOCKET_SERVER_IP")
    ?? "127.0.0.1";

            int port = int.TryParse(
                Environment.GetEnvironmentVariable("SOCKET_PORT"),
                out int configuredPort)
                ? configuredPort
                : 5000;

            try
            {
                using TcpClient client = new TcpClient();

                await client.ConnectAsync(serverIp, port);

                Console.WriteLine("Connected to TCP Server!");

                using NetworkStream stream = client.GetStream();

                using StreamReader reader = new StreamReader(stream);

                using StreamWriter writer = new StreamWriter(stream)
                {
                    AutoFlush = true
                };

                Console.Write("Enter request (Example: SetA-Two): ");

                string? request = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(request))
                {
                    Console.WriteLine("Request cannot be empty.");
                    return;
                }

                string encryptedRequest = AesEncryption.Encrypt(request, aesKey);

                await writer.WriteLineAsync(encryptedRequest);

                Console.WriteLine("\nServer Response:");

                string? response;

                while ((response = await reader.ReadLineAsync()) != null)
                {
                    string decryptedResponse = AesEncryption.Decrypt(response, aesKey);

                    Console.WriteLine(decryptedResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client Error: {ex.Message}");
            }
        }
    }
}
