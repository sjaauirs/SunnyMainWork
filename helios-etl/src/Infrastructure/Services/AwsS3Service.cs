using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class AwsS3Service : IAwsS3Service
    {
        private readonly IVault _vault;
        private readonly ILogger<AwsQueueService> _logger;
        private readonly IConfiguration _configuration;
        private const string className = nameof(AwsS3Service);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vault"></param>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public AwsS3Service(IVault vault, ILogger<AwsQueueService> logger, IConfiguration configuration)
        {
            _vault = vault;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// It will move file to destination and delete file form source
        /// </summary>
        /// <param name="sourceKey">e.g incoming/pld.txt</param>
        /// <param name="destinationKey">e.g processing/pld.txt</param>
        /// <returns></returns>
        public async Task MoveFileInAwsS3(string sourceKey, string destinationKey, string? sourceBucket = null, string? destinationBucket = null)
        {
            const string methodName = nameof(MoveFileInAwsS3);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing ", className, methodName);
                (string awsAccessKey, string awsSecretKey, string defaultBucket) = await GetAWSSettings();
                var region = RegionEndpoint.USEast2;

                using var client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);

                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = sourceBucket ?? defaultBucket,
                    SourceKey = sourceKey,
                    DestinationBucket = destinationBucket ?? defaultBucket,
                    DestinationKey = destinationKey
                };
                await client.CopyObjectAsync(copyRequest);

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = sourceBucket ?? defaultBucket,
                    Key = sourceKey
                };
                await client.DeleteObjectAsync(deleteRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while moving file into s3 with SourceBucketName:{SName}, Destinaation:{Name}, ErrorCode:{Code},ERROR:{Message}",
                    className, methodName, sourceBucket, destinationBucket, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<(string, string, string)> GetAWSSettings()
        {
            const string methodName = nameof(GetAWSSettings);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing...", className, methodName);

            try
            {
                string awsAccessKey = await _vault.GetSecret(_configuration.GetSection("AWS:AWS_ACCESS_KEY_NAME").Value?.ToString() ?? "");
                string awsSecretKey = await _vault.GetSecret(_configuration.GetSection("AWS:AWS_SECRET_KEY_NAME").Value?.ToString() ?? "");
                string bucketName = _configuration.GetSection("AWS:AWS_BUCKET_NAME").Value?.ToString() ?? "";

                _logger.LogInformation("{ClassName}.{MethodName} - Ended processing...", className, methodName);
                return (awsAccessKey, awsSecretKey, bucketName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured processing GetAWSSettings,ErrorCode:{Code},ERROR:{Msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public async Task<byte[]> GetFileFromAwsS3(string keyName, string? bucketName = null)
        {
            const string methodName = nameof(GetFileFromAwsS3);
            _logger.LogInformation("{ClassName}.{MethodName} - Stared processing for keyName: {KeyName}", className, methodName, keyName);

            try
            {
                (string awsAccessKey, string awsSecretKey, string defaultBucket) = await GetAWSSettings();
                var region = RegionEndpoint.USEast2;
                using var client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);

                var request = new GetObjectRequest
                {
                    BucketName = bucketName ?? defaultBucket,
                    Key = keyName
                };

                using var response = await client.GetObjectAsync(request);
                using var responseStream = response.ResponseStream;
                using var memoryStream = new MemoryStream();
                await responseStream.CopyToAsync(memoryStream);

                _logger.LogInformation("{ClassName}.{MethodName} - Ended processing for keyName: {KeyName}", className, methodName, keyName);

                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error while processing GetFileFromAwsS3 for keyName: {KeyName}", className, methodName, keyName);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public async Task<List<string>> GetAllFileNames(string folderName, string? bucketName = null)
        {
            const string methodName = nameof(GetAllFileNames);
            _logger.LogInformation("{ClassName}.{MethodName} - Stared processing for FolderName: {Name}", className, methodName, folderName);
            try
            {
                (string awsAccessKey, string awsSecretKey, string defaultBucketName) = await GetAWSSettings();
                bucketName = bucketName ?? defaultBucketName;
                RegionEndpoint region = RegionEndpoint.USEast2;
                List<S3Object> s3Objects = new();
                using (var client = new AmazonS3Client(awsAccessKey, awsSecretKey, region))
                {
                    var request = new ListObjectsV2Request
                    {
                        BucketName = bucketName,
                        Prefix = folderName + "/",
                    };

                    ListObjectsV2Response response;

                    do
                    {
                        response = await client.ListObjectsV2Async(request);

                        s3Objects.AddRange(response.S3Objects);

                        request.ContinuationToken = response.NextContinuationToken;
                    } while (response.IsTruncated);
                }

                // Sort the files by LastModified date (FIFO order)
                var sortedS3Objects = s3Objects.OrderBy(o => o.LastModified).ToList();

                // Extract file names from sorted S3Objects
                List<string> fileNames = sortedS3Objects
                    .Select(s3Object => Path.GetFileName(s3Object.Key))
                    .Where(fileName => !string.IsNullOrEmpty(fileName))
                    .ToList();

                _logger.LogInformation("{ClassName}.{MethodName} - Ended processing for FolderName: {Name}", className, methodName, folderName);
                return fileNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Failed processing  GetAllFileNames for FolderName: {Name},ErrorCode:{Code},ERROR:{Msg}",
                    className, methodName, folderName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<bool> CreateFile(string fileName)
        {
            const string methodName = nameof(CreateFile);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Stared processing for Filename: {Name}", className, methodName, fileName);
                (string awsAccessKey, string awsSecretKey, string bucketName) = await GetAWSSettings();

                var region = RegionEndpoint.USEast2;

                using var client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);

                var emptyStream = new System.IO.MemoryStream(); // Create an empty stream

                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    InputStream = emptyStream,
                };

                var response = await client.PutObjectAsync(request);
                return true;
                _logger.LogInformation("{ClassName}.{MethodName} - Ended processing for Filename: {Name}", className, methodName, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Failed processing Create file for Filename: {Name},ErrorCode:{Code},ERROR:{Msg}", className, methodName, fileName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task AppendInFile(string keyName, string contentJson)
        {
            const string methodName = nameof(AppendInFile);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Stared processing for keyName: {KeyName}", className, methodName, keyName);
                (string awsAccessKey, string awsSecretKey, string bucketName) = await GetAWSSettings();


                // Initialize the S3 client
                using var s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.USEast2);

                // Read the existing object
                var getObjectRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                using var getObjectResponse = await s3Client.GetObjectAsync(getObjectRequest);
                using var streamReader = new StreamReader(getObjectResponse.ResponseStream);
                var existingContent = await streamReader.ReadToEndAsync();

                // Append new data
                var updatedContent = existingContent + contentJson;

                // Upload the updated content
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    ContentBody = updatedContent
                };

                await s3Client.PutObjectAsync(putObjectRequest);
                _logger.LogInformation("{ClassName}.{MethodName} - Ended processing for keyName: {KeyName}", className, methodName, keyName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Failed processing append in file for keyName: {KeyName}", className, methodName, keyName);
                throw;
            }
        }


        /// <summary>
        /// Move file form processing to archive folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task MoveFileFromProcessingToArchive(string fileName)
        {
            const string methodName = nameof(MoveFileFromProcessingToArchive);
            try
            {
                await MoveFileInAwsS3($"{Constants.PROCESSING_FOLDER}/{fileName}", $"{Constants.ARCHIVE_FOLDER}/{fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className} {methodName}: Error occurred while moving file from processing to Archive. FileName: {fileName}");
                throw;
            }
        }

        /// <summary>
        /// Read consumer codes from S3/local file
        /// </summary>
        /// <param name="s3File"></param>
        /// <returns></returns>
        public async Task<List<string>> GetConsumerListFromFile(string consumerListFile)
        {
            const string methodName = nameof(GetConsumerListFromFile);
            try
            {
                var consumerCodesList = new List<string>();

                if (!string.IsNullOrEmpty(consumerListFile))
                {
                    var isPathFullyQualified = Path.IsPathFullyQualified(consumerListFile);
                    byte[]? s3ConsumerCodesFileContent = null;

                    if (!isPathFullyQualified)
                    {
                        s3ConsumerCodesFileContent = await GetFileFromAwsS3(
                            $"{Constants.INCOMING_FOLDER}/{consumerListFile}" ?? "");
                    }

                    using (StreamReader reader = s3ConsumerCodesFileContent?.Length > 0
                        ? new StreamReader(new MemoryStream(s3ConsumerCodesFileContent))
                        : new StreamReader(consumerListFile))
                    {
                        string consumerCode;
                        int recordNbr = -1;

                        while ((consumerCode = await reader.ReadLineAsync()) != null)
                        {
                            consumerCode = consumerCode.Trim();
                            if (recordNbr == -1 && consumerCode != "consumer_code")
                            {
                                _logger.LogError($"{className} {methodName}: Failed processing consumer codes file with invalid header: {consumerCode}");
                                throw new ETLException(ETLExceptionCodes.InValidValue, $"Failed processing consumer codes file with invalid header: {consumerCode}");
                            }
                            else if (recordNbr != -1)
                                consumerCodesList.Add(consumerCode);

                            recordNbr++;
                        }
                    }

                    if (consumerCodesList.Count == 0)
                    {
                        _logger.LogError($"{className} {methodName}: Consumer codes file has no data. FileName: {consumerListFile}");
                        throw new ETLException(ETLExceptionCodes.InValidValue, $"Consumer codes file has no data. FileName: {consumerListFile}");
                    }
                    if (!isPathFullyQualified)
                    {
                        await MoveFileInAwsS3($"{Constants.INCOMING_FOLDER}/{consumerListFile}", $"{Constants.PROCESSING_FOLDER}/{consumerListFile}");
                    }
                    _logger.LogInformation($"{className} {methodName}: Consumer list from the file {consumerListFile}. Consumer list: {consumerCodesList.ToJson()}");
                }

                return consumerCodesList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className} {methodName}: Error occurred while reading consumer list from file. FileName: {consumerListFile}");
                throw;
            }
        }

        /// <summary>
        /// Read consumer codes from S3/local file
        /// </summary>
        /// <param name="s3File"></param>
        /// <returns></returns>
        public async Task<List<object>?> GetConsumerListFromFileForCard60(string consumerListFile)
        {
            const string methodName = nameof(GetConsumerListFromFileForCard60);
            try
            {
                List<object>? consumersList = null;
                var consumerCodesList = new List<object>();

                if (!string.IsNullOrEmpty(consumerListFile))
                {
                    var isPathFullyQualified = Path.IsPathFullyQualified(consumerListFile);
                    byte[]? s3ConsumerCodesFileContent = null;

                    if (!isPathFullyQualified)
                    {
                        s3ConsumerCodesFileContent = await GetFileFromAwsS3(
                            $"{Constants.INCOMING_FOLDER}/{consumerListFile}" ?? "");
                    }

                    Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                    using (StreamReader reader = s3ConsumerCodesFileContent?.Length > 0
                        ? new StreamReader(new MemoryStream(s3ConsumerCodesFileContent), utf8WithoutBom)
                        : new StreamReader(consumerListFile, utf8WithoutBom))
                    {
                        // Peek header line to determine structure
                        string? headerLine = reader.ReadLine();
                        if (headerLine == null)
                        {
                            throw new ETLException(ETLExceptionCodes.InValidValue, $"File is empty. FileName: {consumerListFile}");
                        }

                        int columnCount = headerLine.Split('\t').Length;

                        // Reset stream for actual processing
                        reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        reader.DiscardBufferedData();

                        if (columnCount == 1)
                        {
                            object consumerCode;
                            int recordNbr = -1;

                            while ((consumerCode = await reader.ReadLineAsync()) != null)
                            {
                                if (recordNbr == -1 && !consumerCode.Equals("consumer_code"))
                                {
                                    _logger.LogError($"{className} {methodName}: Failed processing consumer codes file with invalid header: {consumerCode}");
                                    throw new ETLException(ETLExceptionCodes.InValidValue, $"Failed processing consumer codes file with invalid header: {consumerCode}");
                                }
                                else if (recordNbr != -1)
                                    consumerCodesList.Add(consumerCode);

                                recordNbr++;
                            }
                            consumersList = consumerCodesList;
                        }
                        else if (columnCount == 4)
                        {
                            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                            {
                                Delimiter = "\t",
                                HasHeaderRecord = true,
                                TrimOptions = TrimOptions.Trim,
                                IgnoreBlankLines = true,
                                PrepareHeaderForMatch = args => args.Header.Trim('\uFEFF', ' ', '\t').ToLowerInvariant(),
                                HeaderValidated = null, // Ignore header validation errors
                                MissingFieldFound = null, // Ignore missing field errors
                            };
                            using (var csv = new CsvReader(reader, csvConfig))
                            {
                                var records = csv.GetRecords<ETLCard60ConsumerInputDto>().Cast<object>().ToList();
                                if (records.Count == 0)
                                {
                                    _logger.LogError($"{className} {methodName}: Consumer codes file has no data. FileName: {consumerListFile}");
                                    throw new ETLException(ETLExceptionCodes.InValidValue, $"Consumer codes file has no data. FileName: {consumerListFile}");
                                }
                                consumersList = records;
                            }
                        }
                        else
                        {
                            _logger.LogError($"{className} {methodName}: Unrecognized CSV format. Column count: {columnCount}. FileName: {consumerListFile}");
                            throw new ETLException(ETLExceptionCodes.InValidValue, $"Invalid file format. Column count: {columnCount}. FileName: {consumerListFile}");
                        }
                    }
                    if (!isPathFullyQualified)
                    {
                        await MoveFileInAwsS3($"{Constants.INCOMING_FOLDER}/{consumerListFile}", $"{Constants.PROCESSING_FOLDER}/{consumerListFile}");
                    }
                    _logger.LogInformation($"{className} {methodName}: Consumer list from the file {consumerListFile}. Consumer list: {consumersList.ToJson()}");
                }

                return consumersList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className} {methodName}: Error occurred while reading consumer list from file. FileName: {consumerListFile}");
                throw;
            }
        }
        public async Task<bool> CreateCsvAndUploadToS3<T>(CsvConfiguration csvConfig, List<T> records, string fileName, string bucketName)
        {
            try
            {
                // Create a MemoryStream to hold the CSV data
                using (var memoryStream = new MemoryStream())
                {
                    // Use StreamWriter and CsvWriter to generate CSV
                    using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, leaveOpen: true))
                    using (var csvWriter = new CsvWriter(writer, csvConfig))
                    {
                        await csvWriter.WriteRecordsAsync(records);
                        await writer.FlushAsync();
                    }

                    // Reset the position of the MemoryStream to the beginning
                    memoryStream.Position = 0;

                    // Retrieve AWS credentials and bucket name
                    (string awsAccessKey, string awsSecretKey, string defaultBucketName) = await GetAWSSettings();
                    bucketName = bucketName ?? defaultBucketName;

                    // Initialize AWS region and client
                    RegionEndpoint region = RegionEndpoint.USEast2; // You can change this as needed
                    using (var client = new AmazonS3Client(awsAccessKey, awsSecretKey, region))
                    {
                        // Create transfer utility and upload request
                        var transferUtility = new TransferUtility(client);
                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            InputStream = memoryStream,
                            BucketName = bucketName,
                            Key = fileName,
                            ContentType = fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ? "text/plain" : "text/csv",
                            CannedACL = S3CannedACL.BucketOwnerFullControl // Set ACL to 'bucket-owner-full-control'
                        };

                        // Perform async upload
                        await transferUtility.UploadAsync(uploadRequest);
                        _logger.LogInformation($"Successfully uploaded CSV to S3 with fileName: {fileName}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading CSV to S3: {ex.Message}");
                return false;
            }
        }

        public async Task<Stream> DownloadFile(string bucketName, string fileKey)
        {
            try
            {
                (string awsAccessKey, string awsSecretKey, string defaultBucket) = await GetAWSSettings();
                using var s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.USEast2);
                var getRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileKey
                };

                var response = await s3Client.GetObjectAsync(getRequest);
                return response.ResponseStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.DownloadFile: ERROR downloading file from S3 bucket: {ex.Message}, Error Code:{StatusCodes.Status500InternalServerError}");
                throw;
            }

        }

        public async Task UploadStreamToS3(Stream inputStream, string bucketName, string key)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));

            if (inputStream.CanSeek)
                inputStream.Seek(0, SeekOrigin.Begin);

            (string awsAccessKey, string awsSecretKey, _) = await GetAWSSettings();
            var region = RegionEndpoint.USEast2;

            var tempFilePath = Path.GetTempFileName();

            try
            {
                // Write stream to disk first
                await using (var fileStream = File.Create(tempFilePath))
                {
                    await inputStream.CopyToAsync(fileStream);
                }

                using var s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);
                var transferUtility = new TransferUtility(s3Client);

                var stopwatch = Stopwatch.StartNew();

                await transferUtility.UploadAsync(tempFilePath, bucketName, key);

                stopwatch.Stop();
                _logger.LogInformation($"Uploaded file to S3: {key}, Took: {stopwatch.Elapsed.TotalSeconds:N2} sec");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to upload {key} to S3");
                throw;
            }
            finally
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"⚠️ Failed to delete temp file: {tempFilePath}");
                }
            }
        }

        public async Task UploadFileToS3(string filePath, string bucketName, string key)
        {
            const string methodName = nameof(UploadFileToS3);

            try
            {
                (string awsAccessKey, string awsSecretKey, _) = await GetAWSSettings();
                var region = RegionEndpoint.USEast2;

                using var s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);

                _logger.LogInformation("{Class}.{Method}: Uploading file '{FilePath}' to bucket '{Bucket}' with key '{Key}'",
                    className, methodName, filePath, bucketName, key);

                await using var fileStream = File.OpenRead(filePath);

                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = fileStream
                };

                var response = await s3Client.PutObjectAsync(request);

                _logger.LogInformation("{Class}.{Method}: Successfully uploaded file '{FilePath}' to '{Bucket}/{Key}' with status '{HttpStatus}'",
                    className, methodName, filePath, bucketName, key, response.HttpStatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}: ERROR uploading file to S3. File: '{FilePath}', Bucket: '{Bucket}', Key: '{Key}', Error: {ErrorMessage}, Status Code: {StatusCode}",
                    className, methodName, filePath, bucketName, key, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }


        public async Task DeleteFile(string bucketName, string key)
        {
            if (string.IsNullOrEmpty(bucketName))
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            (string awsAccessKey, string awsSecretKey, string _) = await GetAWSSettings();
            var region = RegionEndpoint.USEast2;

            using var s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            try
            {
                await s3Client.DeleteObjectAsync(deleteRequest);
                _logger.LogInformation($"Deleted object from S3: Bucket={bucketName}, Key={key}");
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, $"AWS S3 error while deleting object. Bucket={bucketName}, Key={key}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"General error while deleting object from S3. Bucket={bucketName}, Key={key}");
                throw;
            }
        }

        public async Task<GetObjectMetadataResponse> GetFileMetadata(string bucketName, string key)
        {
            if (string.IsNullOrEmpty(bucketName))
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            (string awsAccessKey, string awsSecretKey, string _) = await GetAWSSettings();
            var region = RegionEndpoint.USEast2;

            using var s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);

            var metadataRequest = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = key
            };

            try
            {
                var metadataResponse = await s3Client.GetObjectMetadataAsync(metadataRequest);

                _logger.LogInformation($"Retrieved metadata for S3 object: Bucket={bucketName}, Key={key}");

                return metadataResponse;
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, $"AWS S3 error while getting metadata. Bucket={bucketName}, Key={key}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"General error while getting metadata from S3. Bucket={bucketName}, Key={key}");
                throw;
            }
        }

        /// <summary>
        /// Read cohort codes from S3/local file
        /// </summary>
        /// <param name="cohortListFile"></param>
        /// <returns></returns>
        /// <exception cref="ETLException"></exception>
        public async Task<List<string>> GetCohortListFromFile(string cohortListFile)
        {
            const string methodName = nameof(GetCohortListFromFile);
            try
            {
                var cohortCodesList = new List<string>();

                if (!string.IsNullOrEmpty(cohortListFile))
                {
                    var isPathFullyQualified = Path.IsPathFullyQualified(cohortListFile);
                    byte[]? s3CohortCodesFileContent = null;

                    if (!isPathFullyQualified)
                    {
                        s3CohortCodesFileContent = await GetFileFromAwsS3(
                            $"{Constants.INCOMING_FOLDER}/{cohortListFile}" ?? "");
                    }

                    using (StreamReader reader = s3CohortCodesFileContent?.Length > 0
                        ? new StreamReader(new MemoryStream(s3CohortCodesFileContent))
                        : new StreamReader(cohortListFile))
                    {
                        string cohortCode;
                        int recordNbr = -1;

                        while ((cohortCode = await reader.ReadLineAsync()) != null)
                        {
                            cohortCode = cohortCode.Trim();
                            if (recordNbr == -1 && cohortCode != "cohort_code")
                            {
                                _logger.LogError($"{className} {methodName}: Failed processing cohort codes file with invalid header: {cohortCode}");
                                throw new ETLException(ETLExceptionCodes.InValidValue, $"Failed processing cohort codes file with invalid header: {cohortCode}");
                            }
                            else if (recordNbr != -1)
                                cohortCodesList.Add(cohortCode);

                            recordNbr++;
                        }
                    }

                    if (cohortCodesList.Count == 0)
                    {
                        _logger.LogError($"{className} {methodName}: cohort codes file has no data. FileName: {cohortListFile}");
                        throw new ETLException(ETLExceptionCodes.InValidValue, $"cohort codes file has no data. FileName: {cohortListFile}");
                    }
                    if (!isPathFullyQualified)
                    {
                        await MoveFileInAwsS3($"{Constants.INCOMING_FOLDER}/{cohortListFile}", $"{Constants.PROCESSING_FOLDER}/{cohortListFile}");
                    }
                    _logger.LogInformation($"{className} {methodName}: cohort list from the file {cohortListFile}. cohort list: {cohortCodesList.ToJson()}");
                }

                return cohortCodesList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className} {methodName}: Error occurred while reading cohort list from file. FileName: {cohortListFile}");
                throw;
            }
        }
        public async Task UploadImageToS3Async(byte[] imageBytes, string bucketName, string key)
        {
            const string methodName = nameof(UploadImageToS3Async);
            try
            {
                (string awsAccessKey, string awsSecretKey, _) = await GetAWSSettings();
                var region = RegionEndpoint.USEast2;

                using var s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, region);

                _logger.LogInformation("{Class}.{Method}: Uploading image to bucket '{Bucket}' with key '{Key}'",
                    className, methodName, bucketName, key);
                // ✅ Step 4: Upload to S3
                using (var stream = new MemoryStream(imageBytes))
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        InputStream = stream,
                        ContentType = "image/jpeg" // You can detect type dynamically if needed
                    };

                    var response = await s3Client.PutObjectAsync(request);


                    _logger.LogInformation("{Class}.{Method}: Successfully uploaded image to '{Bucket}/{Key}' with status '{HttpStatus}'",
                              className, methodName, bucketName, key, response.HttpStatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}: ERROR uploading file to S3. image to Bucket: '{Bucket}', Key: '{Key}', Error: {ErrorMessage}, Status Code: {StatusCode}",
                    className, methodName, bucketName, key, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }

       

    }
}
