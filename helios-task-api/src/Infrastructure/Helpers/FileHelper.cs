using Amazon.S3;
using Amazon.S3.Model;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers
{
    public class FileHelper : IFileHelper
    {
        public async System.Threading.Tasks.Task UploadFile(AmazonS3Client s3Client, string bucketName, string? folderName, string fileName, Stream inputStream)
        {

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = folderName != null ? $"{folderName.TrimEnd('/')}/{fileName}" : fileName,
                InputStream = inputStream
            };

            await s3Client.PutObjectAsync(putRequest);

        }
    }
}
