using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using KeyGeneratorApp.Dtos;
using KeyGeneratorApp.Helpers;
using System.Configuration;
using SunnyRewards.Helios.Common.Core.Helpers;

namespace KeyGeneratorApp.Services
{
    public static class TokenService
    {
        private const string SymEncryptionKey = "SYM_ENCRYPTION_KEY";
        private const string JwtSecret = "JwtSecret";
        private const string JwtIssuer = "JwtIssuer";
        private const string JwtExpirationTimeInMinutes = "JwtExpirationTimeInMinutes";
        private const string PartnerCodeClaim = "partner_code";
        private const string KeyIdClaim = "key_id";
        private const string MemberNbrClaim = "member_nbr";

        public static string GenerateEncryptedToken(TokenRequestDto? tokenRequestDto)
        {
            var token = GenerateToken(tokenRequestDto);

            var key = ConfigurationManager.AppSettings[SymEncryptionKey] ?? string.Empty;
           
            var encryptionHelper = new EncryptionHelper();
            var encryptedToken = encryptionHelper.Encrypt(token, Convert.FromBase64String(key));

            return encryptedToken;
        }


        public static string GenerateToken(TokenRequestDto? tokenRequestDto)
        {
            var claims = new List<Claim>
        {
            new Claim(PartnerCodeClaim, tokenRequestDto?.PartnerCode ?? string.Empty),
            new Claim(KeyIdClaim, tokenRequestDto?.EncKeyId ?? string.Empty),
            new Claim(MemberNbrClaim, tokenRequestDto?.MemberNbr ?? string.Empty)
        };

            var jwtSecret = ConfigurationManager.AppSettings[JwtSecret];
            var jwtIssuer = ConfigurationManager.AppSettings[JwtIssuer];
            int jwtExpirationTimeInMinutes = 0; 
            if (!int.TryParse(ConfigurationManager.AppSettings[JwtExpirationTimeInMinutes], out jwtExpirationTimeInMinutes))
            {
                jwtExpirationTimeInMinutes = 60;
            }
            var token = JwtTokenHelper.GenerateToken(claims, jwtSecret, jwtIssuer, TimeSpan.FromMinutes(jwtExpirationTimeInMinutes));
            return token;
        }

        public static TokenRequestDto GetRequestFromToken(string encryptedToken)
        {
            var tokenClaims = DecryptToken(encryptedToken);
            if (tokenClaims == null)
            {
                return new TokenRequestDto();
            }

            var tokenRequestDto = new TokenRequestDto
            {
                PartnerCode = tokenClaims.GetValueOrDefault(PartnerCodeClaim),
                EncKeyId = tokenClaims.GetValueOrDefault(KeyIdClaim),
                MemberNbr = tokenClaims.GetValueOrDefault(MemberNbrClaim)
            };

            return tokenRequestDto;
        }


        public static Dictionary<string, string> DecryptToken(string encryptedToken)
        {
            // Decrypt the token and extract claims
            if (ValidateAndDecryptToken(encryptedToken, out var tokenClaims))
            {
                return tokenClaims;
            }

            return null;
        }

        public static bool ValidateAndDecryptToken(string encryptedToken, out Dictionary<string, string> tokenClaims)
        {

            var key = ConfigurationManager.AppSettings[SymEncryptionKey] ?? string.Empty;
            var encryptionHelper = new EncryptionHelper();
            var decryptedToken = encryptionHelper.Decrypt(encryptedToken, Convert.FromBase64String(key));
            var jwtSecret = ConfigurationManager.AppSettings[JwtSecret];
            var jwtIssuer = ConfigurationManager.AppSettings[JwtIssuer];
            if (JwtTokenHelper.ValidateAndExtractClaims(decryptedToken, jwtSecret, jwtIssuer, out tokenClaims))
            {
                return true;
            }
            
            return false;
        }

    }
}
