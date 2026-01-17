using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class SyncMembersFromRedshiftToPostgresService : ISyncMembersFromRedshiftToPostgresService
    {
        private readonly string _redshiftConnectionString;
        private readonly string _postgresConnectionString;
        private readonly IRedshiftSyncStatusRepo _redshiftSyncStatusRepo;
        private readonly IETLMemberImportFileRepo _memberImportFileRepo;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SyncMembersFromRedshiftToPostgresService> _logger;
        private readonly IRedshiftDataReader _redshiftDataReader;
        private readonly IPostgresBulkInserter _postgresBulkInserter;
        private const string className = nameof(TenantConfigSyncService);

        public SyncMembersFromRedshiftToPostgresService(
            ILogger<SyncMembersFromRedshiftToPostgresService> logger,
            ISecretHelper secretHelper,
            IRedshiftSyncStatusRepo redshiftSyncStatusRepo,
            IETLMemberImportFileRepo memberImportFileRepo,
            IMemoryCache cache, IRedshiftDataReader redshiftDataReader,
            IPostgresBulkInserter postgresBulkInserter)
        {
            _logger = logger;
            _redshiftSyncStatusRepo = redshiftSyncStatusRepo;
            _memberImportFileRepo = memberImportFileRepo;
            _cache = cache;
            _redshiftDataReader = redshiftDataReader;
            _postgresBulkInserter = postgresBulkInserter;
            _postgresConnectionString = secretHelper.GetPostgresConnectionString().Result;
            _redshiftConnectionString = secretHelper.GetRedshiftConnectionString().Result;
        }


        public async Task SyncAsync(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(SyncAsync);
            int batchSize = etlExecutionContext.BatchSize;
            if (batchSize <= 0)
            {
                _logger.LogInformation("{ClassName}.{MethodName} Invalid batch size specified. Defaulting to 1000.", className, methodName);
                batchSize = 1000;
            }
            if (batchSize > 5000)
            {
                _logger.LogInformation("{ClassName}.{MethodName} Batch size should not be greater than 5000. Defaulting to 1000.", className, methodName);
                batchSize = 1000;
            }

            _logger.LogInformation("{ClassName}.{MethodName} Starting Redshift-to-Postgres sync. Batch size: {BatchSize}", className, methodName, batchSize);
            var lastRun = await GetLastProcessedSyncStatusAsync(Constants.RedshiftSyncMemberImportDataType);
            long? lastMemberImportFileId = lastRun?.LastLoadedId;
            int totalProcessed = 0;
            var allFileNames = new HashSet<string>();

            try
            {
                while (true)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} Fetching next batch from Redshift after Member Import FileId: {LastMemberImportFileId}", className, methodName, lastMemberImportFileId);
                    var batch = await _redshiftDataReader.FetchBatchAsync(_redshiftConnectionString, lastMemberImportFileId, batchSize);

                    if (batch.Count == 0)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} No new records found. Sync complete.", className, methodName);
                        break;
                    }
                    _logger.LogInformation("{ClassName}.{MethodName} Fetched {Count} records from Redshift", className, methodName, batch.Count);

                    foreach (var fileName in batch.Select(b => b.FileName))
                        allFileNames.Add(fileName);

                    var lastCreatedMemberImportFileDataId = await InsertBatchToPostgresAsync(batch, totalProcessed);

                    if (lastCreatedMemberImportFileDataId.HasValue)
                    {
                        lastMemberImportFileId = lastCreatedMemberImportFileDataId.Value;
                        totalProcessed += batch.Count;
                        _logger.LogInformation("{ClassName}.{MethodName} Inserted batch to Postgres. Updated offset: {Offset}", className, methodName, totalProcessed);
                    }
                    else
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} Insert failed or skipped. Breaking the sync loop.", className, methodName);
                        break;
                    }

                    await UpdateSyncStatusAsync(lastMemberImportFileId, batch.Count, null);
                    
                    _logger.LogInformation("{ClassName}.{MethodName} Updated sync status. Records synced: {Count}, Last LastMemberImportFileId: {LastMemberImportFileId}", className, methodName, batch.Count, lastMemberImportFileId);
                }
                if (etlExecutionContext.ShouldMarkFileAsCompleted)
                {
                    foreach (var fileName in allFileNames)
                    {
                        var fileId = await GetOrCreateMemberImportFileIdAsync(fileName);
                        await UpdateFileStatus(fileId);
                        _logger.LogInformation("{ClassName}.{MethodName} Marked file {FileName} (ID: {FileId}) as COMPLETED.", className, methodName, fileName, fileId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} Error during Redshift sync. Total records processed before failure: {Count}", className, methodName, totalProcessed);
                await UpdateSyncStatusAsync(lastMemberImportFileId, totalProcessed, ex.Message);
                throw;
            }
        }

        private async Task<long?> InsertBatchToPostgresAsync(List<RedShiftMemberImportFileDataDto> batch, int recordOffset)
        {
            const string methodName = nameof(InsertBatchToPostgresAsync);
            if (batch == null || batch.Count == 0)
                return null;

            var models = new List<ETLMemberImportFileDataModel>();

            foreach (var record in batch)
            {
                var fileId = await GetOrCreateMemberImportFileIdAsync(record.FileName);
                var model = MapToImportFileDataModel(record, fileId);
                models.Add(model);
            }

            var lastMemberImportFileDataId = models.Max(m => m.MemberImportFileDataId);

            try
            {
                await _postgresBulkInserter.BulkInsertAsync(_postgresConnectionString, models);

                _logger.LogInformation("{ClassName}.{MethodName} Successfully bulk inserted {Count} records to Postgres. Last MemberImportFileDataId: {lastMemberImportFileDataId}", className, methodName, models.Count, lastMemberImportFileDataId);
                return lastMemberImportFileDataId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} Bulk insert to PostgreSQL failed. Batch start offset: {Offset}, Count: {Count}", className, methodName, recordOffset, batch.Count);
                return null;
            }
        }


        private ETLMemberImportFileDataModel MapToImportFileDataModel(RedShiftMemberImportFileDataDto record, long fileId)
        {
            return new ETLMemberImportFileDataModel
            {
                MemberImportFileDataId = record.MemberImportFileDataId,
                MemberImportFileId = fileId,
                RecordNumber = record.RecordNumber,
                RawDataJson = record.RawDataJson!,
                MemberId = record.MemberId,
                MemberType = record.MemberType,
                LastName = record.LastName,
                FirstName = record.FirstName,
                Gender = record.Gender,
                Age = record.Age,
                Dob = record.Dob,
                Email = record.Email,
                City = record.City,
                Country = record.Country,
                PostalCode = record.PostalCode,
                MobilePhone = record.MobilePhone,
                EmpOrDep = record.EmpOrDep,
                MemNbr = record.MemNbr,
                SubscriberMemNbr = record.SubscriberMemNbr,
                EligibilityStart = record.EligibilityStart,
                EligibilityEnd = record.EligibilityEnd,
                MailingAddressLine1 = record.MailingAddressLine1,
                MailingAddressLine2 = record.MailingAddressLine2,
                MailingState = record.MailingState,
                MailingCountryCode = record.MailingCountryCode,
                HomePhoneNumber = record.HomePhoneNumber,
                Action = record.Action,
                PartnerCode = record.PartnerCode,
                MiddleName = record.MiddleName,
                HomeAddressLine1 = record.HomeAddressLine1,
                HomeAddressLine2 = record.HomeAddressLine2,
                HomeState = record.HomeState,
                HomeCity = record.HomeCity,
                HomePostalCode = record.HomePostalCode,
                LanguageCode = record.LanguageCode,
                RegionCode = record.RegionCode,
                SubscriberMemNbrPrefix = record.SubscriberMemNbrPrefix,
                MemNbrPrefix = record.MemNbrPrefix,
                PlanId = record.PlanId,
                PlanType = record.PlanType,
                SubgroupId = record.SubgroupId,
                IsSsoUser = record.IsSsoUser,
                PersonUniqueIdentifier = record.PersonUniqueIdentifier,
                CreateTs = record.CreateTs,
                CreateUser = Constants.CreateUserAsETL,
                UpdateUser = record.UpdateUser,
                DeleteNbr = 0
            };
        }


        private async Task<long> GetOrCreateMemberImportFileIdAsync(string? fileName)
        {
            const string methodName = nameof(GetOrCreateMemberImportFileIdAsync);
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

            if (_cache.TryGetValue(fileName, out long cachedId))
                return cachedId;

            var fileRecord = (await _memberImportFileRepo.FindAsync(x => x.FileName == fileName && x.FileStatus == nameof(FileStatus.NOT_STARTED) &&
                    x.DeleteNbr == 0))
                    ?.OrderByDescending(x => x.CreateTs)
                    .FirstOrDefault();

            if (fileRecord != null)
            {
                CacheFileId(fileName, fileRecord.MemberImportFileId);
                return fileRecord.MemberImportFileId;
            }

            _logger.LogInformation("{ClassName}.{MethodName} No member import file found. Creating new one for: {FileName}", className, methodName, fileName);

            var newFile = new ETLMemberImportFileModel
            {
                FileName = fileName,
                CreateTs = DateTime.UtcNow,
                CreateUser = Constants.CreateUserAsETL,
                MemberImportCode = $"mic-{Guid.NewGuid():N}",
                FileStatus = FileStatus.NOT_STARTED.ToString(),
                DeleteNbr = 0
            };

            var createdFile = await _memberImportFileRepo.CreateAsync(newFile);

            if (createdFile?.MemberImportFileId <= 0)
                throw new InvalidOperationException($"{className}.{methodName} Failed to insert new member import file for: {fileName}");

            CacheFileId(fileName, createdFile!.MemberImportFileId);
            return createdFile.MemberImportFileId;
        }

        private void CacheFileId(string fileName, long fileId)
        {
            _cache.Set(fileName, fileId, TimeSpan.FromHours(Constants.DefaultCacheDurationForMemberImportFile));
        }

        private async Task UpdateSyncStatusAsync(long? lastLoadedId, int count, string? errorMessage)
        {
            var syncStatus = new ETLRedshiftSyncStatusModel
            {
                DataType = Constants.RedshiftSyncMemberImportDataType,
                LastLoadedId = lastLoadedId ?? 0,
                RecordsProcessed = count,
                ErrorMessage = errorMessage,
                DeleteNbr = 0,
                CreateTs = DateTime.UtcNow,
                CreateUser = Constants.CreateUserAsETL
            };

            await _redshiftSyncStatusRepo.CreateAsync(syncStatus);
        }

        private async Task<ETLRedshiftSyncStatusModel?> GetLastProcessedSyncStatusAsync(string dataType)
        {
            var records = await _redshiftSyncStatusRepo.FindAsync(x =>
                x.DataType == dataType &&
                x.DeleteNbr == 0);

            return records
                .OrderByDescending(x => x.LastLoadedId)
                .FirstOrDefault();
        }

        private async Task UpdateFileStatus(long fileId)
        {
            var fileRecord = await _memberImportFileRepo.FindOneAsync(x =>
                x.MemberImportFileId == fileId &&
                x.DeleteNbr == 0);
            if (fileRecord != null)
            {
                fileRecord.FileStatus = FileStatus.COMPLETED.ToString();
                fileRecord.UpdateTs = DateTime.UtcNow;
                fileRecord.UpdateUser = Constants.CreateUserAsETL;
                await _memberImportFileRepo.UpdateAsync(fileRecord);
            }
        }
    }
}
