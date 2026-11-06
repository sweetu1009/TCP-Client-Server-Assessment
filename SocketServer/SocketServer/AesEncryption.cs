
using System.Security.Cryptography;
using System.Text;

namespace SocketServer
{
    internal class AesEncryption
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;

        public static string Encrypt(string plainText, byte[] key)
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = new byte[plainBytes.Length];
            byte[] tag = new byte[TagSize];

            using (AesGcm aes = new AesGcm(key, TagSize))
            {
                aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
            }

            byte[] result = new byte[NonceSize + TagSize + cipherBytes.Length];

            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
            Buffer.BlockCopy(cipherBytes, 0, result, NonceSize + TagSize, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string encryptedText, byte[] key)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            if (encryptedBytes.Length < NonceSize + TagSize)
            {
                throw new CryptographicException("Invalid encrypted message.");
            }

            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] cipherBytes = new byte[encryptedBytes.Length - NonceSize - TagSize];
            byte[] plainBytes = new byte[cipherBytes.Length];

            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(encryptedBytes, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(encryptedBytes, NonceSize + TagSize, cipherBytes, 0, cipherBytes.Length);

            using (AesGcm aes = new AesGcm(key, TagSize))
            {
                aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
            }

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
