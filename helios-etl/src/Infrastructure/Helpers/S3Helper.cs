using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using System.Reflection;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers
{
    public class S3Helper : AwsConfiguration, IS3Helper
    {
        private readonly ILogger<S3Helper> _logger;
        const string className = nameof(S3Helper);
        public S3Helper(ILogger<S3Helper> logger, IVault vault, IConfiguration configuration) : base(vault, configuration)
        {
            _logger = logger;
        }
        public async Task UploadFileToS3(Stream streamData, string s3BucketName, string fileName)
        {
            try
            {
                using (var s3Client = new AmazonS3Client(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    await s3Client.PutObjectAsync(new PutObjectRequest
                    {
                        BucketName = s3BucketName,
                        Key = fileName,
                        InputStream = streamData,
                        ContentType = "text/plain",
                        ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                    });

                    _logger.LogInformation($"{className}.UploadFileToS3: File uploaded to S3: {fileName}, Bucket Name:{s3BucketName}");
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, $"{className}.UploadFileToS3: Error uploading file to S3, File Name: {fileName}, Error Message: {ex.Message}");
                throw;
            }
        }

        public async Task UploadCsvFileToS3<T>(List<T> records, string bucketName, string fileName, string delimiter = "\t")
        {
            // Step 1: Generate CSV content in memory
            var csvContent = new StringBuilder();

            // Step 2: Reflect on object properties
            PropertyInfo[] properties = typeof(T).GetProperties();

            // Step 3: Write CSV header
            csvContent.AppendLine(string.Join(delimiter, properties.Select(p => p.Name)));

            // Step 4: Write CSV rows
            foreach (var record in records)
            {
                var values = properties.Select(p => p.GetValue(record, null)?.ToString() ?? string.Empty);
                csvContent.AppendLine(string.Join(delimiter, values));
            }

            // Convert to byte array
            byte[] csvBytes = Encoding.UTF8.GetBytes(csvContent.ToString());

            // Step 5: Upload to S3
            using var memoryStream = new MemoryStream(csvBytes);
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = fileName,
                InputStream = memoryStream,
                ContentType = "text/csv"
            };

            using var s3Client = new AmazonS3Client(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2);
            await s3Client.PutObjectAsync(putRequest);
        }

        public async Task DownloadFileToLocalFolder(string bucketName, string fileKey, string localFolderPath)
        {
            const string methodName = nameof(DownloadFileToLocalFolder);
            try
            {
                using (var s3Client = new AmazonS3Client(await GetAwsAccessKey(), await GetAwsSecretKey(), RegionEndpoint.USEast2))
                {
                    var getRequest = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = fileKey
                    };

                    var response = await s3Client.GetObjectAsync(getRequest);

                    // Generate a unique file name for the downloaded file
                    string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{fileKey}";
                    string filePath = Path.Combine(localFolderPath, fileName);
                    using (var fileStream = File.Create(filePath))
                    {
                        await response.ResponseStream.CopyToAsync(fileStream);
                    }

                    _logger.LogInformation($"{className}.{methodName}: Downloaded file to local folder successfully, File: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{className}.{methodName}: Error downloading file from S3 bucket: {ex.Message}");
                throw;
            }
        }

        public async Task<CopyObjectResponse> CopyFileToFolder(string sourceBucketName, string sourceKey, string destinationBucketName, string destinationKey)
        {
            const string methodName = nameof(CopyFileToFolder);
            try
            {
                using (var s3Client = new AmazonS3Client(await GetAwsAccessKey(), await GetAwsSecretKey(), RegionEndpoint.USEast2))
                {
                    // Perform copy operation
                    CopyObjectRequest copyRequest = new CopyObjectRequest
                    {
                        SourceBucket = sourceBucketName,
                        SourceKey = sourceKey,
                        DestinationBucket = destinationBucketName,
                        DestinationKey = destinationKey
                    };

                    CopyObjectResponse copyResponse = await s3Client.CopyObjectAsync(copyRequest);

                    _logger.LogInformation($"{className}.{methodName}: Copied file successfully from {sourceKey} to {destinationKey}");
                    return copyResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{className}.{methodName}: Error copying file from {sourceKey} bucket to {destinationKey} bucket: {ex.Message}");
                throw;
            }
        }

        public async Task MoveFileToFolder(string sourceBucketName, string sourceKey, string destinationBucketName, string destinationKey)
        {
            const string methodName = nameof(MoveFileToFolder);
            try
            {
                var copyResponse = await CopyFileToFolder(sourceBucketName, sourceKey, destinationBucketName, destinationKey);
                // Check if copy was successful
                if (copyResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    await DeleteFileFromBucket(sourceBucketName, sourceKey);
                }
                _logger.LogInformation($"{className}.{methodName}: Moved file successfully from {sourceKey} to {destinationKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{className}.{methodName} : Error copying file from {sourceKey} bucket to {destinationKey} bucket: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteFileFromBucket(string bucketName, string fileName)
        {
            const string methodName = nameof(DeleteFileFromBucket);

            // Check for empty or null values in bucketName and fileName
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                _logger.LogError($"{className}.{methodName}: Bucket name cannot be null or empty.");
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogError($"{className}.{methodName}: File name cannot be null or empty.");
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            try
            {
                // Logging before deletion to verify file and bucket being deleted
                _logger.LogInformation($"{className}.{methodName}: Attempting to delete file '{fileName}' from bucket '{bucketName}'.");

                using var s3Client = new AmazonS3Client(await GetAwsAccessKey(), await GetAwsSecretKey(), RegionEndpoint.USEast2);

                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName
                };

                var response = await s3Client.DeleteObjectAsync(deleteObjectRequest);

                // Ensure the response is successful
                if (response.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _logger.LogInformation($"{className}.{methodName}: Successfully deleted file '{fileName}' from bucket '{bucketName}'.");
                }
                else
                {
                    _logger.LogWarning($"{className}.{methodName}: File '{fileName}' deletion returned status code '{response.HttpStatusCode}'.");
                }
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, $"{className}.{methodName}: AWS S3 error when deleting file '{fileName}' from bucket '{bucketName}': {s3Ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.{methodName}: Error deleting file '{fileName}' from S3 bucket: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Uploads a byte array to an S3 bucket by converting it to a stream. Constructs the full S3 path using the folder and file names.
        /// Uses the existing UploadFileToS3 method for the upload.
        /// </summary>
        /// <param name="fileData">The byte array to upload.</param>
        /// <param name="s3BucketName">The target S3 bucket name.</param>
        /// <param name="folderName">The folder within the S3 bucket.</param>
        /// <param name="fileName">The file name to save as in S3.</param>
        /// <returns>A task representing the async upload operation.</returns>
        public async Task UploadByteDataToS3(byte[] fileData, string s3BucketName, string folderName, string fileName)
        {
            using (var memoryStream = new MemoryStream(fileData))
            {
                string s3FileName = $"{folderName}/{fileName}";
                await UploadFileToS3(memoryStream, s3BucketName, s3FileName);
            }
        }
    }

}
