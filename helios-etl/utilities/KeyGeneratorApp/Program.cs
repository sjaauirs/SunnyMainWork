using KeyGeneratorApp;

public class Program
{
    static void Main(string[] args)
    {
        // Generate RSA Keys
        //var (publicKey, privateKey) = RSAKeyGenerator.GenerateKeyPair();

        //Console.WriteLine("Public Key:");
        //Console.WriteLine(publicKey);

        //Console.WriteLine("\nPrivate Key:");
        //Console.WriteLine(privateKey);

        //Convert PGP keys to base64 string
        string publicKeyBase64 = RSAKeyGenerator.ConvertPGPKeyToBase64String(@"c:\tmp\Sunny_Admin_0x191C6A45_public.asc");
        Console.WriteLine("Public Key:");
        Console.WriteLine(publicKeyBase64);

        string privateKeyBase64 = RSAKeyGenerator.ConvertPGPKeyToBase64String(@"c:\tmp\Sunny_Admin_0x191C6A45_SECRET.asc");
        Console.WriteLine("\nPrivate Key:");
        Console.WriteLine(privateKeyBase64);

        //Convert FIS SSL Certificate to base64 string
        string base64Cert = RSAKeyGenerator.ConvertFISSSLCertificateToBase64String(@"c:\tmp\Derinsu.Savas.p12");
        Console.WriteLine("SSL Certificate Base64 String:");
        Console.WriteLine(base64Cert);

        //Convert KP Certificate to base64 string
        //string base64Cert1 = RSAKeyGenerator.ConvertFISSSLCertificateToBase64String(@"c:\tmp\helios-dev-redshift-sftp-private-key.PEM");
        //Console.WriteLine("KP Certificate Base64 String:");
        //Console.WriteLine(base64Cert1);

    }
}