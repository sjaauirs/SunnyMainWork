using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class CardBatchFileCreateService : AwsConfiguration, ICardBatchFileCreateService
    {
        private static int CARD_CREATE_MAX_RECORDS = 30000;
        private static int CARD_CREATE_CHUNK_SIZE = 100;

        private readonly ILogger<CardBatchFileCreateService> _logger;
        private readonly ICardBatchFileRecordCreateService _cardBatchRecordCreateService;
        private readonly IPgpS3FileEncryptionHelper _s3FileEncryptionHelper;
        private readonly IS3Helper _s3Helper;
        private readonly ITenantAccountRepo _tenantAccountRepo;
        private readonly ITenantRepo _tenantRepo;
        private readonly IBatchOperationService _batchOperationService;
        const string className = nameof(CardBatchFileCreateService);

        public CardBatchFileCreateService(ILogger<CardBatchFileCreateService> logger,
            IConfiguration configuration, IVault vault, ICardBatchFileRecordCreateService cardBatchRecordCreateService,
            IPgpS3FileEncryptionHelper s3FileEncryptionHelper, IS3Helper s3Helper, ITenantAccountRepo tenantAccountRepo, ITenantRepo tenantRepo
            , IBatchOperationService batchOperationService) : base(vault, configuration)
        {
            _logger = logger;
            _cardBatchRecordCreateService = cardBatchRecordCreateService;
            _s3FileEncryptionHelper = s3FileEncryptionHelper;
            _s3Helper = s3Helper;
            _tenantAccountRepo = tenantAccountRepo;
            _tenantRepo = tenantRepo;
            _batchOperationService = batchOperationService;
        }

        /// <summary>
        /// Generates a card creation file for the specified tenant.
        /// </summary>
        /// <param name="etlExecutionContext">The execution context containing tenant and batch operation information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GenerateCardCreateFile(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(GenerateCardCreateFile);
            try
            {
                string tenantCode = etlExecutionContext.TenantCode;
                _logger.LogInformation("{className}.{methodName}: Started card create file creation for TenantCode: {TenantCode}", className, methodName, tenantCode);

                await _batchOperationService.ValidateTenant(tenantCode);
                _batchOperationService.ValidateBatchOperationGroupCode(etlExecutionContext);
                var tenantConfig = await ValidateSubProgramId(methodName, tenantCode);
                var tenantOption = await GetTenantOptionCardIssueFlowType(tenantCode);
                
                // Generate card create file
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                using (var memoryStream = new MemoryStream())
                using (var writer = new StreamWriter(memoryStream, utf8WithoutBom))
                {
                    writer.NewLine = BenefitsConstants.NewLineSequenceToCRLF;
                    var subprogramId = tenantConfig.FISProgramDetail!.SubprogramId;
                    var tempS3BucketName = GetAwsTmpS3BucketName();
                    var archiveS3Bucket = GetAwsSunnyArchiveFileBucketName();
                    var archiveDecryptedLocation = $"{FISBatchConstants.FIS_OUTBOUND_ARCHIVE_FOLDER}/{FISBatchConstants.DECRYPTED_FOLDER_NAME}";

                    await GenerateCardCreateFileAsync(etlExecutionContext, writer, tenantOption);

                    MemoryStream copiedStream = new MemoryStream();
                    memoryStream.CopyTo(copiedStream);
                    // Reset the stream position to the beginning
                    memoryStream.Position = 0;

                    // generate file name
                    string fileName = await GetUniqueFileName(subprogramId, archiveS3Bucket);

                    // Upload the generated file to temp S3 bucket
                    await _s3Helper.UploadFileToS3(memoryStream, tempS3BucketName, fileName);

                    using (copiedStream)
                    {
                        copiedStream.Position = 0;
                        await _s3Helper.UploadFileToS3(copiedStream, archiveS3Bucket, $"{archiveDecryptedLocation}/{fileName}");
                    }

                    await _batchOperationService.SaveBatchOperationGenerateRecord(etlExecutionContext.BatchOperationGroupCode, tempS3BucketName, null, fileName);

                    if (!string.IsNullOrEmpty(etlExecutionContext.LocalDownloadFolderPath))
                    {
                        await _s3Helper.DownloadFileToLocalFolder(tempS3BucketName, fileName, etlExecutionContext.LocalDownloadFolderPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while creating card create file for TenantCode: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);
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
                await _s3FileEncryptionHelper.CopyFileToS3Destination(etlExecutionContext.BatchOperationGroupCode, GetAwsFisSftpS3BucketName(), Constants.FIS_OUTBOUND_FOLDER);
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
                await _s3FileEncryptionHelper.ArchiveFile(etlExecutionContext.BatchOperationGroupCode, GetAwsSunnyArchiveFileBucketName(), FISBatchConstants.FIS_OUTBOUND_ARCHIVE_FOLDER);
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


        private async Task<FISTenantConfigDto> ValidateSubProgramId(string methodName, string tenantCode)
        {
            // Retrieve tenant account information and tenant configuration
            var tenantAccount = await _tenantAccountRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenantAccount == null || tenantAccount.TenantConfigJson == null)
            {
                var msg = $"{className}.{methodName}: Tenant configuration not available for Tenant: {tenantCode}, Error Code:{StatusCodes.Status404NotFound}";
                _logger.LogWarning(msg);
                throw new InvalidDataException(msg);
            }
            // Deserialize tenant configuration
            var tenantConfig = JsonConvert.DeserializeObject<FISTenantConfigDto>(tenantAccount.TenantConfigJson);
            if (tenantConfig == null || tenantConfig.FISProgramDetail == null || tenantConfig.FISProgramDetail.SubprogramId == null)
            {
                var msg = $"{className}.{methodName}: GenerateCardLoadAsync SubprogramId not found or empty for Tenant: {tenantCode}";
                throw new InvalidDataException(msg);
            }
            return tenantConfig;
        }
        private async Task<TenantOption> GetTenantOptionCardIssueFlowType(string tenantCode)
        {
            const string methodName = nameof(GetTenantOptionCardIssueFlowType);

            // Retrieve tenant account information and tenant configuration
            var tenantModel = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenantModel.TenantOption == null)
            {
               return new TenantOption();
            }
            // Deserialize tenant configuration
            var tenantOption = JsonConvert.DeserializeObject<TenantOption>(tenantModel.TenantOption);
            if (tenantOption == null || tenantOption.BenefitsOptions == null|| tenantOption.BenefitsOptions.CardIssueFlowType==null)
            {
                return new TenantOption();

            }
            return tenantOption;
        }
        private async Task<string> GetUniqueFileName(string subprogramId, string sunnyArchiveFileBucketName)
        {
            string formattedDate = DateTime.UtcNow.ToString("MMddyyyy");
            var filePrefix = $"{subprogramId}{formattedDate}";
            var sequence = "01";
            var filePrexWithFolder = $"{FISBatchConstants.FIS_OUTBOUND_ARCHIVE_FOLDER}/{FISBatchConstants.DECRYPTED_FOLDER_NAME}/{filePrefix}";
            var filenames = await GetAllFileNames(sunnyArchiveFileBucketName, filePrexWithFolder);
            if (filenames.Any())
            {
                var nextSequenceNumber = GetNextSequence(filenames, filePrefix);
                sequence = nextSequenceNumber.ToString("D2");
            }
            var fileName = $"{filePrefix}{sequence}.issuance.txt";
            _logger.LogInformation("{className}.GetUniqueFileName: generated File Name:{fileName}", className, fileName);
            return fileName;
        }

        private async Task<List<string>> GetAllFileNames(string bucketName, string? folderName)
        {
            try
            {
                using (var s3Client = new AmazonS3Client(await GetAwsAccessKey(), await GetAwsSecretKey(), RegionEndpoint.USEast2))
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{className}.GetAllFileNames: Error retrieving file names from S3 bucket: {ex.Message}");
                throw;
            }
        }


        private int GetNextSequence(List<string> fileNames, string prefix)
        {
            int maxSequence = 0;

            foreach (var fileName in fileNames)
            {
                if (fileName.StartsWith(prefix) && fileName.Contains("issuance"))
                {
                    string sequenceStr = fileName.Substring(prefix.Length, 2);
                    if (int.TryParse(sequenceStr, out int sequence))
                    {
                        maxSequence = Math.Max(maxSequence, sequence);
                    }
                }
            }

            return maxSequence + 1;
        }
        private async Task GenerateCardCreateFileAsync(EtlExecutionContext etlExecutionContext, StreamWriter writer, TenantOption tenantOption)
        {
            int max = CARD_CREATE_MAX_RECORDS;
            await _cardBatchRecordCreateService.Init(etlExecutionContext);

            var fileHeader = _cardBatchRecordCreateService.GenerateFileHeader(etlExecutionContext);
            var batchHeader = _cardBatchRecordCreateService.GenerateBatchHeader(etlExecutionContext, 1, FISBatchConstants.FILE_HEADER_CLIENT_UNIQUE_ID_INDICATOR, FISBatchConstants.GENERATE_CLIENT_UNIQUE_ID_INDICATOR);

            writer.WriteLine(fileHeader);
            writer.WriteLine(batchHeader);

            // generate 30 record type for each consumer
            bool done = false;
            int index = 0;
            int totalCards = 0;
            int processedPersonRecords = 0;
            int pagination = tenantOption.BenefitsOptions.IncludeDiscretionaryCardData ? 3 : 2;

            while (!done)
            {
                List<string> cardCreateRecs = new List<string>();
                processedPersonRecords = 0;

                if (etlExecutionContext.FISCreateCards)
                {
                    cardCreateRecs = await _cardBatchRecordCreateService.GenerateCardHolderData(
                    etlExecutionContext, index, CARD_CREATE_CHUNK_SIZE, tenantOption);
                    processedPersonRecords += cardCreateRecs.Count / pagination; //In the above, 2 card records are generated for each person
                }

                if (etlExecutionContext.IsUpdateUserInfoInFIS)
                {
                    cardCreateRecs = await _cardBatchRecordCreateService.UpdateCardHolderData(etlExecutionContext, CARD_CREATE_CHUNK_SIZE);
                }

                if (cardCreateRecs.Count < CARD_CREATE_CHUNK_SIZE)
                {
                    done = true;
                }

                foreach (var cardCreateRec in cardCreateRecs)
                {
                    writer.WriteLine(cardCreateRec);
                }

                totalCards += cardCreateRecs.Count;
                index += processedPersonRecords;

                if (index >= max)
                {
                    break;
                }
            }

            var totalFileRecs = totalCards + 4; // including 4 file/batch headers/trailers

            var batchTrailer = _cardBatchRecordCreateService.GenerateBatchTrailer(
                etlExecutionContext, 1, totalCards, 0.0f, 0.0f);
            var fileTrailer = _cardBatchRecordCreateService.GenerateFileTrailer(
                etlExecutionContext, totalFileRecs, 1, totalCards, 0.0f, 0.0f);

            writer.WriteLine(batchTrailer);
            writer.WriteLine(fileTrailer);

            writer.Flush();
        }
        #endregion
    }
}
