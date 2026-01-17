using Amazon;
using Amazon.S3;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IAmazonS3ClientService
    {
        /// <summary>
        /// Creates and returns an Amazon S3 client.
        /// </summary>
        /// <param name="accessKey">The AWS access key.</param>
        /// <param name="secretKey">The AWS secret key.</param>
        /// <param name="region">The AWS region endpoint.</param>
        /// <returns>An instance of IAmazonS3.</returns>
        IAmazonS3 GetAmazonS3Client(string accessKey, string secretKey, RegionEndpoint region);
    }

}
