using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KeyGeneratorApp.Helpers
{
    public static class JwtTokenHelper
    {
        public static string GenerateToken(List<Claim> claims, string jwtSecret, string jwtIssuer, TimeSpan expirationTime)
        {
            var key = Encoding.UTF8.GetBytes(jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(expirationTime),
                Issuer = jwtIssuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public static bool ValidateAndExtractClaims(string jwtToken, string jwtSecret, string jwtIssuer, out Dictionary<string, string> tokenClaims)
        {
            tokenClaims = null;

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Define the key and signing credentials
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Validate the JWT token
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = false, // Adjust as needed
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                tokenHandler.ValidateToken(jwtToken, validationParameters, out validatedToken);

                // Extract token claims
                var jwtTokenClaims = (JwtSecurityToken)validatedToken;
                tokenClaims = new Dictionary<string, string>();

                foreach (var claim in jwtTokenClaims.Claims)
                {
                    tokenClaims.Add(claim.Type, claim.Value);
                }

                return true;
            }
            catch (Exception)
            {
                // Token validation failed
                return false;
            }
        }
    }
}
