using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using AutoMapper;
using Newtonsoft.Json.Serialization;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using Microsoft.Extensions.Configuration;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.ETL.Common.Constants;
using Microsoft.IdentityModel.Tokens;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class JobReportService : IJobReportService
    {
        private List<JobReportDetailRecord> _errorRecords;
        private BatchJobRecordsDto _batchJobRecords;
        private JobResultDetails _jobResultDetails;
        private IAwsQueueService _awsQueueService;
        private readonly IMapper _mapper;
        private readonly IVault _vault;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, RecordError> _keyRecordErrorMap;
        private readonly ILogger<JobReportService> _logger;
        private readonly IAdminClient _adminClient;
        public JobReportService(ILogger<JobReportService> logger, IMapper mapper,
            IAwsQueueService awsQueueService, IConfiguration configuration, IVault vault
            , IAdminClient adminClient)
        {
            _logger = logger;
            _errorRecords = new List<JobReportDetailRecord>();
            _batchJobRecords = new BatchJobRecordsDto();
            _jobResultDetails = new JobResultDetails();
            _keyRecordErrorMap = new Dictionary<string, RecordError>();
            _mapper = mapper;
            _awsQueueService = awsQueueService;
            _configuration = configuration;
            _vault = vault;
            _adminClient = adminClient;
        }

        public Dictionary<string, RecordError> keyRecordErrorMap => _keyRecordErrorMap;
        public JobResultDetails JobResultDetails => _jobResultDetails;
        public BatchJobRecordsDto BatchJobRecords => _batchJobRecords;

        /// <summary>
        /// Collects error information and adds it to the error record list.
        /// </summary>
        /// <param name="recordNbr">The record number associated with the error.</param>
        /// <param name="errorNbr">The error number.</param>
        /// <param name="message">Optional message describing the error.</param>
        /// <param name="ex">Optional exception object.</param>
        public void CollectError(int recordNbr, int errorNbr = 400, string? message = null, Exception? ex = null)
        {
            try
            {
                var errorMsg = message ?? ex?.Message ?? "Unknown error occurred";
                var stackTrace = ex?.StackTrace ?? "Stack trace not available";

                var jobReportDetailData = new JobReportDetailRecord(_jobResultDetails.Files.Count - 1, recordNbr)
                {
                    ErrorDetails = new EtlErrorDetailRecord(errorNbr, errorMsg, stackTrace)
                };

                _errorRecords.Add(jobReportDetailData);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "JobReportService : Error in Collecting JobError");
            }
        }

        /// <summary>
        /// Saves the error details to the job detail report repository.
        /// </summary>
        /// /// <param name="jobReportId">
        private async Task<bool> SaveErrorDetails(long jobReportId)
        {
            if (jobReportId < 0)
            {
                _logger.LogError("JobReportService : Invalid Job Report Id {JobReportID}", jobReportId);
                return false;
            }
            try
            {
                var errorListToSave = new BatchJobDetailReportRequestDto();
                errorListToSave.BatchJobDetailReportDtos = new List<BatchJobDetailReportDto>();
                foreach (var record in _errorRecords)
                {
                    var batchJobDetailReportDto = new BatchJobDetailReportDto
                    {
                        BatchJobReportId = jobReportId,
                        FileNum = record.FileNbr,
                        RecordNum = record.RecordNbr,
                        RecordResultJson = JsonConvert.SerializeObject(record.ErrorDetails, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        })
                    };

                    errorListToSave.BatchJobDetailReportDtos.Add(batchJobDetailReportDto);
                }

                var jobReport = await _adminClient.Post<BatchJobDetailReportResponseDto>(Constants.JobDetailReport, errorListToSave);

                if (jobReport != null)
                {
                    return true;
                }
                else
                {
                    _logger.LogError("JobReportService : Job Detail Report not saved Report Id:  {JobReportID}", jobReportId);
                    return false;
                }
            }
            catch (Exception)
            {
                _logger.LogError("Error in saving Details logs for Batch Id {jobReportId}", jobReportId);
                return false;
            }



        }

        // <summary>
        /// Saves ETL errors by mapping DTO to the model and saving batch job records.
        /// </summary>
        /// <returns>A boolean indicating success or failure.</returns>
        public async Task<bool> SaveEtlErrors(string? filePath="")
        {
            try
            {
                _batchJobRecords.SetJobResultDetails(_jobResultDetails);
                var jobReportData = new BatchJobRecordsDto()
                {
                    JobResultJson = _batchJobRecords.JobResultJson,
                    JobType = _batchJobRecords.JobType,
                    ValidationJson = _batchJobRecords.ValidationJson,
                };

                var jobReport = await _adminClient.Post<BatchJobReportResponseDto>(Constants.JobReport, jobReportData);

                if (jobReport != null && jobReport.jobReport != null)
                {
                    await SaveErrorDetails(jobReport.jobReport.BatchJobReportId);
                    await PushToMessageQueue(jobReport.jobReport.BatchJobReportCode, filePath);
                    return true;
                }
                else
                {
                    _logger.LogError("JobReportService :  Job Records not saved Rerquest:  {request}", jobReportData.ToJson());
                    return false;
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ETL Batch Job Records failed to save records for {0}", _batchJobRecords.JobType);
                return false;
            }
        }

        public EtlExecutionContext SetJobHistoryStatus(EtlExecutionContext etlExecutionContext)
        {
            //set job history status
            etlExecutionContext.JobHistoryStatus = JobResultDetails.RecordsErrorCount == 0
                ? Constants.JOB_HISTORY_SUCCESS_STATUS
                : (JobResultDetails.RecordsErrorCount == JobResultDetails.RecordsReceived
                    ? Constants.JOB_HISTORY_FAILURE_STATUS
                    : Constants.JOB_HISTORY_PARTIAL_SUCCESS_STATUS);
            etlExecutionContext.JobHistoryErrorLog = JobResultDetails.RecordsErrorCount != 0
                ? etlExecutionContext.JobHistoryErrorLog + $"Errored records count: {JobResultDetails.RecordsErrorCount}"
                : etlExecutionContext.JobHistoryErrorLog;

            return etlExecutionContext;
        }

        private async Task PushToMessageQueue(string batchJobReportCode, string? filePath="")
        {
            try
            {
                var requestDto = new ETLBatchJobRecordQueueRequestDto(_vault, _configuration);
                await requestDto.InitializeAsync();
                requestDto.JobType = _batchJobRecords.JobType!;
                requestDto.AdminPanelLink = String.Format(requestDto.AdminPanelLink ?? string.Empty,
                                                    batchJobReportCode);
                if (!filePath.IsNullOrEmpty())
                    requestDto.FilePath = filePath;
                var queueMsg = await _awsQueueService.PushToBatchJobRecordQueue(requestDto);
                string message = queueMsg.Item1 ? "successfully" : "failed";
                _logger.LogInformation($"Message push to Queue {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while pushing message to Queue");
                _logger.LogError($"Message push to Queue failed");
            }
        }
    }
}
