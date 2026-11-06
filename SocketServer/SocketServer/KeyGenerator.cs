using System.Security.Cryptography;

namespace SocketServer
{
    internal class KeyGenerator
    {
        public static void Generate()
        {
            byte[] key = RandomNumberGenerator.GetBytes(32);

            string secretKey = Convert.ToBase64String(key);

            Console.WriteLine(secretKey);
        }
    }
}