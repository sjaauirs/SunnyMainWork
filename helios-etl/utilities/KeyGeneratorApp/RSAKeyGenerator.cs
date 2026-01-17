using System.Text;

namespace KeyGeneratorApp
{
    public class RSAKeyGenerator
    {
        public static (string publicKey, string privateKey) GenerateKeyPair()
        {
            string publicKey;
            string privateKey;

            using (var rsa = System.Security.Cryptography.RSA.Create(4096))
            {
                var publicKeyBytes = rsa.ExportRSAPublicKey();
                publicKey = Convert.ToBase64String(publicKeyBytes);

                var privateKeyBytes = rsa.ExportRSAPrivateKey();
                privateKey = Convert.ToBase64String(privateKeyBytes);
            }

            return (publicKey, privateKey);
        }

        public static string ConvertPGPKeyToBase64String(string filePath)
        {
            try
            {
                string pgpKey = File.ReadAllText(filePath);
                string pgpKeyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(pgpKey));
                return pgpKeyBase64;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading PGP key file: " + ex.Message);
                return null;
            }
        }

        public static string ConvertFISSSLCertificateToBase64String(string filePath)
        {
            try
            {
                byte[] certBytes = File.ReadAllBytes(filePath);
                string base64Cert = Convert.ToBase64String(certBytes);

                return base64Cert;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading FIS certificate file: " + ex.Message);
                return null;
            }
        }
    }
}
