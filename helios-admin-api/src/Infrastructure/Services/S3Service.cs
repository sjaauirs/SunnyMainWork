using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using System.IO.Compression;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class S3Service : IS3Service
    {
        private readonly ILogger<S3Service> _logger;
        private readonly ISecretHelper _secretHelper;
        private readonly IAmazonS3ClientService _amazonClientService;
        const string _className = nameof(S3Service);

        public S3Service(ILogger<S3Service> logger, ISecretHelper secretHelper, IAmazonS3ClientService amazonClientService)
        {
            _logger = logger;
            _secretHelper = secretHelper;
            _amazonClientService = amazonClientService;
        }

        /// <summary>
        /// Deletes the specified folder and its contents from the S3 bucket.
        /// </summary>
        /// <param name="bucketName">The name of the S3 bucket.</param>
        /// <param name="folderName">The name of the folder to be deleted.</param>
        public async System.Threading.Tasks.Task DeleteFolder(string bucketName, string folderName)
        {
            const string methodName = nameof(DeleteFolder);
            _logger.LogInformation("{ClassName}.{MethodName} started for bucket: {BucketName}, folder: {FolderName}", _className, methodName, bucketName, folderName);

            try
            {
                using var s3Client = _amazonClientService.GetAmazonS3Client(_secretHelper.GetAwsAccessKey().Result, _secretHelper.GetAwsSecretKey().Result, RegionEndpoint.USEast2);

                var listObjectsRequest = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = folderName
                };

                ListObjectsV2Response listObjectsResponse;
                do
                {
                    listObjectsResponse = await s3Client.ListObjectsV2Async(listObjectsRequest);
                    _logger.LogInformation("{ClassName}.{MethodName} - Retrieved {ObjectCount} objects for deletion.", _className, methodName, listObjectsResponse.S3Objects.Count);

                    foreach (S3Object obj in listObjectsResponse.S3Objects)
                    {
                        var deleteObjectRequest = new DeleteObjectRequest
                        {
                            BucketName = bucketName,
                            Key = obj.Key
                        };
                        await s3Client.DeleteObjectAsync(deleteObjectRequest);
                        _logger.LogInformation("{ClassName}.{MethodName} - Deleted object: {ObjectKey}", _className, methodName, obj.Key);
                    }

                    listObjectsRequest.ContinuationToken = listObjectsResponse.NextContinuationToken;
                } while (listObjectsResponse.IsTruncated);

                _logger.LogInformation("{ClassName}.{MethodName}: Folder '{FolderName}' deleted successfully.", _className, methodName, folderName);

            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, "{ClassName}.{MethodName}: Error encountered on server. Message: '{ErrorMessage}' when deleting folder", _className, methodName, s3Ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Unknown error encountered. Message: '{ErrorMessage}' when deleting folder", _className, methodName, ex.Message);
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} completed for bucket: {BucketName}, folder: {FolderName}", _className, methodName, bucketName, folderName);
            }
        }

        /// <summary>
        /// Gets the content of the file.
        /// </summary>
        /// <param name="s3BucketName">Name of the s3 bucket.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public async Task<byte[]> GetFileContent(string s3BucketName, string filePath)
        {
            try
            {
                using var s3Client = _amazonClientService.GetAmazonS3Client(_secretHelper.GetAwsAccessKey().Result, _secretHelper.GetAwsSecretKey().Result, RegionEndpoint.USEast2);

                var request = new GetObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = filePath
                };
                using var response = await s3Client.GetObjectAsync(request);
                using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetFileContent: Error downloading file from S3 bucket, Bucket Name: {s3BucketName}, File Path {filePath}, Error Message: {ex.Message}");
                throw;
            }

        }

        /// <summary>
        /// Gets the list of files in folder.
        /// </summary>
        /// <param name="s3BucketName">Name of the s3 bucket.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetListOfFilesInFolder(string s3BucketName, string folderPath)
        {
            try
            {
                using var s3Client = _amazonClientService.GetAmazonS3Client(_secretHelper.GetAwsAccessKey().Result, _secretHelper.GetAwsSecretKey().Result, RegionEndpoint.USEast2);

                var request = new ListObjectsV2Request
                {
                    BucketName = s3BucketName,
                    Prefix = folderPath
                };
                var response = await s3Client.ListObjectsV2Async(request);
                return response.S3Objects.Select(o => o.Key).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetListOfFilesInFolder: Error while fetch list of files from S3 bucket, Bucket Name: {s3BucketName}, Folder Path: {folderPath}, Error Message: {ex.Message}");
                throw;
            }

        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="s3BucketName">Name of the s3 bucket.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        public async System.Threading.Tasks.Task UploadFile(string s3BucketName, string fileName, string content, string contentType)
        {
            try
            {
                using var s3Client = _amazonClientService.GetAmazonS3Client(_secretHelper.GetAwsAccessKey().Result, _secretHelper.GetAwsSecretKey().Result, RegionEndpoint.USEast2);

                await s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = fileName,
                    ContentBody = content,
                    ContentType = contentType,
                });

                _logger.LogInformation($"UploadFileToS3: Card create file uploaded to S3: {fileName}");

            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, $"UploadFileToS3: Error uploading file to S3, File Name: {fileName}, Error Message: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Downloads a zip file from the specified S3 bucket.
        /// </summary>
        /// <param name="bucketName">The name of the S3 bucket.</param>
        /// <param name="zipFileName">The name of the zip file to be downloaded.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the zip file as a MemoryStream.
        /// </returns>
        public async Task<MemoryStream> DownloadZipFile(string bucketName, string zipFileName)
        {
            try
            {
                using var s3Client = _amazonClientService.GetAmazonS3Client(_secretHelper.GetAwsAccessKey().Result, _secretHelper.GetAwsSecretKey().Result, RegionEndpoint.USEast2);

                var getObjectRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = zipFileName
                };

                using (var getObjectResponse = await s3Client.GetObjectAsync(getObjectRequest))
                using (var responseStream = getObjectResponse.ResponseStream)
                {
                    var memoryStream = new MemoryStream();
                    await responseStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    return memoryStream;
                }

            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, "Error encountered on server. Message:'{ErrorMessage}' when downloading zip file", s3Ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown error encountered. Message:'{ErrorMessage}' when downloading zip file", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Zips the folder and upload.
        /// </summary>
        /// <param name="s3BucketName">Name of the s3 bucket.</param>
        /// <param name="s3FolderPrefix">The s3 folder prefix.</param>
        /// <param name="zipFileName">Name of the zip file.</param>
        /// <returns></returns>
        public async Task<bool> ZipFolderAndUpload(string s3BucketName, string s3FolderPrefix, string zipFileName)
        {
            try
            {
                using var s3Client = _amazonClientService.GetAmazonS3Client(_secretHelper.GetAwsAccessKey().Result, _secretHelper.GetAwsSecretKey().Result, RegionEndpoint.USEast2);

                _logger.LogInformation($"Starting the zip process for folder '{s3FolderPrefix}' in bucket '{s3BucketName}'.");

                using (var zipStream = new MemoryStream())
                {
                    // Create zip archive in memory
                    using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        _logger.LogInformation($"Fetching objects from S3 folder '{s3FolderPrefix}'.");

                        var listRequest = new ListObjectsV2Request
                        {
                            BucketName = s3BucketName,
                            Prefix = s3FolderPrefix
                        };

                        var listResponse = await s3Client.ListObjectsV2Async(listRequest);

                        if (listResponse.S3Objects.Count == 0)
                        {
                            _logger.LogWarning($"No objects found in S3 folder '{s3FolderPrefix}'.");
                            return false;
                        }

                        // Loop through each file in the "folder"
                        foreach (var s3Object in listResponse.S3Objects)
                        {
                            _logger.LogInformation($"Adding '{s3Object.Key}' to the zip archive.");

                            var getRequest = new GetObjectRequest
                            {
                                BucketName = s3BucketName,
                                Key = s3Object.Key
                            };

                            using (var response = await s3Client.GetObjectAsync(getRequest))
                            using (var entryStream = response.ResponseStream)
                            {
                                var zipEntry = zipArchive.CreateEntry(s3Object.Key.Replace(s3FolderPrefix, "").TrimStart('/'));

                                using (var zipEntryStream = zipEntry.Open())
                                {
                                    await entryStream.CopyToAsync(zipEntryStream);
                                }

                                _logger.LogInformation($"Successfully added '{s3Object.Key}' to the zip.");
                            }
                        }
                    }

                    // Reset stream position before uploading
                    zipStream.Seek(0, SeekOrigin.Begin);

                    _logger.LogInformation($"Uploading zip file '{zipFileName}' to S3 bucket '{s3BucketName}'.");

                    var putRequest = new PutObjectRequest
                    {
                        BucketName = s3BucketName,
                        Key = zipFileName,
                        InputStream = zipStream,
                        ContentType = "application/zip"
                    };

                    await s3Client.PutObjectAsync(putRequest);

                    _logger.LogInformation($"Successfully uploaded zip file '{zipFileName}' to S3.");

                    return true;

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while zipping and uploading folder '{s3FolderPrefix}': {ex.Message}", ex);
                return false;
            }
        }

    }
}
