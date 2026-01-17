using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using System.Collections.Generic;
using System.Transactions;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class CardBatchFileReadService : AwsConfiguration, ICardBatchFileReadService
    {
        private readonly ITenantRepo _tenantRepo;
        private readonly IPgpS3FileEncryptionHelper _s3FileEncryptionHelper;
        private readonly ILogger<CardBatchFileReadService> _logger;
        private readonly IFISFlatFileRecordDtoFactory _fISFlatFileRecordDtoFactory;
        private readonly IRecordType30ProcessService _recordType30ProcessService;
        private readonly IRecordType60ProcessService _recordType60ProcessService;
        private readonly IS3Helper _s3Helper;
        private readonly IJobReportService _jobReportService;
        const string className = nameof(CardBatchFileReadService);



        public CardBatchFileReadService(ITenantRepo tenantRepo, ILogger<CardBatchFileReadService> logger, IVault vault, IConfiguration configuration,
            IPgpS3FileEncryptionHelper s3FileEncryptionHelper, IFISFlatFileRecordDtoFactory fISFlatFileRecordDtoFactory,
            IRecordType30ProcessService recordType30ProcessService, IRecordType60ProcessService recordType60ProcessService,
            IS3Helper s3Helper, IJobReportService jobReportService)
            : base(vault, configuration)
        {
            _logger = logger;
            _tenantRepo = tenantRepo;
            _s3FileEncryptionHelper = s3FileEncryptionHelper;
            _fISFlatFileRecordDtoFactory = fISFlatFileRecordDtoFactory;
            _recordType30ProcessService = recordType30ProcessService;
            _recordType60ProcessService = recordType60ProcessService;
            _s3Helper = s3Helper;
            _jobReportService = jobReportService;
        }

        public async Task CardBatchFileReadAsync(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(CardBatchFileReadAsync);
            _jobReportService.BatchJobRecords.JobType = etlExecutionContext.FIS30RecordFileLoad ? nameof(etlExecutionContext.FIS30RecordFileLoad) : nameof(etlExecutionContext.FIS60RecordFileLoad);
            _logger.LogInformation($"{className}.{methodName}: Starting to process read card batch file...");

            
            var secureFileTransferRequestDto = new SecureFileTransferRequestDto
            {
                TenantCode = etlExecutionContext.TenantCode,
                SourceBucketName = GetAwsFisSftpS3BucketName(),
                SourceFileName = etlExecutionContext.FISRecordFileName,
                SourceFolderName = FISBatchConstants.FIS_INBOUND_FOLDER,
                InboundArchiveFolderName = FISBatchConstants.FIS_INBOUND_ARCHIVE_FOLDER,
                FisAplPublicKeyName = GetFisAplPublicKeyName(),
                SunnyAplPrivateKeyName = GetSunnyAplPrivateKeyName(),
                SunnyAplPublicKeyName = GetSunnyAplPublicKeyName(),
                TargetBucketName = GetAwsFisSftpS3BucketName(),
                ArchiveBucketName = GetAwsSunnyArchiveFileBucketName(),
                TargetFileName = "",
                DeleteFileAfterCopy = false,
                PassPhraseKeyName = GetSunnyPrivateKeyPassPhraseKeyName()
            };
            // Download and decrypt the file
            var response = await _s3FileEncryptionHelper.DownloadAndDecryptFile(secureFileTransferRequestDto);


            var consumerAccountRequest = new List<ETLConsumerAccountModel>();
            _jobReportService.JobResultDetails.Files.Add(secureFileTransferRequestDto.SourceFileName);

            var dataStream = new MemoryStream(response);
            using (StreamReader reader = new StreamReader(dataStream))
            {
                string line;
                int recordNbr = -1;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    try
                    {
                        var modelObject = _fISFlatFileRecordDtoFactory.CreateFisRecordInstance(line);
                        //30 Record type file process
                        if (modelObject.GetType() == typeof(FISCardHolderDataDto))
                        {
                            _jobReportService.JobResultDetails.RecordsReceived++;
                            recordNbr++;
                            var consumerAccount = await _recordType30ProcessService.Process30RecordFile(line,
                                (FISCardHolderDataDto)modelObject, etlExecutionContext);
                            if (consumerAccount != null && consumerAccount.CardIssueStatus != BenefitsConstants.EligibleCardIssueStatus)
                            {
                                consumerAccountRequest.Add(consumerAccount);
                                _jobReportService.JobResultDetails.RecordsProcessed++;
                                _jobReportService.keyRecordErrorMap.Add(consumerAccount.ConsumerCode!, new RecordError(recordNbr));
                            }
                            else if (consumerAccount != null && consumerAccount.CardIssueStatus == BenefitsConstants.EligibleCardIssueStatus)
                            {
                                consumerAccountRequest.Add(consumerAccount);
                                _jobReportService.JobResultDetails.RecordsErrorCount++;
                            }
                            else
                            {
                                _jobReportService.JobResultDetails.RecordsErrorCount++;
                            }

                        }
                        //60 Record type file process
                        else if (modelObject.GetType() == typeof(FISCardAdditionalDisbursementRecordDto))
                        {
                            _jobReportService.JobResultDetails.RecordsReceived++;
                            recordNbr++;
                            await _recordType60ProcessService.Process60RecordFile(line, (FISCardAdditionalDisbursementRecordDto)modelObject, etlExecutionContext);
                            _jobReportService.JobResultDetails.RecordsProcessed++;
                            _jobReportService.JobResultDetails.RecordsSuccessCount++;
                        }
                    }
                    catch (Exception ex)
                    {

                        _logger.LogError(ex, $"CardBatchFileReadAsync: Error Occured while reading Card Batch File: {ex.Message}, for data {secureFileTransferRequestDto.ToJson()}");
                        _jobReportService.JobResultDetails.RecordsErrorCount++;

                        _jobReportService.CollectError(recordNbr,400,null,ex);
                    }
                }
                if (consumerAccountRequest.Count > 0)
                {

                    var result = await _recordType30ProcessService.CreateConsumerAccount(consumerAccountRequest);
                    _jobReportService.JobResultDetails.RecordsErrorCount = _jobReportService.JobResultDetails.RecordsErrorCount + result.ErrorRecords.Count;
                    _jobReportService.JobResultDetails.RecordsSuccessCount = result.SuccessRecords.Count;

                    foreach (var record in result.ErrorRecords)
                    {
                        var errorByKey = _jobReportService.keyRecordErrorMap[record.ConsumerCode!];
                          _jobReportService.CollectError(errorByKey.RecordNbr , 400, errorByKey.ErrorMessage ,null);
                    }

                }

            }
            // Save Batch job report and Save job Detail Report
            await _jobReportService.SaveEtlErrors();

            //set job history status
            etlExecutionContext = _jobReportService.SetJobHistoryStatus(etlExecutionContext);


            #region Move file from inbound to archive folder after processing
            var sourceFileFullPath = secureFileTransferRequestDto.SourceFolderName != null
                        ? $"{secureFileTransferRequestDto.SourceFolderName.TrimEnd('/')}/{secureFileTransferRequestDto.SourceFileName}"
                        : secureFileTransferRequestDto.SourceFileName;

            var archiveFileFullPath = secureFileTransferRequestDto.SourceFolderName != null
                        ? $"{secureFileTransferRequestDto.InboundArchiveFolderName.TrimEnd('/')}/{secureFileTransferRequestDto.SourceFileName}"
                        : $"{FISBatchConstants.ARCHIVE_FOLDER_NAME}/{secureFileTransferRequestDto.SourceFileName}";

            await _s3Helper.MoveFileToFolder(secureFileTransferRequestDto.SourceBucketName, sourceFileFullPath,
                secureFileTransferRequestDto.ArchiveBucketName, archiveFileFullPath);

            #endregion
        }
    }
}
