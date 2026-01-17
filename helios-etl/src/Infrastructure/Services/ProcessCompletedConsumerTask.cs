extern alias SunnyRewards_Task;

using Amazon.S3.Model;
using Amazon.S3;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using System.Security.Principal;
using System.Text;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using Microsoft.Extensions.Configuration;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers;
using Amazon;
using SunnyRewards_Task::SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System.Globalization;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class ProcessCompletedConsumerTask : AwsConfiguration, IProcessCompletedConsumerTask
    {
        private readonly IAdminClient _adminClient;
        private readonly ILogger<ProcessCompletedConsumerTask> _logger;
        private readonly IVault _vault;
        private readonly IConfiguration _configuration;

        private const string className = nameof(ProcessCompletedConsumerTask);

        public ProcessCompletedConsumerTask(ILogger<ProcessCompletedConsumerTask> logger,
            IAdminClient adminClient , IVault vault, IConfiguration configuration) : base(vault, configuration)
        {
            _vault = vault;
            _configuration = configuration;
            _logger = logger;
            _adminClient = adminClient;
        }

        public async Task ProcessCompletedConsumerTasksAsync(EtlExecutionContext context)
        {
            const string MethodName = nameof(ProcessCompletedConsumerTasksAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Extracting Consumer Details for taskId : {TaskId} , TenantCode :  {TenantCode}.",
                className, MethodName, context.TaskId, context.TenantCode);

            try
            {

                var tenants = await GetTenantCodesAsync(context.TenantCode);
                if (tenants == null || tenants.Count == 0)
                {
                    LogAndThrowInvalidTenantCode(MethodName, context.TenantCode);
                }

                var allowedFormats = new[] { "yyyy-MM-dd" };

                if (!DateTime.TryParseExact(context.StartDate, allowedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invalid StartDate, supported formats is yyyy-mm-dd: {StartDate}", className, MethodName, context.StartDate);
                    return;
                }

                DateTime endDate;

                if (!string.IsNullOrWhiteSpace(context.EndDate))
                {
                    if (!DateTime.TryParseExact(context.EndDate, allowedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEndDate))
                    {
                        _logger.LogError("{ClassName}.{MethodName} - Invalid EndDate, supported formats is yyyy-mm-dd: {EndDate}", className, MethodName, context.EndDate);
                        return;
                    }

                    endDate = parsedEndDate.Date == startDate.Date
                        ? startDate.AddDays(1).AddTicks(-1)
                        : parsedEndDate;
                }
                else
                {
                    endDate = startDate.AddDays(1).AddTicks(-1);
                }


                if (endDate < startDate)
                {
                    _logger.LogError("{ClassName}.{MethodName} - EndDate : {EndDate} can not be greater than StartDate: {StartDate}", className, MethodName, startDate.ToShortDateString() , endDate.ToShortDateString());
                    return;
                }

                const int batchSize = 500;
                int totalProcessed = 0;
                int totalRecords = 0;

                var request = new GetConsumerTaskByTaskId
                {
                    TaskId = context.TaskId,
                    TenantCode = context.TenantCode,
                    StartDate = startDate.Date,
                    EndDate = endDate,
                    Skip = 0,
                    PageSize = batchSize
                };

                bool isFirstBatch = true;
                var fileName = $"outbound/{context.TenantCode}/{context.TaskId}/consumer_details_completed_task({context.TaskId})_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
                do
                {
                    var response = await FetchConsumersDetailsWithTaskAsync(request);
                    if(response.ErrorCode != null && response.ErrorCode != StatusCodes.Status404NotFound)
                    {
                        _logger.LogError("Error while fetching consumer details, ErrorCode : {ErrorCode}, Message: {Message}", response.ErrorCode, response.ErrorMessage);
                        throw new ETLException(ETLExceptionCodes.ErrorFromAPI, response.ErrorMessage?? "Error while fetching consumer details");
                    }

                    if (response?.consumerwithTask == null || response.consumerwithTask.Count == 0)
                    {
                        _logger.LogInformation("no active consumer in batchsize.continue..");
                        totalProcessed += batchSize;
                        request.Skip = totalProcessed;
                        continue; 
                    }

                    if (totalRecords == 0)
                        totalRecords = response.totalconsumersTasks;

                    await HandleConsumerTaskBatchAsync(fileName,response, isFirstBatch);
                    totalProcessed += batchSize;
                    request.Skip = totalProcessed;
                    isFirstBatch = false;

                } while (totalProcessed < totalRecords);
                _logger.LogInformation("Consumer Task file generated successfully for TaskId :{TaskId}", request.TaskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Unexpected error while extracting Consumer Details and Task", className, MethodName);
                throw;
            }
        }

        private async Task HandleConsumerTaskBatchAsync(String fileName , ConsumersByTaskIdResponseDto response, bool isFirstChunk)
        {
            var records = ConvertApiResponseToRecords(response);
            await WriteRecordsToCsvAndUploadAsync(records, fileName, isFirstChunk);
        }

        private List<ConsumerPersonConsumerTaskRecord> ConvertApiResponseToRecords(ConsumersByTaskIdResponseDto apiData)
        {
            var result = new List<ConsumerPersonConsumerTaskRecord>();

            foreach (var item in apiData.consumerwithTask)
            {
                foreach (var task in item.ConsumerTasks)
                {
                    var record = new ConsumerPersonConsumerTaskRecord
                    {
                        // Consumer fields
                        ConsumerId = item.Consumer.ConsumerId,
                        PersonId = item.Consumer.PersonId,
                        TenantCode = item.Consumer.TenantCode,
                        ConsumerCode = item.Consumer.ConsumerCode,
                        RegistrationTs = item.Consumer.RegistrationTs,
                        EligibleStartTs = item.Consumer.EligibleStartTs,
                        EligibleEndTs = item.Consumer.EligibleEndTs,
                        Registered = item.Consumer.Registered,
                        Eligible = item.Consumer.Eligible,
                        MemberNbr = item.Consumer.MemberNbr,
                        SubscriberMemberNbr = item.Consumer.SubscriberMemberNbr,
                        ConsumerAttribute = item.Consumer.ConsumerAttribute,
                        AnonymousCode = item.Consumer.AnonymousCode,
                        SubscriberOnly = item.Consumer.SubscriberOnly,
                        IsSSOUser = item.Consumer.IsSSOUser,
                        EnrollmentStatus = item.Consumer.EnrollmentStatus,
                        EnrollmentStatusSource = item.Consumer.EnrollmentStatusSource,
                        OnBoardingState = item.Consumer.OnBoardingState,
                        AgreementStatus = item.Consumer.AgreementStatus,

                        // Person fields
                        PersonCode = item.Person.PersonCode,
                        FirstName = item.Person.FirstName,
                        LastName = item.Person.LastName,
                        LanguageCode = item.Person.LanguageCode,
                        MemberSince = item.Person.MemberSince,
                        Email = item.Person.Email,
                        City = item.Person.City,
                        Country = item.Person.Country,
                        YearOfBirth = item.Person.YearOfBirth,
                        PostalCode = item.Person.PostalCode,
                        PhoneNumber = item.Person.PhoneNumber,
                        Region = item.Person.Region,
                        DOB = item.Person.DOB,
                        Gender = item.Person.Gender,
                        IsSpouse = item.Person.IsSpouse,
                        IsDependent = item.Person.IsDependent,
                        SSNLast4 = item.Person.SSNLast4,
                        MailingAddressLine1 = item.Person.MailingAddressLine1,
                        MailingAddressLine2 = item.Person.MailingAddressLine2,
                        MailingState = item.Person.MailingState,
                        MailingCountryCode = item.Person.MailingCountryCode,
                        HomePhoneNumber = item.Person.HomePhoneNumber,
                        SyncRequired = item.Person.SyncRequired,
                        SyncOptions = item.Person.SyncOptions,
                        SyntheticUser = item.Person.SyntheticUser,

                        // Task fields
                        ConsumerTaskId = task.ConsumerTaskId,
                        TaskId = task.TaskId,
                        TaskStatus = task.TaskStatus,
                        Progress = task.Progress,
                        Notes = task.Notes,
                        TaskStartTs = task.TaskStartTs,
                        TaskCompleteTs = task.TaskCompleteTs,
                        AutoEnrolled = task.AutoEnrolled,
                        ProgressDetail = task.ProgressDetail,
                        ParentConsumerTaskId = task.ParentConsumerTaskId,
                        CreateTs = task.CreateTs,
                        WalletTransactionCode = task.WalletTransactionCode,
                        RewardInfoJson = task.RewardInfoJson 
                    };

                    result.Add(record);
                }
            }

            return result;
        }

        private async Task WriteRecordsToCsvAndUploadAsync(List<ConsumerPersonConsumerTaskRecord> records, string fileName, bool isFirstChunk)
        {
            const string MethodName = nameof(WriteRecordsToCsvAndUploadAsync);
            var bucketName = GetAwsConsumerBucketName();

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            if (isFirstChunk)
                await csv.WriteRecordsAsync(records);
            else
                foreach (var record in records)
                {
                    csv.WriteRecord(record);
                    csv.NextRecord();
                }

            await writer.FlushAsync();
            memoryStream.Position = 0;

            try
            {
                using var s3Client = new AmazonS3Client(await GetAwsAccessKey(), await GetAwsSecretKey(), RegionEndpoint.USEast2);

                if (!isFirstChunk)
                {
                    var existingObject = await s3Client.GetObjectAsync(bucketName, fileName);
                    using var combinedStream = new MemoryStream();
                    await existingObject.ResponseStream.CopyToAsync(combinedStream);
                    await memoryStream.CopyToAsync(combinedStream);
                    combinedStream.Position = 0;

                    await s3Client.PutObjectAsync(new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = fileName,
                        InputStream = combinedStream,
                        ContentType = "text/csv",
                        ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                    });
                }
                else
                {
                    await s3Client.PutObjectAsync(new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = fileName,
                        InputStream = memoryStream,
                        ContentType = "text/csv",
                        ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                    });
                }

                _logger.LogInformation("{ClassName}.{MethodName} - CSV uploaded to S3: {FileName}", className, MethodName, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Failed to upload file: {FileName}", className, MethodName, fileName);
                throw;
            }
        }

        private async Task<ConsumersByTaskIdResponseDto> FetchConsumersDetailsWithTaskAsync(GetConsumerTaskByTaskId request)
        {
            return await _adminClient.Post<ConsumersByTaskIdResponseDto>("consumers-completing-task-in-range", request);
        }


        private async Task<List<TenantDto>?> GetTenantCodesAsync(string tenantCode)
        {
            if (string.IsNullOrWhiteSpace(tenantCode))
                return new List<TenantDto>();

            if (tenantCode.Trim().Equals(NotificationConstants.TenantCodesAll, StringComparison.OrdinalIgnoreCase))
                return await GetAllTenantsAsync();

            var tenantCodes = tenantCode
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrEmpty(code))
                .ToList();

            return await GetTenantsByTenantCodesAsync(tenantCodes);
        }

        private async Task<List<TenantDto>> GetAllTenantsAsync()
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            var response = await _adminClient.Get<TenantsResponseDto>(AdminConstants.GetAllTenantsAPIUrl, parameters);
            if (response?.ErrorCode != null)
            {
                _logger.LogError("{ClassName}.{MethodName}: API - Error occurred while processing all Tenants, Error Code: {ErrorCode}, Error Message: {Message}", className, nameof(GetAllTenantsAsync), response.ErrorCode, response.ErrorMessage);
                return new List<TenantDto>();
            }
            return response?.Tenants ?? new List<TenantDto>();

        }

        private async Task<List<TenantDto>?> GetTenantsByTenantCodesAsync(List<string>? tenantCodes)
        {
            var tenantList = new List<TenantDto>();
            if (tenantCodes == null || tenantCodes.Count == 0)
            {
                return tenantList;
            }
            foreach (var tenatCode in tenantCodes)
            {
                var tenant = await GetTenantDetails(tenatCode);
                if (tenant != null)
                {
                    tenantList.Add(tenant);
                }
            }
            if (tenantCodes.Count > 0 && tenantList.Count == 0)
            {
                throw new ETLException(ETLExceptionCodes.NullValue,
             $"Invalid tenant code(s) in job params: {string.Join(", ", tenantCodes)}. ErrorCode: {StatusCodes.Status500InternalServerError}");
            }

            return tenantList;

        }

        private async Task<TenantDto?> GetTenantDetails(string tenantCode)
        {
            var methodName = nameof(GetTenantDetails);
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            var response = await _adminClient.Get<TenantResponseDto>($"{AdminConstants.GetTenant}?tenantCode={tenantCode}", parameters);
            if (response?.ErrorCode != null)
            {
                _logger.LogError("{ClassName}.{MethodName}: API - Error occurred while fetching tenant details, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}", className, methodName, response.ErrorCode, response.ErrorMessage);
                return null;
            }
            return response?.Tenant?.TenantId > 0 ? response.Tenant : null;
        }

        private void LogAndThrowInvalidTenantCode(string methodName, string? tenantCode)
        {
            _logger.LogError("{ClassName}.{MethodName} - Invalid tenant code in job params: {TenantCode}. ErrorCode: {Code}",
                className, methodName, tenantCode, StatusCodes.Status500InternalServerError);

            throw new ETLException(ETLExceptionCodes.NullValue, $"Invalid tenant code in job params: {tenantCode}. ErrorCode: {StatusCodes.Status500InternalServerError}");
        }
    }
}