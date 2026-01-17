using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class CardDisbursementFileCreateService : AwsConfiguration, ICardDisbursementFileCreateService
    {
        private const string FIS_OUTBOUND_FOLDER = "FIS/Batch/outbound";
        private readonly ILogger<CardDisbursementFileCreateService> _logger;
        private readonly ICardBatchFileRecordCreateService _cardBatchRecordCreateService;
        private readonly ICardDisbursementFileRecordCreateService _cardDisbursementFileRecordCreateService;
        private readonly ITenantAccountRepo _tenantAccountRepo;
        private readonly ITenantRepo _tenantRepo;
        private readonly IPgpS3FileEncryptionHelper _s3FileEncryptionHelper;
        private readonly IS3Helper _s3Helper;
        private readonly IBatchOperationService _batchOperationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        const string className = nameof(CardDisbursementFileCreateService);

        public CardDisbursementFileCreateService(ILogger<CardDisbursementFileCreateService> logger,
            IConfiguration configuration, IVault vault, IPgpS3FileEncryptionHelper s3FileEncryptionHelper,
            ICardBatchFileRecordCreateService cardBatchRecordCreateService,
            ITenantAccountRepo tenantAccountRepo, ITenantRepo tenantRepo,
            ICardDisbursementFileRecordCreateService cardDisbursementFileRecordCreateService,
            IS3Helper s3Helper, IBatchOperationService batchOperationService, IDateTimeHelper dateTimeHelper) : base(vault, configuration)
        {
            _logger = logger;
            _cardBatchRecordCreateService = cardBatchRecordCreateService;
            _tenantAccountRepo = tenantAccountRepo;
            _tenantRepo = tenantRepo;
            _cardDisbursementFileRecordCreateService = cardDisbursementFileRecordCreateService;
            _s3FileEncryptionHelper = s3FileEncryptionHelper;
            _s3Helper = s3Helper;
            _batchOperationService = batchOperationService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task GenerateCardLoadFile(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(GenerateCardLoadFile);
            try
            {
                var tenantCode = etlExecutionContext.TenantCode;
                _logger.LogInformation("{className}.{methodName}: Started card create file creation for TenantCode: {TenantCode}", className, methodName, tenantCode);

                await _batchOperationService.ValidateTenant(tenantCode);
               _batchOperationService.ValidateBatchOperationGroupCode(etlExecutionContext);
                var tenantConfig = await ValidateSubProgramId(methodName, tenantCode);

                // Generate card load file
                Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                using (var memoryStream = new MemoryStream())
                using (var writer = new StreamWriter(memoryStream, utf8WithoutBom))
                {
                    writer.NewLine = BenefitsConstants.NewLineSequenceToCRLF;
                    var subprogramId = tenantConfig.FISProgramDetail!.SubprogramId!;
                    var tempS3BucketName = GetAwsTmpS3BucketName();
                    var archiveS3Bucket = GetAwsSunnyArchiveFileBucketName();
                    var archiveDecryptedLocation = $"{FISBatchConstants.FIS_OUTBOUND_ARCHIVE_FOLDER}/{FISBatchConstants.DECRYPTED_FOLDER_NAME}";

                    await GenerateCardLoadFileAsync(etlExecutionContext, writer);

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
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while creating card create file for TenantCode: {TenantCode}, Error Msg:{msg}", className, methodName, etlExecutionContext.TenantCode, ex.Message);
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


        #region Private members

        private async Task<FISTenantConfigDto> ValidateSubProgramId(string methodName, string tenantCode)
        {
            // Retrieve tenant account information and tenant configuration
            var tenantAccount = await _tenantAccountRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenantAccount == null || tenantAccount.TenantConfigJson == null)
            {
                var msg = $"{className}.{methodName}: Tenant account or tenantAccount configuration is not available for Tenant: {tenantCode}, Error Code:{StatusCodes.Status404NotFound}";
                _logger.LogError(msg);
                throw new ETLException(ETLExceptionCodes.NotFoundInDb, msg);
            }
            // Deserialize tenant configuration
            var tenantConfig = JsonConvert.DeserializeObject<FISTenantConfigDto>(tenantAccount.TenantConfigJson);
            if (tenantConfig == null || tenantConfig.FISProgramDetail == null || tenantConfig.FISProgramDetail.SubprogramId == null)
            {
                var msg = $"{className}.{methodName}: GenerateCardLoadAsync SubprogramId not found or empty for Tenant: {tenantCode}";
                _logger.LogError(msg);
                throw new ETLException(ETLExceptionCodes.NullValue, msg);
            }
            return tenantConfig;
        }
        private async Task GenerateCardLoadFileAsync(EtlExecutionContext etlExecutionContext, StreamWriter writer)
        {
            const string methodName = nameof(GenerateCardLoadFileAsync);
            var tenantCode = etlExecutionContext.TenantCode;

            // Check if a valid tenant code is provided
            if (string.IsNullOrEmpty(tenantCode))
            {
                _logger.LogError("{className}.{methodName}: No tenant code provided.Error Code:{errorCode}", className, methodName, StatusCodes.Status404NotFound);
                throw new ETLException(ETLExceptionCodes.NullValue, "No tenant code provided");
            }
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started card load file creation for TenantCode: {TenantCode}", className, methodName, tenantCode);

                // Retrieve tenant information
                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
                if (tenant == null)
                {
                    _logger.LogError("{className}.{methodName}: Invalid tenant code: {TenantCode}, Error Code:{errorCode}", className, methodName, tenantCode, StatusCodes.Status400BadRequest);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Invalid tenant code: {tenantCode}");
                }

                var tenantAttributes = !string.IsNullOrEmpty(tenant.TenantAttribute)
                        ? JsonConvert.DeserializeObject<TenantAttribute>(tenant.TenantAttribute)
                        : new TenantAttribute();

                // Retrieve tenant account information and tenant configuration
                var tenantAccount = await _tenantAccountRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
                if (tenantAccount == null || tenantAccount.TenantConfigJson == null)
                {
                    _logger.LogError("{className}.{methodName}: Tenant configuration not available for Tenant: {TenantCode}, Error Code:{errorCode}", className, methodName, tenantCode, StatusCodes.Status404NotFound);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Tenant Account or TenantAccount configuration is not available for Tenant: {tenantCode}");
                }

                // Deserialize tenant configuration
                FISTenantConfigDto tenantConfig = JsonConvert.DeserializeObject<FISTenantConfigDto>(tenantAccount.TenantConfigJson);
                if (tenantConfig == null || tenantConfig.PurseConfig == null || tenantConfig.PurseConfig.Purses == null || !tenantConfig.PurseConfig.Purses.Any())
                {
                    _logger.LogWarning("{className}.{methodName}: Purse configuration not found or empty for Tenant: {TenantCode}, Error Code:{errorCode}", className, methodName, tenantCode, StatusCodes.Status404NotFound);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Purse configuration not found or empty in TenantAccount with TenantCode: {tenantCode}");
                }

                //Verify JustInTimeFunding is enabled and if current tenant timezone hour == 23 as per tenant.utc_time_offset and dst_enabled columns
                //If not skip creating card 60
                if (tenantAttributes?.JustInTimeFunding == true && tenant.DstEnabled)
                {
                    if (string.IsNullOrEmpty(tenant.UtcTimeOffset))
                    {
                        _logger.LogError("{className}.{methodName}: Current tenant utc_time_offset value is null for TenantCode: {TenantCode}",
                            className, methodName, tenantCode);
                        throw new ETLException(ETLExceptionCodes.InValidValue, $"Current tenant utc_time_offset value is null for TenantCode: {tenantCode}");
                    }
                    var specificDateTime = _dateTimeHelper.GetUtcOffsetDateTime(tenant.UtcTimeOffset);

                    if (specificDateTime.Hour != 23)
                    {
                        _logger.LogError("{className}.{methodName}: Current tenant timezone hour is not 23. TenantCode: {TenantCode}, utcOffset:{utcOffset}, specificDateTime:{specificDateTime}",
                            className, methodName, tenantCode, tenant.UtcTimeOffset, specificDateTime);
                        throw new ETLException(ETLExceptionCodes.InValidValue, $"Current tenant timezone hour is not 23. TenantCode: {tenantCode}, utcOffset:{tenant.UtcTimeOffset}, specificDateTime:{specificDateTime}");
                    }
                }

                await _cardBatchRecordCreateService.Init(etlExecutionContext);

                var fileHeader = _cardBatchRecordCreateService.GenerateFileHeader(etlExecutionContext);
                var batchHeader = _cardBatchRecordCreateService.GenerateBatchHeader(etlExecutionContext, 1);

                writer.WriteLine(fileHeader);
                writer.WriteLine(batchHeader);


                int totalRecords = 0;
                double totalCreditAmount = 0;

                // generate 60 record type for each consumer
                var (disbursementRecords, totalRedemptionAmount) = await _cardDisbursementFileRecordCreateService.GenerateDisbursementRecords(
                     etlExecutionContext, tenantConfig.PurseConfig.Purses, tenantAttributes?.JustInTimeFunding ?? false);

                foreach (var disbursementRecord in disbursementRecords)
                {
                    writer.WriteLine(disbursementRecord);
                }

                totalRecords = disbursementRecords.Count;
                totalCreditAmount = totalRedemptionAmount;

                var totalFileRecs = totalRecords + 4; // including 4 file/batch headers/trailers

                var batchTrailer = _cardBatchRecordCreateService.GenerateBatchTrailer(
                    etlExecutionContext, 1, totalRecords, 0.0f, totalCreditAmount);
                var fileTrailer = _cardBatchRecordCreateService.GenerateFileTrailer(
                    etlExecutionContext, totalFileRecs, 1, totalRecords, 0.0f, totalCreditAmount);

                writer.WriteLine(batchTrailer);
                writer.WriteLine(fileTrailer);

                writer.Flush();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while creating card load file for TenantCode: {TenantCode}, Error msg:{msg}", className, methodName, tenantCode, ex.Message);
                throw;
            }
        }

        private async Task<string> GetUniqueFileName(string subprogramId, string sunnyFileArchiveBucketName)
        {
            string formattedDate = DateTime.UtcNow.ToString("MMddyyyy");
            var filePrefix = $"{subprogramId}{formattedDate}";
            var sequence = "01";
            var filePrexWithFolder = $"{FISBatchConstants.FIS_OUTBOUND_ARCHIVE_FOLDER}/{FISBatchConstants.DECRYPTED_FOLDER_NAME}/{filePrefix}";
            var filenames = await GetAllFileNames(sunnyFileArchiveBucketName, filePrexWithFolder);
            if (filenames.Any())
            {
                var nextSequenceNumber = GetNextSequence(filenames, filePrefix);
                sequence = nextSequenceNumber.ToString("D2");
            }
            var fileName = $"{filePrefix}{sequence}.load.txt";
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
                if (fileName.StartsWith(prefix) && fileName.Contains("load"))
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

        #endregion
    }
}
