using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.AWSConfig;
using SunnyRewards.Helios.User.Infrastructure.AWSConfig.Interface;
using SunnyRewards.Helios.User.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Infrastructure.Helpers
{
    public class S3Helper :  IS3Helper
    {
        private readonly ILogger<S3Helper> _logger;
        private readonly IAmazonS3ClientService _amazonClientService;
        private readonly IAwsConfiguration _awsConfig;

        const string className = nameof(S3Helper);
        public S3Helper(ILogger<S3Helper> logger, IVault vault, IConfiguration configuration, IAmazonS3ClientService amazonClientService, IAwsConfiguration awsConfiguration) 
        {
            _logger = logger;
            _amazonClientService = amazonClientService;
            _awsConfig = awsConfiguration;
            
        }
        public async Task<bool> UploadFileToS3(Stream streamData, string s3BucketName, string fileName)
        {
            try
            {
                using (var s3Client = _amazonClientService.GetAmazonS3Client(_awsConfig.GetAwsAccessKey().Result, _awsConfig.GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    var result = await s3Client.PutObjectAsync(new PutObjectRequest
                    {
                        BucketName = s3BucketName,
                        Key = fileName,
                        InputStream = streamData,
                        ContentType = "application/pdf",
                    });
                    if (result != null && result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogInformation($"{className}.UploadFileToS3: File uploaded to S3: {fileName}, Bucket Name:{s3BucketName}");
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"{className}.UploadFileToS3: File uploaded to S3 failed for filename: {fileName}, Bucket Name:{s3BucketName}");
                        return false;
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, $"{className}.UploadFileToS3: Error uploading file to S3, File Name: {fileName}, Error Message: {ex.Message}");
                return false;
            }
        }
        public async Task<string> GetHtmlFromS3Async(string key,string bucketName)
        {
            var s3Client =_amazonClientService.GetAmazonS3Client(_awsConfig.GetAwsAccessKey().Result, _awsConfig.GetAwsSecretKey().Result, RegionEndpoint.USEast2)
                ;
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key//  "cms/html/ten-54a775d232e340e78e85f1077fecad40/t_and_c.html"
            };

            using var response = await s3Client.GetObjectAsync(request);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
        }


    }
}
