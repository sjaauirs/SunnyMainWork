using Amazon;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NHibernate.Util;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Text;
using ISecretHelper = SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces.ISecretHelper;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class RetailProductSyncService : AwsConfiguration, IRetailProductSyncService
    {
        private readonly ILogger<RetailProductSyncService> _logger;
        private const string FIS_OUTBOUND_FOLDER = "FIS/Costco/outbound";
        private const string FIS_INBOUND_FOLDER = "Costco/inbound";
        private readonly IPgpS3FileEncryptionHelper _s3FileEncryptionHelper;
        private readonly ITenantRepo _tenantRepo;
        private readonly IVault _vault;
        private readonly IConfiguration _configuration;
        private readonly IBatchOperationService _batchOperationService;
        private const string className=nameof(RetailProductSyncService);
        private const string Action_Delete = "D";
        private readonly IDynamoDbHelper _dynamoDbHelper;
        private readonly ISecretHelper _secretHelper;
        private readonly IRetailClient _retailClient;

        public RetailProductSyncService(IVault vault, ILogger<RetailProductSyncService> logger, IConfiguration configuration,
            IPgpS3FileEncryptionHelper s3FileEncryptionHelper, ITenantRepo tenantRepo, IBatchOperationService batchOperationService, IDynamoDbHelper dynamoDbHelper,
            ISecretHelper secretHelper, IRetailClient retailClient)
            : base(vault, configuration)
        {
            _logger = logger;
            _s3FileEncryptionHelper = s3FileEncryptionHelper;
            _tenantRepo = tenantRepo;
            _vault = vault;
            _batchOperationService = batchOperationService;
            _configuration = configuration;
            _dynamoDbHelper = dynamoDbHelper;
            _secretHelper = secretHelper;
            _retailClient = retailClient;
        }

        public async Task DecryptAndSaveToLocalPath(EtlExecutionContext etlExecutionContext, string localFolderPath)
        {
            const string methodName = nameof(DecryptAndSaveToLocalPath);
            try
            {
                var tenantCode = etlExecutionContext.TenantCode;
                var tmpS3BucketName = GetAwsTmpS3BucketName();
                var fisFtpS3BucketName = GetAwsFisSftpS3BucketName();
                var fileName = "";
                var sunnyPrivateKeyBase64 = GetTenantSecret(tenantCode, GetSunnyAplPrivateKeyName()).Result;
                if (string.IsNullOrEmpty(sunnyPrivateKeyBase64) || sunnyPrivateKeyBase64 == _vault.InvalidSecret)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Sunny Private key is not configured for Tenant: {TenantCode}", className, methodName,tenantCode);
                    return;
                }
                var passPhraseKey = GetTenantSecret(tenantCode, GetSunnyPrivateKeyPassPhraseKeyName()).Result;
                if (string.IsNullOrEmpty(passPhraseKey) || passPhraseKey == _vault.InvalidSecret)
                {
                    _logger.LogError("{ClassName}.{MethodName} - PassPhrase key is not configured for Tenant: {TenantCode}", className, methodName,tenantCode);
                    return;
                }
                await _s3FileEncryptionHelper.DecryptAndSaveToLocalPath(fisFtpS3BucketName, FIS_OUTBOUND_FOLDER, fileName,
                        sunnyPrivateKeyBase64, passPhraseKey, localFolderPath);

                // Testing download and decryption
                var secureFileTransferRequestDto = new SecureFileTransferRequestDto
                {
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    SourceBucketName = GetAwsFisSftpS3BucketName(),
                    SourceFileName = "",
                    SourceFolderName = FIS_INBOUND_FOLDER,
                    FisAplPublicKeyName = GetFisAplPublicKeyName(),
                    SunnyAplPrivateKeyName = GetSunnyAplPrivateKeyName(),
                    SunnyAplPublicKeyName = GetSunnyAplPublicKeyName(),
                    TargetBucketName = GetAwsFisSftpS3BucketName(),
                    ArchiveBucketName = GetAwsSunnyArchiveFileBucketName(),
                    TargetFileName = "",
                    DeleteFileAfterCopy = true,
                    PassPhraseKeyName = GetSunnyPrivateKeyPassPhraseKeyName()
                };

                // Download and decrypt the file
                var response = await _s3FileEncryptionHelper.DownloadAndDecryptFile(secureFileTransferRequestDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error decrypting and saving to local path,ErrorCode:{Code},ERROR:{Msg}",className,methodName,StatusCodes.Status500InternalServerError,ex.Message);
                throw;
            }
        }


        /// <summary>
        /// Processes messages from the Retail product AWS SQS queue and generate FIS APL file.
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task ProcessQueueMessages(EtlExecutionContext etlExecutionContext)
        {
            const string methodName=nameof(ProcessQueueMessages);
            var records = new List<FisRecordDto>();
            var processedMessagesList = new List<Message>();
            var chunkCounter = 0;
            var fileName = $"SUNNY_UPC_FILE_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            var messageChunkSize = GetMessageChunkSize();
            var maxNumberOfMessages = 10;
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Starting to process queue messages...", className, methodName);
              
                await _batchOperationService.ValidateTenant(etlExecutionContext.TenantCode);
                _batchOperationService.ValidateBatchOperationGroupCode(etlExecutionContext);

                string queueUrl = await GetAwsRetailProductSyncQueueUrl();

                var processedMessages = new List<Message>();
                using (var sqsClient = new AmazonSQSClient(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    while (true)
                    {
                        var messagesChunk = await ReceiveMessagesChunk(sqsClient, queueUrl, maxNumberOfMessages, messageChunkSize, methodName);
                        if (!messagesChunk.Any()) break;

                        var fisRecords = ConvertMessagesToFisRecords(messagesChunk, processedMessagesList);
                        if (fisRecords.Any())
                        {
                            await WriteFisRecordsToCsvChunked(fisRecords, fileName, chunkCounter == 0, etlExecutionContext.BatchOperationGroupCode);
                            chunkCounter++;
                            _logger.LogInformation("{ClassName}.{MethodName} - Fis records written to CSV file.", className, methodName);
                            await DeleteMessages(sqsClient, queueUrl, processedMessagesList);
                            _logger.LogInformation("{ClassName}.{MethodName} - Processed and deleted messages from the queue.", className, methodName);
                        }
                        else
                        {
                            _logger.LogInformation("{ClassName}.{MethodName} - No records to write to CSV file.", className, methodName);
                        }
                        
                    }
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Finished processing queue messages.", className, methodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing queue messages, ErrorCode:{Code},ERROR: {Message}", className, methodName, StatusCodes.Status500InternalServerError,ex.Message);
                _logger.LogError("Unhandled exception occurred. Unable to process the following messages:");

                foreach (var message in processedMessagesList)
                {
                    _logger.LogError("{ClassName}.{MethodName} -Message not processed - Message ID: {MessageId}, Body: {Body}", className, methodName,message.MessageId,message.Body);
                    //set job history status
                    etlExecutionContext.JobHistoryStatus = chunkCounter == 0
                        ? Constants.JOB_HISTORY_FAILURE_STATUS
                        : Constants.JOB_HISTORY_PARTIAL_SUCCESS_STATUS;
                    etlExecutionContext.JobHistoryErrorLog = $"Errored records count: {processedMessagesList.Count}";
                }

                throw;
            }
        }

        /// <summary>
        /// Encrypts the generated card creation file and uploads it to a secure location.
        /// calls common EncryptGeneratedFile() from _s3FileEncryptionHelper
        /// </summary>
        /// <param name="etlExecutionContext">The execution context containing tenant and batch operation information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task EncryptFile(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(EncryptFile);
            try
            {
                await _batchOperationService.ValidateTenant(etlExecutionContext.TenantCode);
                _batchOperationService.ValidateBatchOperationGroupCode(etlExecutionContext);
                await _s3FileEncryptionHelper.EncryptGeneratedFile(etlExecutionContext.BatchOperationGroupCode, etlExecutionContext.TenantCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while encrypting the file for TenantCode: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);
                throw;
            }
        }

        /// <summary>
        /// Copies the generated card file ( Encrypted/Unencrypted/both) to the specified destination.
        /// calls common CopyFileToS3Destination() from _s3FileEncryptionHelper
        /// </summary>
        /// <param name="etlExecutionContext">The execution context containing tenant and batch operation information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CopyFileToDestination(EtlExecutionContext etlExecutionContext)
        {
            try
            {
                _batchOperationService.ValidateBatchOperationGroupCode(etlExecutionContext);
                await _s3FileEncryptionHelper.CopyFileToS3Destination(etlExecutionContext.BatchOperationGroupCode, GetAwsFisSftpS3BucketName(),FIS_OUTBOUND_FOLDER);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while Copying the file for TenantCode: {TenantCode}", className, nameof(CopyFileToDestination), etlExecutionContext.TenantCode);
                throw;
            }

        }

        /// <summary>
        /// Archives the encrypted card file to a specified archive folder.
        /// calls common ArchiveFile() from _s3FileEncryptionHelper
        /// </summary>
        /// <param name="etlExecutionContext">The execution context containing tenant and batch operation information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ArchiveFile(EtlExecutionContext etlExecutionContext)
        {
            try
            {
                _batchOperationService.ValidateBatchOperationGroupCode(etlExecutionContext);
                await _s3FileEncryptionHelper.ArchiveFile(etlExecutionContext.BatchOperationGroupCode, GetAwsSunnyArchiveFileBucketName(), FISBatchConstants.FIS_APL_FILE_OUTBOUND_ARCHIVE_FOLDER);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while Copying the file for TenantCode: {TenantCode}", className, nameof(CopyFileToDestination), etlExecutionContext.TenantCode);
                throw;
            }
        }

        /// <summary>
        /// Deletes the generated or encrypted card file from the S3 bucket.
        /// calls common ArchiveFile() from _s3FileEncryptionHelper
        /// </summary>
        /// <param name="etlExecutionContext">The execution context containing tenant and batch operation information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteFile(EtlExecutionContext etlExecutionContext)
        {
            try
            {
                _batchOperationService.ValidateBatchOperationGroupCode(etlExecutionContext);
                await _s3FileEncryptionHelper.DeleteFile(etlExecutionContext.BatchOperationGroupCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while Copying the file for TenantCode: {TenantCode}", className, nameof(CopyFileToDestination), etlExecutionContext.TenantCode);
                throw;
            }
        }

        #region Private Members

        private List<FisRecordDto> ConvertMessagesToFisRecords(List<Message> messages, List<Message> processedMessages)
        {
            const string methodName=nameof(ConvertMessagesToFisRecords);
            _logger.LogInformation("{ClassName}.{MethodName} - Starting conversion of {Count} message(s) to FIS records.", className, methodName,messages.Count);

            var records = new List<FisRecordDto>();
            foreach (var message in messages)
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Processing message with ID: {MessageId}, Body: {Body}", className, methodName,message.MessageId,message.Body);

                try
                {
                    var messageBody = JsonConvert.DeserializeObject<ProductIngestionMessageDto>(message.Body);
                    if (messageBody != null)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Successfully deserialized message body for ID: {MessageId}, Body: {Body}", className, methodName,message.MessageId,message.Body);

                        var record = MapMessageToFisRecord(messageBody);
                        if (record != null)
                        {
                            records.Add(record);
                            processedMessages.Add(message);
                            _logger.LogInformation("{ClassName}.{MethodName} - Message ID: {MessageId} mapped to FIS record and added to processed messages. Body: {Body}", className, methodName, message.MessageId, message.Body);
                        }
                        else
                        {
                            _logger.LogWarning("{ClassName}.{MethodName} - Message ID: {MessageId} could not be mapped to an FIS record. Body: {Body}", className, methodName, message.MessageId, message.Body);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("{ClassName}.{MethodName} - Message ID: {MessageId} deserialization returned null. Body: {Body}", className, methodName, message.MessageId, message.Body);
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "{ClassName}.{MethodName} - JSON deserialization failed for message ID: {MessageId}. Body: {Body}", className, methodName, message.MessageId, message.Body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName} - An unexpected error occurred while processing message ID: {MessageId}. Body: {Body}", className, methodName, message.MessageId, message.Body);
                }
            }

            _logger.LogInformation("{ClassName}.{MethodName} - Conversion completed: {Count} FIS record(s) created from {Count} message(s).",className,methodName,records.Count,messages.Count);
            return records;
        }

        private async Task WriteFisRecordsToCsv(List<FisRecordDto> records, string fileName, string batchOperationGroupCode)
        {
            var s3BucketName = GetAwsTmpS3BucketName();
            const string methodName=nameof(WriteFisRecordsToCsv);
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(records);
                await writer.FlushAsync();

                // Reset the position of the memory stream to the beginning
                memoryStream.Position = 0;
               
                try
                {
                    using (var s3Client = new AmazonS3Client(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                    {
                        await s3Client.PutObjectAsync(new PutObjectRequest
                        {
                            BucketName = s3BucketName,
                            Key = fileName,
                            InputStream = memoryStream,
                            ContentType = "text/csv",
                            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                        });

                        // Log successful upload
                        _logger.LogInformation("{ClassName}.{MethodName} - FIS records written to S3 file: {FileName}", className, methodName,fileName);
                    }
                    await _batchOperationService.SaveBatchOperationGenerateRecord(batchOperationGroupCode, s3BucketName, null, fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName} - Error writing FIS records to S3 file: {fileName}, ErrorCode:{Code},ERROR: {ex.Message}", 
                        className, methodName,fileName,StatusCodes.Status500InternalServerError,ex.Message);
                    throw;
                }


            }
        }

        private async Task DeleteMessages(AmazonSQSClient sqsClient, string queueUrl, List<Message> messages)
        {
            const string methodName = nameof(DeleteMessages);
            var totalMessages = messages.Count;
            var batches = messages.Select((message, index) => new { message, index })
                                  .GroupBy(x => x.index / 10)
                                  .Select(g => g.Select(x => x.message).ToList())
                                  .ToList();

            foreach (var batch in batches)
            {
                var deleteRequests = batch.Select(message => new DeleteMessageBatchRequestEntry
                {
                    Id = message.MessageId,
                    ReceiptHandle = message.ReceiptHandle
                }).ToList();

                _logger.LogInformation("{ClassName}.{MethodName} - Initiating batch deletion of {Count} messages from the queue., Request Data: {DeleteRequests}", className, methodName, deleteRequests.Count, deleteRequests.ToJson());
                await sqsClient.DeleteMessageBatchAsync(new DeleteMessageBatchRequest
                {
                    QueueUrl = queueUrl,
                    Entries = deleteRequests
                });
                _logger.LogInformation("{ClassName}.{MethodName} - Successfully deleted {Count} messages from the queue., Deleted Messages: {DeleteRequests}", className, methodName, deleteRequests.Count, deleteRequests.ToJson());
            }

            messages.Clear();
            _logger.LogInformation("Messages deleted successfully.");
        }

        private FisRecordDto? MapMessageToFisRecord(ProductIngestionMessageDto message)
        {
            const string methodName = nameof(MapMessageToFisRecord);
            var fisRecord = new FisRecordDto
            {
                Action = GetAction(message?.Item?.Action),
                UpcOrPluValue = message?.Item?.UpcNumber,
                UpcOrPluDataLength = message?.Item?.UpcNumber?.Length,
                UpcOrPluIndicator = message?.Item?.UpcIndicator.ToString(),
                Manufacturer = message?.Item?.Data?.Manufacturer,
                Brand = string.Empty,
                ProductName = message?.Item?.Data?.ProdName != null ? message.Item?.Data?.ProdName.Substring(0, Math.Min(message.Item.Data.ProdName.Length, 150)) : string.Empty,
                ProductShortName = string.Empty,
                ProductSize = message?.Item?.Data?.ProdSize,
                UnitOfMeasure = message?.Item?.Data?.UOM,
                PkgSize = message?.Item?.Data?.PkgSize,
                DeptName = message?.Item?.Data?.DeptName,
                ProductImage = string.Empty,
                NutritionalInformationImage = string.Empty,
                IngredientsImage = string.Empty,
                DrugFactsImage = string.Empty,
                AdditionalImages = string.Empty,
                Company = message?.Item?.Company,
                ProductSKU = message?.ProductSku
            };

            // Validate mandatory fields
            if (fisRecord.Action != Action_Delete &&
                (string.IsNullOrEmpty(fisRecord.Action)
                || string.IsNullOrEmpty(fisRecord.UpcOrPluValue)
                || fisRecord.UpcOrPluDataLength == null
                || string.IsNullOrEmpty(fisRecord.UpcOrPluIndicator)
                || string.IsNullOrEmpty(fisRecord.Manufacturer)
                || string.IsNullOrEmpty(fisRecord.ProductName)
                || string.IsNullOrEmpty(fisRecord.PkgSize)
                || string.IsNullOrEmpty(fisRecord.DeptName)
                || string.IsNullOrEmpty(fisRecord.Company)))
            {
                // Log the validation failure
                _logger.LogWarning("MapMessageToFisRecord: Skipping record due to missing mandatory data. {MessageName}", message?.MessageName);
                return null;
            }
            // Validate mandatory fields
            if (fisRecord.Action == Action_Delete &&
                (string.IsNullOrEmpty(fisRecord.Action)
                || string.IsNullOrEmpty(fisRecord.UpcOrPluValue)
                || fisRecord.UpcOrPluDataLength == null
                || string.IsNullOrEmpty(fisRecord.UpcOrPluIndicator)
                || string.IsNullOrEmpty(fisRecord.Company)))
            {
                // Log the validation failure
                _logger.LogWarning("{ClassName}.{MethodName} - Skipping record due to missing mandatory data. {MessageName}", className, methodName, message?.MessageName);
                return null;
            }

            return fisRecord;
        }

        private async Task<List<string>> GetAllFileNames(AmazonS3Client s3Client, string bucketName, string? folderName)
        {
            const string methodName = nameof(GetAllFileNames);
            try
            {
                List<string> fileNames = new();

                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                };

                if (!string.IsNullOrEmpty(folderName))
                {
                    request.Prefix = folderName;
                }

                ListObjectsV2Response response;

                do
                {
                    response = await s3Client.ListObjectsV2Async(request);

                    foreach (var s3Object in response.S3Objects)
                    {
                        string fileName = Path.GetFileName(s3Object.Key);
                        if (!string.IsNullOrEmpty(fileName))
                            fileNames.Add(fileName);
                    }

                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                return fileNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error retrieving file names from S3 bucket ErrorCode:{Code},ERROR: {Message}",className,methodName,StatusCodes.Status500InternalServerError,ex.Message);
                throw;
            }
        }

        private string? GetAction(string? action)
        {
            switch (action?.ToUpper())
            {
                case "INSERT":
                case "I":
                    return "A";
                case "DELETE":
                case "D":
                    return "D";
                case "UPDATE":
                case "U":
                    return "U";
                default:
                    return null;
            }
        }

        private async Task WriteFisRecordsToCsvChunked(List<FisRecordDto> records, string fileName, bool isFirstChunk, string batchOperationGroupCode)
        {
            const string methodName = nameof(WriteFisRecordsToCsvChunked);
            var s3BucketName = GetAwsTmpS3BucketName();

            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                if (!isFirstChunk)
                {
                    // Write records without header
                    foreach (var record in records)
                    {
                        csv.WriteRecord(record);
                        csv.NextRecord();
                    }
                }
                else
                {
                    // Write records with header
                    await csv.WriteRecordsAsync(records);
                }
                await writer.FlushAsync();

                // Reset the position of the memory stream to the beginning
                memoryStream.Position = 0;

                try
                {
                    using (var s3Client = new AmazonS3Client(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                    {
                        if (!isFirstChunk)
                        {
                            // Download the existing file
                            var existingObject = await s3Client.GetObjectAsync(s3BucketName, fileName);
                            using (var existingStream = existingObject.ResponseStream)
                            using (var combinedStream = new MemoryStream())
                            {
                                await existingStream.CopyToAsync(combinedStream);
                                await memoryStream.CopyToAsync(combinedStream);
                                combinedStream.Position = 0;

                                var putObjectRequest = new PutObjectRequest
                                {
                                    BucketName = s3BucketName,
                                    Key = fileName,
                                    InputStream = combinedStream,
                                    ContentType = "text/csv",
                                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                                };

                                await s3Client.PutObjectAsync(putObjectRequest);
                            }
                        }
                        else
                        {
                            var putObjectRequest = new PutObjectRequest
                            {
                                BucketName = s3BucketName,
                                Key = fileName,
                                InputStream = memoryStream,
                                ContentType = "text/csv",
                                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                            };

                            await s3Client.PutObjectAsync(putObjectRequest);
                            await _batchOperationService.SaveBatchOperationGenerateRecord(batchOperationGroupCode, s3BucketName, null, fileName);
                        }

                        // Log successful upload
                        _logger.LogInformation("{ClassName}.{MethodName} - FIS records written to S3 file: {FileName}", className, methodName, fileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName} - Error writing FIS records to S3 file: {fileName}, ErrorCode:{Code}, ERROR: {ex.Message}",
                        className, methodName, fileName, StatusCodes.Status500InternalServerError, ex.Message);
                    throw;
                }
            }
        }

        private async Task<List<Message>> ReceiveMessagesChunk(IAmazonSQS sqsClient, string queueUrl, int maxNumberOfMessages, int messageChunkSize, string methodName)
        {
            var messagesChunk = new List<Message>();
            int count = 0;

            while (count < messageChunkSize)
            {
                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = maxNumberOfMessages
                };

                _logger.LogInformation("{ClassName}.{MethodName} - Receiving up to {MaxNumberOfMessages} messages from the Queue: {QueueUrl}", className, methodName, maxNumberOfMessages, queueUrl);
                var receiveResponse = await sqsClient.ReceiveMessageAsync(receiveRequest);

                if (!receiveResponse.Messages.Any())
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - No messages found in the queue. Exiting processing loop.", className, methodName);
                    break;
                }

                messagesChunk.AddRange(receiveResponse.Messages);
                count += receiveResponse.Messages.Count;
                _logger.LogInformation("{ClassName}.{MethodName} - Received {Count} message(s) from the queue.", className, methodName, receiveResponse.Messages.Count);
                _logger.LogInformation("{ClassName}.{MethodName} - Received messages, Response:{ReceiveResponse}", className, methodName, receiveResponse.ToJson());
            }

            return messagesChunk;
        }

        private int GetMessageChunkSize()
        {
            if (!int.TryParse(_configuration.GetSection("MessageChunkSize").Value, out var messageChunkSize))
            {
                throw new InvalidOperationException("Invalid configuration value for MessageChunkSize");
            }
            return messageChunkSize;
        }

        /// <summary>
        /// UpdateProduct
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="productIngestionMessage"></param>
        /// <returns></returns>
        private async Task UpdateProduct(string tenantCode, ProductIngestionMessageDto productIngestionMessage)
        {
            const string methodName = nameof(UpdateProduct);
            try
            {
                var xApiKeySecret = await _secretHelper.GetTenantSecret(tenantCode, Constants.XApiKeySecret);

                var authHeaders = new Dictionary<string, string>
                    {
                        { Constants.XApiKey, xApiKeySecret },
                    };
                var response = await _retailClient.Post<BaseResponseDto>(Constants.RetailProductUpdate, productIngestionMessage, authHeaders);

                if (response.ErrorCode != null || !String.IsNullOrEmpty(response.ErrorMessage))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error Response from Retail API For Request:{Request}.,ErrorCode:{Code}, ERROR: {Message}",
                        className, methodName, productIngestionMessage.ToJson(), response.ErrorCode, response.ErrorMessage);
                    throw new Exception($"Error occurred during ProductUpdate from {Constants.RetailProductUpdate} API");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error while restoring Costco messages from dynamoDB to SQS, ErrorCode:{Code},ERROR:{Msg}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }

        }

        #endregion

        /// <summary>
        /// Restore Costco messages Backup from DynamoDB
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task RestoreCostcoBackupFromDynamoDB(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(RestoreCostcoBackupFromDynamoDB);
            var restoredMessages = new List<ProductIngestionMessageDto>();
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Starting to process queue messages...", className, methodName);
                if (string.IsNullOrEmpty(etlExecutionContext.TenantCode))
                {
                    _logger.LogError("{ClassName}.{MethodName} - No tenant code provided for restoring Costco backup fro dynamoDb.", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NullValue, "No tenant code provided for restoring Costco backup fro dynamoDb.");
                }

                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == etlExecutionContext.TenantCode && x.DeleteNbr == 0);
                if (tenant == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Tenant not found for tenant code: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Tenant not found in DB for tenant code: {etlExecutionContext.TenantCode}.");
                }

                var costcoMessagesSQSTableName = GetDynamoDbCostcoMessagesSQSTableName().Result;
                var dynamoDbMessageChunkSize = GetMessageChunkSize();
                dynamoDbMessageChunkSize = etlExecutionContext.BatchSize != 0 ? etlExecutionContext.BatchSize : dynamoDbMessageChunkSize;
                string queueUrl = await GetAwsRetailProductSyncQueueUrl();
                Dictionary<string, AttributeValue> lastKeyEvaluated = null;
                var minValue = etlExecutionContext.MinEpochTs;
                var maxValue = etlExecutionContext.MaxEpochTs == 0
                    ? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    : etlExecutionContext.MaxEpochTs;

                if (maxValue > minValue)
                {
                    do
                    {
                        var scanRequest = new ScanRequest
                        {
                            TableName = costcoMessagesSQSTableName,
                            ExclusiveStartKey = lastKeyEvaluated,
                            Limit = dynamoDbMessageChunkSize,
                            FilterExpression = $"{Constants.DYNAMODB_COSTCO_SQS_TABLE_EPOCHTS} {Constants.DYNAMODB_BETWEEN_MIN_MAX_VALUE_CONDITION}",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                            {
                                { Constants.DYNAMODB_MIN_VALUE_KEY_NAME, new AttributeValue { N = minValue.ToString() } },
                                { Constants.DYNAMODB_MAX_VALUE_KEY_NAME, new AttributeValue { N = maxValue.ToString() } }
                            }
                        };

                        var response = await _dynamoDbHelper.ScanAsync(scanRequest);
                        lastKeyEvaluated = response.LastEvaluatedKey;

                        // Process the items in this chunk
                        foreach (var item in response.Items)
                        {
                            if (item.ContainsKey(Constants.DYNAMODB_COSTCO_SQS_TABLE_MESSAGEBODY))
                            {
                                var messageBody = JsonConvert.DeserializeObject<ProductIngestionMessageDto>
                                    (item[Constants.DYNAMODB_COSTCO_SQS_TABLE_MESSAGEBODY].S);
                                if (messageBody == null)
                                {
                                    _logger.LogError("{ClassName}.{MethodName} - Error while parsing MessageBody column value from dynamoDb row. messageId: {MessageId} dynamoDb row:{Item}",
                                        className, methodName, item[Constants.DYNAMODB_COSTCO_SQS_TABLE_MESSAGEID].S,
                                        item.ToJson());
                                    continue;
                                }
                                //DisableDbBackup property is used to skip writing entry to DynamoDB in Retail API.
                                messageBody.DisableDbBackup = true;
                                await UpdateProduct(tenant.TenantCode, messageBody);
                                restoredMessages.Add(messageBody);
                                _logger.LogInformation("{ClassName}.{MethodName} - Restored Costco messages count: {Count}",
                                    className, methodName, restoredMessages.Count);
                            }
                            else
                            {
                                _logger.LogError("{ClassName}.{MethodName} - MessageBody column is missing from dynamoDb row. dynamoDb row:{Item}",
                                    className, methodName, item.ToJson());
                                continue;
                            }
                        }

                        // If there are more items, continue to the next chunk
                    } while (lastKeyEvaluated.Count > 0);

                }
                _logger.LogInformation("{ClassName}.{MethodName} - Finished restoring Costco messages. Count: {Count}",
                    className, methodName, restoredMessages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error while restoring Costco messages from dynamoDB to SQS," +
                    "restoring Costco messages. Count: {Count}, ErrorCode:{Code},ERROR:{Msg}",
                    restoredMessages.Count, className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

    }
}