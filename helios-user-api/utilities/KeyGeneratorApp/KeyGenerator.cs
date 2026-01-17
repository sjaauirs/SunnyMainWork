using System.Security.Cryptography;
namespace KeyGeneratorApp
{
    public static class KeyGenerator
    {
        public static string GenerateRandomKey(int keySize)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = keySize; // Key size in bits
                aes.GenerateKey(); // Generate a random key
                return Convert.ToBase64String(aes.Key);
            }
        }

        public static string GenerateRandomIV()
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.GenerateIV(); // Generate a random IV
                byte[] iv = new byte[18];
                Array.Copy(aes.IV, iv, aes.IV.Length);
                return Convert.ToBase64String(aes.IV);
            }
        }
    }
}