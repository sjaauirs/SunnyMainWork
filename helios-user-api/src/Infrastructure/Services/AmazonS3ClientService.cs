using Amazon;
using Amazon.S3;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    [ExcludeFromCodeCoverage]
    public class AmazonS3ClientService : IAmazonS3ClientService
    {
        /// <summary>
        /// Creates and returns an Amazon S3 client.
        /// </summary>
        /// <param name="accessKey">The AWS access key.</param>
        /// <param name="secretKey">The AWS secret key.</param>
        /// <param name="region">The AWS region endpoint.</param>
        /// <returns>An instance of IAmazonS3.</returns>
        public IAmazonS3 GetAmazonS3Client(string accessKey, string secretKey, RegionEndpoint region)
        {
            return new AmazonS3Client(accessKey, secretKey, region);
        }

     
    }
}
