using KeyGeneratorApp;
using KeyGeneratorApp.Dtos;
using KeyGeneratorApp.Services;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text.Json;

public class Program
{
    static void Main(string[] args)
    {
        int type = 0;

        if (args.Length > 0)
        {
            type = int.Parse(args[0]);
        }
        else
        {
            Console.WriteLine("\nSelect a process type to proceed:");
            Console.WriteLine("1 => Generate Key");
            Console.WriteLine("2 => Generate Encrypted Jwt Token");
            Console.WriteLine("3 => Decrypt the Encrypted Jwt Token and read claims");
            Console.WriteLine();

            var inputType = Console.ReadLine();
            if (!int.TryParse(inputType, out type))
            {
                Console.WriteLine("\nPlease enter valid number");
                Main(args);
            }
        }

        if (type == 1)
        {
            // Generate a random key of size 256 bits
            var key = KeyGenerator.GenerateRandomKey(256);
            // Print the generated key
            Console.WriteLine($"\nGenerated Key: {key}");
            Console.ReadLine();
        }
        else if (type == 2)
        {
            var arg1 = args?.Length > 1 ? args[1] : null;
            var arg2 = args?.Length > 2 ? args[2] : null;
            var arg3 = args?.Length > 3 ? args[3] : null;
            Console.WriteLine($"\nEnter Partner Code: {arg1}");
            var partnerCode = arg1 ?? Console.ReadLine();

            Console.WriteLine($"\nEnter MemberNbr: {arg2}");
            var memberNbr = arg2 ?? Console.ReadLine();

            Console.WriteLine($"\nEnter KeyId: {arg3}");
            var keyId = arg3 ?? Console.ReadLine();

            var tokenRequest = new TokenRequestDto
            {
                EncKeyId = keyId,
                MemberNbr = memberNbr,
                PartnerCode = partnerCode
            };

            var encryptedToken = TokenService.GenerateEncryptedToken(tokenRequest);

            // Print the generated Jwt encrypted token
            Console.WriteLine($"\nGenerated encrypted Jwt token: {encryptedToken}");
            Console.ReadLine();
        }
        else if(type == 3)
        {
            var arg1 = args?.Length > 1 ? args[1] : null;
            Console.WriteLine($"\nEnter encrypted Jwt token: {arg1}");
            var encryptedTokenRequest = arg1 ?? Console.ReadLine();

            var decryptedToken = TokenService.GetRequestFromToken(encryptedTokenRequest);

            var json = JsonConvert.SerializeObject(decryptedToken);

            // Print the generated Jwt encrypted token
            Console.WriteLine($"\nDecrypted Jwt token claims: {json}");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("\nInvalid option, try again");
            Main(args);
        }

    }
}