using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Cohort.Core.Domain.Models;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Globalization;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class CohortConsumerService : ICohortConsumerService
    {
        private readonly ILogger<CohortConsumerService> _logger;
        private readonly IJobReportService _jobReportService;
        private readonly ICohortRepo _cohortRepo;
        private readonly ICohortConsumerRepo _cohortConsumerRepo;
        private readonly ITenantRepo _tenantRepo;
        private readonly IMapper _mapper;
        private const string _className = nameof(MemberImportService);

        public CohortConsumerService(ILogger<CohortConsumerService> logger, IJobReportService jobReportService,
            ICohortRepo cohortRepo, ICohortConsumerRepo cohortConsumerRepo, ITenantRepo tenantRepo, IMapper mapper)
        {
            _logger = logger;
            _jobReportService = jobReportService;
            _cohortRepo = cohortRepo;
            _cohortConsumerRepo = cohortConsumerRepo;
            _tenantRepo = tenantRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Imports cohort consumer data from a specified file path or file contents.
        /// </summary>
        /// <param name="etlExecutionContext">The execution context containing import file details.</param>
        public async Task Import(EtlExecutionContext etlExecutionContext)
        {
            const string MethodName = nameof(Import);
            LogInfo(MethodName, $"Started processing for CohortConsumerImportFilePath: {etlExecutionContext.CohortConsumerImportFilePath}");

            try
            {
                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == etlExecutionContext.TenantCode && x.DeleteNbr == 0);
                if (tenant == null)
                {
                    _logger.LogError($"{_className}.{MethodName}: Invalid tenant code: {etlExecutionContext.TenantCode}, Error Code:{StatusCodes.Status404NotFound}");
                    return;
                }
                _jobReportService.BatchJobRecords.JobType = nameof(CohortConsumerService);
                string cohortConsumerImportFilePath = etlExecutionContext.CohortConsumerImportFilePath;
                var cohortConsumerImportFileContents = etlExecutionContext.CohortConsumerImportFileContents;
                string fileName = Path.GetFileName(cohortConsumerImportFilePath);
                _jobReportService.JobResultDetails.Files.Add(fileName);

                if (string.IsNullOrEmpty(cohortConsumerImportFilePath) && (cohortConsumerImportFileContents == null || cohortConsumerImportFileContents.Length == 0))
                {
                    _logger.LogError($"{_className}.{MethodName} Cohort consumers import file path or S3 file content needs to be provided");
                    return;
                }

                using var cohortConsumerImportFileReader = GetFileReader(cohortConsumerImportFilePath, cohortConsumerImportFileContents);
                var csvConfiguration = GetCsvConfiguration();

                using var csvReader = new CsvReader(cohortConsumerImportFileReader, csvConfiguration);
                await ProcessImportAsync(csvReader, etlExecutionContext);

                LogInfo(MethodName, "Completed processing.");
            }
            catch (Exception ex)
            {
                HandleException(MethodName, ex);
                throw;
            }
        }

        /// <summary>
        /// Gets a StreamReader for the import file, either from a file path or file contents.
        /// </summary>
        /// <param name="filePath">The file path of the import file.</param>
        /// <param name="fileContents">The byte array of the file contents.</param>
        /// <returns>A StreamReader for the import file.</returns>
        private StreamReader GetFileReader(string filePath, byte[] fileContents)
        {
            return fileContents?.Length > 0
                ? new StreamReader(new MemoryStream(fileContents))
                : new StreamReader(filePath);
        }

        /// <summary>
        /// Configures the CSV reader settings.
        /// </summary>
        /// <returns>A CsvConfiguration object with the necessary settings.</returns>
        private CsvConfiguration GetCsvConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "\t",
                HeaderValidated = args =>
                {
                    if (args.InvalidHeaders.Any())
                    {
                        var missingHeaders = string.Join(", ", args.InvalidHeaders.SelectMany(header => header.Names));
                        throw new HeaderValidationException(args.Context, args.InvalidHeaders,
                            $"Header validation failed. Missing or invalid headers: {missingHeaders}");
                    }
                }
            };
        }

        /// <summary>
        /// Processes the import of cohort consumer data from the CSV reader.
        /// </summary>
        /// <param name="csvReader">The CsvReader to read the import data.</param>
        /// <param name="etlExecutionContext">The execution context containing import details.</param>
        private async Task ProcessImportAsync(CsvReader csvReader, EtlExecutionContext etlExecutionContext)
        {
            const string MethodName = nameof(ProcessImportAsync);
            LogInfo(MethodName, "Started processing import.");

            int recordNbr = 0;
            while (await csvReader.ReadAsync())
            {
                try
                {
                    _jobReportService.JobResultDetails.RecordsReceived++;
                    recordNbr++;

                    var record = csvReader.GetRecord<CohortConsumerImportCSVDto>();
                    if (record == null || !IsValidRecord(recordNbr, record)) continue;

                    var cohort = await _cohortRepo.FindOneAsync(x => x.CohortCode == record.CohortCode && x.DeleteNbr == 0);
                    if (cohort == null)
                    {
                        ReportError(recordNbr, 400, $"Cohort not found: {record.CohortCode}");
                        continue;
                    }

                    var cohortConsumer = await FindExistingCohortConsumer(record, cohort, etlExecutionContext);
                    if (cohortConsumer != null)
                    {
                        ReportError(recordNbr, 400, $"CohortConsumer already exists: CohortCode={record.CohortCode}, ConsumerCode={record.ConsumerCode}");
                        continue;
                    }

                    await CreateCohortConsumer(record, cohort, etlExecutionContext, recordNbr);
                }
                catch (Exception ex)
                {
                    HandleException(MethodName, ex, recordNbr);
                }
            }

            LogInfo(MethodName, "Finished processing.");
        }

        /// <summary>
        /// Finds an existing cohort consumer based on the provided record and context.
        /// </summary>
        /// <param name="record">The CSV record containing cohort consumer data.</param>
        /// <param name="cohort">The cohort model associated with the record.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>An existing ETLCohortConsumerModel if found, otherwise null.</returns>
        private async Task<ETLCohortConsumerModel> FindExistingCohortConsumer(CohortConsumerImportCSVDto record, ETLCohortModel cohort, EtlExecutionContext context)
        {
            return await _cohortConsumerRepo.FindOneAsync(x =>
                x.ConsumerCode == record.ConsumerCode &&
                x.CohortId == cohort.CohortId &&
                x.TenantCode == context.TenantCode &&
                x.DeleteNbr == 0);
        }

        /// <summary>
        /// Creates a new cohort consumer based on the provided record and context.
        /// </summary>
        /// <param name="record">The CSV record containing cohort consumer data.</param>
        /// <param name="cohort">The cohort model associated with the record.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="recordNbr">The record number being processed.</param>
        private async Task CreateCohortConsumer(CohortConsumerImportCSVDto record, ETLCohortModel cohort, EtlExecutionContext context, int recordNbr)
        {
            var cohortConsumer = new ETLCohortConsumerModel
            {
                CohortId = cohort.CohortId,
                ConsumerCode = record.ConsumerCode,
                TenantCode = context.TenantCode,
                CohortDetectDescription = record.DetectDescription ?? string.Empty,
                CreateUser = Constants.CreateUserAsETL,
                CreateTs = DateTime.UtcNow,
                DeleteNbr = 0
            };

            var createdCohortConsumer = await _cohortConsumerRepo.CreateAsync(cohortConsumer);
            if (createdCohortConsumer.CohortConsumerId > 0)
            {
                LogInfo(nameof(ProcessImportAsync), $"CohortConsumer created successfully for CohortCode={record.CohortCode}, ConsumerCode={record.ConsumerCode}");
                _jobReportService.JobResultDetails.RecordsProcessed++;
                _jobReportService.JobResultDetails.RecordsSuccessCount++;
            }
        }

        /// <summary>
        /// Validates the CSV record.
        /// </summary>
        /// <param name="recordNbr">The record number being processed.</param>
        /// <param name="record">The CSV record to validate.</param>
        /// <returns>True if the record is valid, otherwise false.</returns>
        private bool IsValidRecord(int recordNbr, CohortConsumerImportCSVDto record)
        {
            var validationMessage = ValidateRecord(recordNbr, record);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                LogInfo(nameof(ProcessImportAsync), validationMessage);
                ReportError(recordNbr, 400, validationMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates the CSV record to ensure required fields are present.
        /// </summary>
        /// <param name="recordNbr">The record number being processed.</param>
        /// <param name="record">The CSV record to validate.</param>
        /// <returns>A string containing the validation error message, if any.</returns>
        private string ValidateRecord(int recordNbr, CohortConsumerImportCSVDto record)
        {
            if (string.IsNullOrWhiteSpace(record.CohortCode))
            {
                return $"CohortCode is missing for record {recordNbr}.";
            }

            if (string.IsNullOrWhiteSpace(record.ConsumerCode))
            {
                return $"ConsumerCode is missing for record {recordNbr}.";
            }

            return string.Empty;
        }


        /// <summary>
        /// Reports an error encountered during processing.
        /// </summary>
        /// <param name="recordNbr">The record number where the error occurred.</param>
        /// <param name="errorCode">The error code associated with the error.</param>
        /// <param name="message">The error message to report.</param>
        private void ReportError(int recordNbr, int errorCode, string message)
        {
            _jobReportService.CollectError(recordNbr, errorCode, message, null);
            _jobReportService.JobResultDetails.RecordsErrorCount++;
            _jobReportService.JobResultDetails.RecordsProcessed++;
        }

        /// <summary>
        /// Handles exceptions that occur during processing.
        /// </summary>
        /// <param name="methodName">The name of the method where the exception occurred.</param>
        /// <param name="ex">The exception that was thrown.</param>
        /// <param name="recordNbr">The record number being processed, if applicable.</param>
        private void HandleException(string methodName, Exception ex, int recordNbr = -1)
        {
            var errorMessage = $"{methodName}: Error during processing. {ex.Message}";
            _logger.LogError(ex, "{_className}.{MethodName} - ErrorCode: {Code}, Error: {Msg}",
                _className, methodName, StatusCodes.Status500InternalServerError, ex.Message);

            if (recordNbr >= 0)
            {
                ReportError(recordNbr, StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        /// <summary>
        /// Logs informational messages.
        /// </summary>
        /// <param name="methodName">The name of the method generating the log message.</param>
        /// <param name="message">The informational message to log.</param>
        private void LogInfo(string methodName, string message)
        {
            _logger.LogInformation("{_className}.{MethodName} - {Message}", _className, methodName, message);
        }

        /// <summary>
        /// Adds a consumer to a cohort.
        /// </summary>
        /// <param name="cohortConsumerRequestDto"></param>
        /// <returns></returns>
        public async Task<ETLCohortConsumerModel> AddConsumerToCohort(CohortConsumerRequestDto cohortConsumerRequestDto)
        {
            const string methodName = nameof(AddConsumerToCohort);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing Add consumer to Cohort with TenantCode:{TenantCode},ConsumerCode:{ConsumerCode}", _className, methodName,
                    cohortConsumerRequestDto.TenantCode, cohortConsumerRequestDto.ConsumerCode);
                var cohort = await _cohortRepo.FindOneAsync(x => x.CohortName == cohortConsumerRequestDto.CohortName && x.DeleteNbr == 0);
                if (cohort == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Cohort not found with CohortName:{Name}", _className, methodName, cohortConsumerRequestDto.CohortName);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Cohort Not Found with Cohort Name:{cohortConsumerRequestDto.CohortName}");
                }
                var cohortConsumer = await _cohortConsumerRepo.FindOneAsync(x => x.CohortId == cohort.CohortId && x.TenantCode == cohortConsumerRequestDto.TenantCode && x.ConsumerCode == cohortConsumerRequestDto.ConsumerCode && x.DeleteNbr == 0);
                if (cohortConsumer != null)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - CohortConsumer Already exist with, CohortName:{Name}, Tenant:{Tenant}, Consumer:{Consumer}", _className, methodName, cohortConsumerRequestDto.CohortName, cohortConsumerRequestDto.TenantCode, cohortConsumerRequestDto.ConsumerCode);
                    return cohortConsumer;
                }

                var cohortConsumerModel = _mapper.Map<ETLCohortConsumerModel>(cohortConsumerRequestDto);
                cohortConsumerModel.CohortId = cohort.CohortId;
                cohortConsumerModel.CreateTs = DateTime.UtcNow;
                cohortConsumerModel.DeleteNbr = 0;
                cohortConsumerModel.CreateUser = "SYSTEM";

                await _cohortConsumerRepo.CreateAsync(cohortConsumerModel);
                _logger.LogInformation("{ClassName}.{MethodName} - Cohort Consumer Added Successfully, with TenantCode:{TenantCode},ConsumerCode:{ConsumerCode}", _className, methodName, cohortConsumerRequestDto.TenantCode, cohortConsumerRequestDto.ConsumerCode);
                return cohortConsumerModel;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Error occurred while creating cohortConsumer. ErrorMessage:{ErrorMessage}", _className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Detach consumer from cohort.
        /// </summary>
        /// <param name="cohortConsumerRequestDto"></param>
        /// <returns></returns>
        public async Task<ETLCohortConsumerModel> RemoveConsumerToCohort(CohortConsumerRequestDto cohortConsumerRequestDto)
        {
            const string methodName = nameof(RemoveConsumerToCohort);
            string tenantCode = cohortConsumerRequestDto.TenantCode;
            string consumerCode = cohortConsumerRequestDto.ConsumerCode;
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing remove consumer with ConsumerCode:{Code},TenantCode:{TenantCode}", _className, methodName,
                    consumerCode, tenantCode);

                var cohort = await _cohortRepo.FindOneAsync(x => x.CohortName == cohortConsumerRequestDto.CohortName && x.DeleteNbr == 0);
                if (cohort == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Cohort not found with CohortName:{Name}", _className, methodName, cohortConsumerRequestDto.CohortName);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, $"Cohort Not Found with Cohort Name:{cohortConsumerRequestDto.CohortName}");
                }

                var cohortConsumer = await _cohortConsumerRepo.FindOneAsync(x => x.CohortId == cohort.CohortId && x.TenantCode == tenantCode && x.ConsumerCode == consumerCode && x.DeleteNbr == 0);
                if (cohortConsumer == null)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Cohort Consumer not found with ConsumerCode:{Code},TenantCode:{TenantCode}", _className, methodName,
                        tenantCode, consumerCode);
                    return new ETLCohortConsumerModel();
                }
                cohortConsumer.DeleteNbr = cohortConsumer.CohortConsumerId;

                await _cohortConsumerRepo.UpdateAsync(cohortConsumer);

                _logger.LogInformation("{ClassName}.{MethodName} -  Consumer removed successfully with ConsumerCode:{Code},TenantCode:{TenantCode}", _className, methodName,
                    consumerCode, tenantCode);
                return cohortConsumer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing remove consumer with ConsumerCode:{Code},TenantCode:{TenantCode},ERROR:{Msg}", _className, methodName,
                    tenantCode, consumerCode, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get cohort consumer task.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerCode"></param>
        /// <param name="cohortName"></param>
        /// <returns></returns>
        public async Task<List<CohortConsumerTaskDto>> GetCohortConsumerTask(string tenantCode, string consumerCode, string cohortName)
        {
            var query = _cohortConsumerRepo.GetCohortConsumerTask(tenantCode, consumerCode, cohortName);
            return await query.ToListAsync();
        }
    }
}
