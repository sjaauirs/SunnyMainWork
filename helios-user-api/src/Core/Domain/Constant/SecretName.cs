namespace SunnyRewards.Helios.User.Core.Domain.Constant
{
    public static class SecretName
    {
        public const string SymmetricEncryptionKey = "SYM_ENCRYPTION_KEY";
        public const string CustomerJwtValidationKey = "CUSTOMER_JWT_VALIDATION_KEY";
        public const string TokenIssuer = "TOKEN_ISSUER";
        public const string Env = "env";
        public const string JwtSecretKey = "JWT_SECRET_KEY";
        public const string AdminJwtSecretKey = "ADMIN_JWT_SECRET_KEY";
    }
}
