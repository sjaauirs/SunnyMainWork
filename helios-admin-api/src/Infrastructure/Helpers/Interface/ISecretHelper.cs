namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface
{
    public interface ISecretHelper
    {
        /// <summary>
        /// Retrieves the AWS access key from the vault.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the AWS access key.</returns>
        Task<string> GetAwsAccessKey();
        Task<string> GetAwsFireBaseCredentialKey();

        /// <summary>
        /// Retrieves the AWS secret key from the vault.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the AWS secret key.</returns>
        Task<string> GetAwsSecretKey();

        /// <summary>
        /// Retrieves the temporary S3 bucket name from the configuration.
        /// </summary>
        /// <returns>The temporary S3 bucket name.</returns>
        string GetAwsTmpS3BucketName();

        /// <summary>
        /// Retrieves the environment from the vault.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the environment.</returns>
        Task<string> GetEnvironment();

        /// <summary>
        /// Retrieves the export tenant version from the configuration.
        /// </summary>
        /// <returns>The export tenant version.</returns>
        string GetExportTenantVersion();
    }
}
