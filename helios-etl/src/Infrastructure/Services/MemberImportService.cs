using Amazon.S3;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.ClearScript.JavaScript;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NHibernate.Util;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Etl.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using System.Globalization;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ISecretHelper = SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces.ISecretHelper;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    /// <summary>
    /// Service for importing member data.
    /// </summary>
    public class MemberImportService : BasePldProcessor, IMemberImportService
    {
        private readonly ILogger<MemberImportService> _logger;
        private readonly ITenantRepo _tenantRepo;
        private readonly IPersonRepo _personRepo;
        private readonly IDataFeedClient _dataFeedClient;
        private readonly ISecretHelper _secretHelper;
        private readonly IJobReportService _jobReportService;
        private readonly IMemberImportFileDataService _memberImportFileDataService;
        private readonly IConsumerService _consumerService;
        private readonly IAwsNotificationService _awsNotificationService;
        private const string className = nameof(MemberImportService);
        private string _fileName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="session"></param>
        /// <param name="tenantRepo"></param>
        /// <param name="personRepo"></param>
        /// <param name="pldParser"></param>
        /// <param name="s3FileLogger"></param>
        /// <param name="dataFeedClient"></param>
        public MemberImportService(ILogger<MemberImportService> logger, NHibernate.ISession session, ITenantRepo tenantRepo, IPersonRepo personRepo, IMemberImportFileDataService memberImportFileDataService,
            IPldParser pldParser, IS3FileLogger s3FileLogger, IDataFeedClient dataFeedClient, ISecretHelper secretHelper, IJobReportService jobReportService, IConsumerService consumerService, 
            IAwsNotificationService awsNotificationService) : base(logger, session, pldParser, s3FileLogger)

        {
            _logger = logger;
            _tenantRepo = tenantRepo;
            _personRepo = personRepo;
            _dataFeedClient = dataFeedClient;
            _secretHelper = secretHelper;
            _jobReportService = jobReportService;
            _memberImportFileDataService = memberImportFileDataService;
            _consumerService = consumerService;
            _awsNotificationService = awsNotificationService;
        }

        /// <summary>
        /// Imports member data.
        /// </summary>
        /// <param name="etlExecutionContext">The ETL execution context.</param>
        /// <returns>A tuple containing lists of ETLConsumerModel and ETLPersonModel.</returns>
        public async Task<(List<ETLConsumerModel>, List<ETLPersonModel>)> Import(EtlExecutionContext etlExecutionContext)
        {
            const string _methodName = nameof(Import);
            _jobReportService.BatchJobRecords.JobType = nameof(MemberImportService);
            _logger.LogInformation("{ClassName}.{MethodName} - Stared processing for MemberImportFilePath: {MemberImportFilePath}, MemberImportFileContents: {MemberImportFileContents}",
                className, _methodName, etlExecutionContext.MemberFilePath, etlExecutionContext.MemberFileContents);

            ETLMemberImportFileModel? fileinImport = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(etlExecutionContext.MemberImportFilePath))
                {
                    // we have file to import
                    (long memberImportFileId, bool isDataSavedInStaging) = await _memberImportFileDataService.saveMemberImportFileData(etlExecutionContext); 
                }
               var filesToImport = await _memberImportFileDataService.GetMemberImportFilesToImport();

                var memberImportCSVDtoList = new List<MemberImportCSVDto>();
                var etlConsumers = new List<ETLConsumerModel>();
                var etlPersons = new List<ETLPersonModel>();
                var FilePaths = new StringBuilder();
                foreach (var file in filesToImport) {
                    fileinImport = file;
                    _jobReportService.JobResultDetails.Files.Add(file.FileName);
                    _fileName = Path.GetFileName(file.FileName);
                    await _memberImportFileDataService.updateFileStatus(file.MemberImportFileId, FileStatus.IN_PROGRESS);
                    _logger.LogInformation("Member Import file {filename} in progress", _fileName);
                    await ProcessImportAsync(file.MemberImportFileId, memberImportCSVDtoList, etlExecutionContext, etlConsumers, etlPersons);
                   var fileStatus =  await _memberImportFileDataService.updateFileStatus(file.MemberImportFileId , FileStatus.COMPLETED);
                    if (!fileStatus)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - failed to update file status for MemberImportFileId :{MemberImportFileId}.", className, _methodName, file.MemberImportFileId);
                    }
                    FilePaths.Append(await _memberImportFileDataService.CreateAndUploadCsv(file.FileName));
                    if (!file.Equals(filesToImport  .Last()))
                    {
                        FilePaths.Append(",");
                    }
                }

                if (filesToImport.Count > 0) // if something is there to import then save Errors
                {
                    if (etlExecutionContext.EnableMemberImport)// Single file is processed 
                    {
                        _jobReportService.BatchJobRecords.ValidationJson = _memberImportFileDataService.batchJobReportValidationJson.SetValidationJsonDetails(_memberImportFileDataService.batchJobReportValidationJson);
                        await _jobReportService.SaveEtlErrors(FilePaths.ToString());
                        etlExecutionContext = _jobReportService.SetJobHistoryStatus(etlExecutionContext);
                    }

                    if (etlExecutionContext.EnableS3) 
                    {
                        _jobReportService.BatchJobRecords.ValidationJson = _memberImportFileDataService.batchJobReportValidationJson.SetValidationJsonDetails(_memberImportFileDataService.batchJobReportValidationJson);
                        await _jobReportService.SaveEtlErrors(FilePaths.ToString());
                    }
                }
                _logger.LogInformation("{ClassName}.{MethodName} - Completed processing.", className, _methodName);

                return (etlConsumers, etlPersons);
            }
            catch (Exception ex)
            {
                if (fileinImport != null)
                {
                    await _memberImportFileDataService.updateFileStatus(fileinImport.MemberImportFileId, FileStatus.FAILED);
                }
                var msg = $"{_methodName}: Failed processing. Error: {ex.Message}. \n {ex}";
                _logger.LogError(ex, "{ClassName}.{MethodName} -{FileName} Failed processing Import,ErrorCode:{Code}, ERROR:{Msg}",
                 className, _methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }

        }

        /// <summary>
        /// Processes member data from the given file asynchronously.
        /// </summary>
        /// <param name="memberImportCSV"></param>
        /// <param name="memberImportCSVDtoList"></param>
        /// <param name="tenant"></param>
        /// <param name="etlExecutionContext"></param>
        /// <param name="etlConsumers"></param>
        /// <param name="etlPersons"></param>
        /// <returns></returns>
        private async Task ProcessImportAsync(long memberImportFileId, List<MemberImportCSVDto> memberImportCSVDtoList, EtlExecutionContext etlExecutionContext, List<ETLConsumerModel> etlConsumers, List<ETLPersonModel> etlPersons)
        {
            const string methodName = nameof(ProcessImportAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Stared processing Import..", className, methodName);

            var batchSize = Constants.DefaultBatchSize;
            var memberImportInsertList = new List<MemberImportCSVDto>();
            var memberImportUpdateList = new List<MemberImportCSVDto>();
            var memberImportDeleteList = new List<MemberImportCSVDto>();
            var memberImportCancelList = new List<MemberImportCSVDto>();
            int recordNbr = -1;

            const int memberbatchSize = Constants.MemberDataBatchSize;
            int skip = 0;
            List<ETLMemberImportFileDataModel> batchedFileData;
            List<string> partnerCodes = new List<string>();
            int batchRecordsCount = 0;
            var totalRecords = await _memberImportFileDataService.GetBatchedDataCount(memberImportFileId);
            _logger.LogInformation("{ClassName}.{MethodName} - Total records {TotalRecords} of fileId = {MemberImportFileId}", className, methodName, totalRecords, memberImportFileId);
            do
            {
                batchedFileData = await _memberImportFileDataService.GetMemberImportFileDataRecords(memberImportFileId, memberbatchSize);
                batchRecordsCount = batchedFileData.Count;
                foreach (var data in batchedFileData)
                {
                    try
                    {
                        _jobReportService.JobResultDetails.RecordsReceived++;
                        recordNbr++;
                        var consumerCsvDto = _memberImportFileDataService.ConvertToConsumerCsvDto(data);


                        if (consumerCsvDto != null)
                        {
                            //Add partner code to list to send to WALLET/ACCOUNT SYNC SNS
                            if (!string.IsNullOrWhiteSpace(consumerCsvDto.partner_code))
                            {
                                if (!partnerCodes.Contains(consumerCsvDto.partner_code))
                                {
                                    partnerCodes.Add(consumerCsvDto.partner_code);
                                }
                            }

                            if (string.IsNullOrWhiteSpace(consumerCsvDto.mem_nbr))
                            {
                                await _memberImportFileDataService.UpdateMemberImportFileDataRecordProcessingStatus(data.MemberImportFileDataId, (long)RecordProcessingStatusType.FAILED);
                                var msg = $"Member Number is null for Record : {recordNbr} in file";
                                throw new ETLException(ETLExceptionCodes.NullValue, msg);
                            }
                            var key = consumerCsvDto.mem_nbr;

                            if (!_jobReportService.keyRecordErrorMap.ContainsKey(key))
                            {
                                _jobReportService.keyRecordErrorMap.Add(key, new RecordError(recordNbr));
                            }

                            string action = consumerCsvDto.action?.Trim()?.ToUpper();

                            if (action == ActionTypes.InsertCode || action == ActionTypes.InsertDescription)
                            {
                                memberImportInsertList.Add(consumerCsvDto);
                            }
                            else if (action == ActionTypes.UpdateCode || action == ActionTypes.UpdateDescription)
                            {
                                memberImportUpdateList.Add(consumerCsvDto);
                            }
                            else if (action == ActionTypes.DeleteCode || action == ActionTypes.DeleteDescription)
                            {
                                memberImportDeleteList.Add(consumerCsvDto);
                            }
                            else if (action == ActionTypes.CancelCode || action == ActionTypes.CancelDescription)
                            {
                                memberImportCancelList.Add(consumerCsvDto);
                            }
                            else if (action == ActionTypes.ActivateCode || action == ActionTypes.ActivateDescription)
                            {
                                await _memberImportFileDataService.UpdateMemberImportFileDataRecordProcessingStatus(data.MemberImportFileDataId, (long)RecordProcessingStatusType.SUCCESS);
                                _logger.LogInformation("{ClassName}.{MethodName} - Skipping record with ACTIVE/A action. Member number: {Mem_nbr}, Subscriber member number: {Subscriber_mem_nbr}.", className, methodName, consumerCsvDto.mem_nbr, consumerCsvDto.subscriber_mem_nbr);
                                continue;
                            }
                            else
                            {
                                await _memberImportFileDataService.UpdateMemberImportFileDataRecordProcessingStatus(data.MemberImportFileDataId, (long)RecordProcessingStatusType.FAILED);
                                var msg = $"{className}.{methodName}:{_fileName + "_Error"} Skipping record with invalid action. MemberNbr: {consumerCsvDto.mem_nbr}, SubscriberNbr: {consumerCsvDto.subscriber_mem_nbr}, Action: {consumerCsvDto.action}.";
                                _logger.LogInformation(msg);
                                _jobReportService.CollectError(recordNbr, 400, msg, null);
                                _jobReportService.JobResultDetails.RecordsErrorCount++;
                                _jobReportService.JobResultDetails.RecordsProcessed++;
                                
                                continue;
                            }
                        }

                        // Check if the insert batch size is reached
                        if (memberImportInsertList.Count >= batchSize)
                        {

                            _logger.LogInformation("{ClassName}.{MethodName} - Processing insert batch of Size:{Count}.", className, methodName, memberImportInsertList.Count);
                            // Process the current insert batch 
                            (var consumers, var persons) = await ProcessBatchAsync(memberImportInsertList, etlExecutionContext);

                            if (consumers?.Count > 0)
                                etlConsumers.AddRange(consumers);

                            if (persons?.Count > 0)
                                etlPersons.AddRange(persons);

                            // Clear current insert batch
                            memberImportInsertList.Clear();
                        }

                        // Check if the update batch size is reached
                        if (memberImportUpdateList.Count >= batchSize)
                        {
                            _logger.LogInformation("{ClassName}.{MethodName} - Processing update batch of Size:{Count}.", className, methodName, memberImportInsertList.Count);
                            // Process the current update batch
                            (var consumers, var persons) = await ProcessUpdateBatchAsync(memberImportUpdateList, etlExecutionContext, ActionTypes.UpdateDescription);

                            if (consumers?.Count > 0)
                                etlConsumers.AddRange(consumers);

                            if (persons?.Count > 0)
                                etlPersons.AddRange(persons);

                            // Clear current update batch
                            memberImportUpdateList.Clear();
                        }

                        // Check if the delete batch size is reached
                        if (memberImportDeleteList.Count >= batchSize)
                        {
                            _logger.LogInformation("{ClassName}.{MethodName} - Processing delete batch of Size:{Count}.", className, methodName, memberImportInsertList.Count);
                            // Process the current delete batch
                            (var consumers, var persons) = await ProcessDeleteBatchAsync(memberImportDeleteList, etlExecutionContext);

                            if (consumers?.Count > 0)
                                etlConsumers.AddRange(consumers);

                            if (persons?.Count > 0)
                                etlPersons.AddRange(persons);

                            // Clear current delete batch
                            memberImportDeleteList.Clear();
                        }

                        // Check if the cancel batch size is reached
                        if (memberImportCancelList.Count >= batchSize)
                        {
                            _logger.LogInformation("{ClassName}.{MethodName} - Processing cancel batch of Size:{Count}.", className, methodName, memberImportInsertList.Count);
                            // Process the current cancel batch
                            (var consumers, var persons) = await ProcessUpdateBatchAsync(memberImportCancelList, etlExecutionContext, ActionTypes.CancelDescription);

                            if (consumers?.Count > 0)
                                etlConsumers.AddRange(consumers);

                            if (persons?.Count > 0)
                                etlPersons.AddRange(persons);

                            // Clear current cancel batch
                            memberImportCancelList.Clear();
                        }
                    }
                    catch (HeaderValidationException ex)
                    {
                        await _memberImportFileDataService.UpdateMemberImportFileDataRecordProcessingStatus(data.MemberImportFileDataId, (long)RecordProcessingStatusType.FAILED);
                        _logger.LogError(ex, "{ClassName}.{MethodName} -{FileName} : Header validation error ,ErrorCode:{Code}, ERROR:{Msg}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        await _memberImportFileDataService.UpdateMemberImportFileDataRecordProcessingStatus(data.MemberImportFileDataId, (long)RecordProcessingStatusType.FAILED);
                        var msg = $"{methodName}: Error during processing. ErrorMessage - {ex.Message}";
                        _logger.LogError(ex, "{ClassName}.{MethodName} -{FileName} : Error while processing Member Import ,ErrorCode:{Code}, ERROR:{Msg}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                        _jobReportService.CollectError(recordNbr, 400, msg, ex);
                        _jobReportService.JobResultDetails.RecordsErrorCount++;
                        _jobReportService.JobResultDetails.RecordsProcessed++;
                    }
                }

                skip += memberbatchSize;

            } while (batchRecordsCount > 0);
            // while (recordsCount > 0)
           

            // Process the remaining records for insert batch
            if (memberImportInsertList.Count > 0)
            {
                (var consumers, var persons) = await ProcessBatchAsync(memberImportInsertList, etlExecutionContext);

                if (consumers?.Count > 0)
                    etlConsumers.AddRange(consumers);

                if (persons?.Count > 0)
                    etlPersons.AddRange(persons);
            }

            // Process the remaining records for update batch
            if (memberImportUpdateList.Count > 0)
            {
                (var consumers, var persons) = await ProcessUpdateBatchAsync(memberImportUpdateList, etlExecutionContext, ActionTypes.UpdateDescription);

                if (consumers?.Count > 0)
                    etlConsumers.AddRange(consumers);

                if (persons?.Count > 0)
                    etlPersons.AddRange(persons);
            }

            // Process the remaining records for delete batch
            if (memberImportDeleteList.Count > 0)
            {
                (var consumers, var persons) = await ProcessDeleteBatchAsync(memberImportDeleteList, etlExecutionContext);

                if (consumers?.Count > 0)
                    etlConsumers.AddRange(consumers);

                if (persons?.Count > 0)
                    etlPersons.AddRange(persons);
            }

            // Process the remaining records for cancel batch
            if (memberImportCancelList.Count > 0)
            {
                (var consumers, var persons) = await ProcessUpdateBatchAsync(memberImportCancelList, etlExecutionContext, ActionTypes.CancelDescription);

                if (consumers?.Count > 0)
                    etlConsumers.AddRange(consumers);

                if (persons?.Count > 0)
                    etlPersons.AddRange(persons);
            }

            // Following code is the first step of ETL Automation
            // It will pusbh notification to AWS SNS for tenant config sync event if the tenant is configured for it
            var tenants = await _tenantRepo.FindAsync(x => partnerCodes.Contains(x.PartnerCode) && x.DeleteNbr == 0);
            foreach (var tenant in tenants)
            {
                if (!string.IsNullOrWhiteSpace(tenant.TenantOption))
                {
                    var tenantOption = JsonConvert.DeserializeObject<TenantOption>(tenant.TenantOption);
                    if (tenantOption?.EtlAutomationConfig != null && tenantOption.EtlAutomationConfig.IsTenantConfigSyncEnabled)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Sending notification to AWS SNS for tenant config sync event. Tenant Code: {TenantCode}", className, methodName, tenant.TenantCode);
                        var message = new
                        {
                            EventType = "SYNC_TENANT_CONFIG",
                            tenant.TenantCode,
                            SyncTenantConfigOptions = "WALLET,CONSUMER_ACCOUNT_CONFIG",
                            ConsumerCodes = "ALL"
                        };

                        // Push notification to AWS SNS
                        await _awsNotificationService.PushNotificationToAwsTopic(new AwsSnsMessage(JsonConvert.SerializeObject(message)), "AWS_ETL_AUTOMATION_SNS_ARN", false, string.Empty, string.Empty);
                        _logger.LogInformation("{ClassName}.{MethodName} - Notification sent to AWS SNS for tenant config sync event. Tenant Code: {TenantCode}", className, methodName, tenant.TenantCode);
                    }
                }

            }

            _logger.LogInformation("{ClassName}.{MethodName} - Finished processing.", className, methodName);
        }

        /// <summary>
        /// Processes a batch of member data asynchronously.
        /// </summary>
        /// <param name="memberCsvDtoList">The list of MemberImportCSVDto.</param>
        /// <param name="etlExecutionContext">The EtlExecutionContext.</param>
        /// <returns>A tuple containing lists of ETLConsumerModel and ETLPersonModel.</returns>
        public async Task<(List<ETLConsumerModel>, List<ETLPersonModel>)> ProcessBatchAsync(List<MemberImportCSVDto> memberCsvDtoList, EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ProcessBatchAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing CreateMembers with PartnerCode:{Code},Memnumbers:{Nbrs}", className, methodName, memberCsvDtoList.Select(e => e.partner_code).ToList(), memberCsvDtoList.Select(e => e.mem_nbr).ToList());
            var etlConsumers = new List<ETLConsumerModel>();
            var etlPersons = new List<ETLPersonModel>();

            try
            {
                var memberEnrDetailList = CreateMemberDtos(memberCsvDtoList, etlExecutionContext.SubscriberOnly);

                if (memberEnrDetailList == null)
                {
                    var msg = $"{methodName}: Member enrollment details are null.";
                    _logger.LogError("{ClassName}.{MethodName} - {FileName} Member enrollment details are null.", className, methodName, _fileName + "_Error");
                    if (!etlExecutionContext.IsCreateDuplicateConsumer)
                    {
                        await AddErrorForBatchRecords(memberCsvDtoList, msg);
                    }
                    throw new ETLException(ETLExceptionCodes.NullValue, "Member enrollment details are null.");
                }
                var tenant = await _tenantRepo.FindOneAsync(x => x.PartnerCode == memberCsvDtoList[0].partner_code && x.DeleteNbr == 0);
                if (tenant != null && tenant.TenantCode != null)
                {
                    etlExecutionContext.TenantCode = tenant.TenantCode;
                    await _memberImportFileDataService.AddtenantforPostRun(memberEnrDetailList, ActionTypes.InsertDescription);
                    var xApiKeySecret = await _secretHelper.GetTenantSecret(tenant.TenantCode, Constants.XApiKeySecret);

                    var authHeaders = new Dictionary<string, string>
                    {
                        { Constants.XApiKey, xApiKeySecret },
                    };

                    var customerRequestDto = new CustomerRequestDto()
                    {
                        CustomerCode = etlExecutionContext.CustomerCode,
                        CustomerLabel = etlExecutionContext.CustomerLabel
                    };
                    var tokenResponse = new TokenResponseDto();

                    try
                    {
                        tokenResponse = await _dataFeedClient.Post<TokenResponseDto>(Constants.Token, customerRequestDto, authHeaders);

                        if (!string.IsNullOrEmpty(tokenResponse.ErrorMessage) || string.IsNullOrEmpty(tokenResponse.JWT))
                        {
                            _logger.LogError("{ClassName}.{MethodName} - {FileName}: Error Response token from Token API For Request:{Request}.,ErrorCode:{Code}, ERROR: {Message}", className, methodName, _fileName + "_Error", customerRequestDto.ToJson(), tokenResponse.ErrorCode, tokenResponse.ErrorMessage);
                            throw new ETLException(ETLExceptionCodes.NullValue, $"Invalid Token from {Constants.Token} API");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} -{FileName} Error while requesting token from Token API.,ErrorCode:{Code}, ERROR: {Message}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                        throw;
                    }
                    authHeaders.Add(Constants.XApiSessionKey, tokenResponse.JWT);
                   
                    var insertMemberDetails = await _consumerService.GetUpdatedInsurancePeriod(memberEnrDetailList.ToList());
                    var memberEnrollmentRequestDto = new MemberEnrollmentRequestDto
                    {
                        Members = insertMemberDetails.ToArray(),
                    };
                    var membersResponseDto = new MembersResponseDto();

                    try
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Sending insert request to data-feed/members API.", className, methodName);
                        membersResponseDto = await _dataFeedClient.Post<MembersResponseDto>(Constants.DatafeedInsertMembers, memberEnrollmentRequestDto, authHeaders);

                        if (membersResponseDto == null)
                        {
                            var msg = $"{methodName}: Received null response from data-feed insert members API.";
                            if (!etlExecutionContext.IsCreateDuplicateConsumer)
                            {
                                await AddErrorForBatchRecords(memberCsvDtoList, msg);
                            }
                            _logger.LogError("{ClassName}.{MethodName} - {fileName} Received null response from data-feed insert members API.", className, methodName, _fileName + "_Error");

                            throw new ETLException(ETLExceptionCodes.NullResponseFromAPI, $"Received null response from data-feed insert members API.");
                        }
                        if (membersResponseDto.ExtendedErrors.Any())
                        {
                            _logger.LogError("{ClassName}.{MethodName} -{FileName} Processing failed for the following MemberNbr(s) due to Errors:{Errors}", className, methodName, _fileName + "_Error", membersResponseDto.ExtendedErrors);
                        }


                        if (membersResponseDto.Consumers == null || !membersResponseDto.Consumers.Any())
                        {
                            var msg = $"{methodName}:{_fileName + "_Error"} No consumers were returned in the response from data-feed insert members API.";
                            _logger.LogError(msg);
                            if (!etlExecutionContext.IsCreateDuplicateConsumer)
                            {
                                await AddErrorForBatchRecords(memberCsvDtoList, msg);
                            }
                            _logger.LogError("{ClassName}.{MethodName} -{FileName} No consumers were returned in the response from data-feed insert members API.", className, methodName, _fileName + "_Error");

                            throw new ETLException(ETLExceptionCodes.NullResponseFromAPI, msg);
                        }
                        else
                        {
                            if (!etlExecutionContext.IsCreateDuplicateConsumer)
                            { await AddErrorsFromDataFeedResponse(membersResponseDto, ActionTypes.InsertDescription, etlExecutionContext.ETLStartTs, memberCsvDtoList); }
                        }

                         
                        // await ProcessPLD(tenant, etlExecutionContext, etlConsumers, etlPersons, membersResponseDto);
                        _logger.LogInformation("{ClassName}.{MethodName} - Completed processing.", className, methodName);



                        return (etlConsumers, etlPersons);
                    }
                    catch (Exception ex)
                    {

                        _logger.LogError(ex, "{ClassName}.{MethodName} -{FileName} Failed processing Create memebers,ErrorCode:{Code}, ERROR: {Msg}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                        _logger.LogInformation("{ClassName}.{MethodName} - Failed processing CreateMembers. AuthHeaders: {Headers}, Request: {Request}", className, methodName, JsonConvert.SerializeObject(authHeaders), JsonConvert.SerializeObject(memberEnrollmentRequestDto));

                        throw;
                    }
                }
                else
                {
                    var msg = $"{methodName}: Tenant not found or TenantCode is null for partner code {memberCsvDtoList[0].partner_code}. Member number: {memberCsvDtoList[0].partner_code}, Subscriber member number: {memberCsvDtoList[0].subscriber_mem_nbr}";
                    _logger.LogError("{ClassName}.{MethodName} -{fileName} Tenant not found or TenantCode is null for partner code: {Partner_code}. Member number: {Mnbr}, Subscriber member number:{SNmbr}", className, methodName, _fileName + "_Error", memberCsvDtoList[0].partner_code, memberCsvDtoList[0].mem_nbr, memberCsvDtoList[0].subscriber_mem_nbr);
                    throw new ETLException(ETLExceptionCodes.NotFoundInDb, msg);
                }
            }
            catch (Exception ex)
            {
                var msg = $"data-feed/members api: Failed processing. Error: {ex.Message}. \n {ex}";
                _logger.LogError(ex, "{ClassName}.{MethodName} -{FileName} Failed processing Create memebers,ErrorCode:{Code}, ERROR: {Msg}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                if (!etlExecutionContext.IsCreateDuplicateConsumer)
                {
                    await AddErrorForBatchRecords(memberCsvDtoList, msg);
                }
                throw;
            }
        }


        private async Task<(List<ETLConsumerModel>, List<ETLPersonModel>)> ProcessUpdateBatchAsync(List<MemberImportCSVDto> memberCsvDtoList, EtlExecutionContext etlExecutionContext, string actionType)
        {
            const string methodName = nameof(ProcessUpdateBatchAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing UpdateMembers with PartnerCode:{Code},Memnumbers:{Nbrs}", className, methodName, memberCsvDtoList.Select(e => e.partner_code).ToList(), memberCsvDtoList.Select(e => e.mem_nbr).ToList());

            var etlConsumers = new List<ETLConsumerModel>();
            var etlPersons = new List<ETLPersonModel>();

            try
            {

                var memberEnrDetailList = CreateMemberDtos(memberCsvDtoList, etlExecutionContext.SubscriberOnly);

                if (memberEnrDetailList == null)
                {
                    var msg = $"{methodName}: No member enrollment details created from the CSV data.";
                    await AddErrorForBatchRecords(memberCsvDtoList, msg);
                    _logger.LogError("{ClassName}.{MethodName} -{FileName} Member enrollment details are null.", className, methodName, _fileName + "_Error");

                    return (etlConsumers, etlPersons);
                }

                var tenant = await _tenantRepo.FindOneAsync(x => x.PartnerCode == memberCsvDtoList[0].partner_code && x.DeleteNbr == 0);
                if (tenant != null && tenant.TenantCode != null)
                {
                    etlExecutionContext.TenantCode = tenant.TenantCode;
                    if (actionType.Equals(ActionTypes.CancelDescription))
                    {
                        await _memberImportFileDataService.AddtenantforPostRun(memberEnrDetailList, ActionTypes.CancelDescription);
                    }

                    else
                    {
                        await _memberImportFileDataService.AddtenantforPostRun(memberEnrDetailList, ActionTypes.UpdateDescription);
                    }


                    var xApiKeySecret = await _secretHelper.GetTenantSecret(tenant.TenantCode, Constants.XApiKeySecret);
                    var authHeaders = new Dictionary<string, string>
                    {
                        { Constants.XApiKey, xApiKeySecret },
                    };

                    var customerRequestDto = new CustomerRequestDto
                    {
                        CustomerCode = etlExecutionContext.CustomerCode,
                        CustomerLabel = etlExecutionContext.CustomerLabel
                    };

                    TokenResponseDto tokenResponse;
                    try
                    {
                        tokenResponse = await _dataFeedClient.Post<TokenResponseDto>(Constants.Token, customerRequestDto, authHeaders);
                        if (!string.IsNullOrEmpty(tokenResponse.ErrorMessage) || string.IsNullOrEmpty(tokenResponse.JWT))
                        {
                            _logger.LogError("{ClassName}.{MethodName} -{FileName} Error Response token from Token API For Request:{Request}.,ErrorCode:{Code}, ERROR: {Message}", className, methodName, _fileName + "_Error", customerRequestDto.ToJson(), tokenResponse.ErrorCode, tokenResponse.ErrorMessage);

                            throw new InvalidDataException($"Invalid Token from {Constants.Token} API");

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} -{FileNmae} Error while requesting token from Token API.,ErrorCode:{Code}, ERROR: {Message}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                        throw;
                    }

                    authHeaders.Add(Constants.XApiSessionKey, tokenResponse.JWT);

                    var memberEnrollmentRequestDto = new MemberEnrollmentRequestDto
                    {
                        Members = memberEnrDetailList
                    };

                    MembersResponseDto membersResponseDto;
                    try
                    {
                        if (actionType == ActionTypes.CancelDescription)
                        {
                            
                            _logger.LogInformation("{ClassName}.{MethodName} - Sending cancel request to data-feed/cancel-members API.", className, methodName);
                            membersResponseDto = await _dataFeedClient.Post<MembersResponseDto>(Constants.DatafeedCancelMembers, memberEnrollmentRequestDto, authHeaders);
                        }
                        else
                        {
                           var updateMemberDetails= await _consumerService.GetUpdatedInsurancePeriod(memberEnrDetailList.ToList());
                             memberEnrollmentRequestDto = new MemberEnrollmentRequestDto
                            {
                                Members = updateMemberDetails.ToArray(),
                            };
                            _logger.LogInformation("{ClassName}.{MethodName} - Sending update request to data-feed/update-members API.", className, methodName);
                            membersResponseDto = await _dataFeedClient.Post<MembersResponseDto>(Constants.DatafeedUpdateMembers, memberEnrollmentRequestDto, authHeaders);
                        }

                        if (membersResponseDto.ExtendedErrors.Any())
                        {
                            _logger.LogError("{ClassName}.{MethodName} -{FileName} Processing failed for the following MemberNbr(s) due to Errors:{Errors}", className, methodName, _fileName + "_Error", membersResponseDto.ExtendedErrors);
                        }


                        if (membersResponseDto.Consumers == null || !membersResponseDto.Consumers.Any())
                        {
                            var msg = $"{methodName}: No consumers were returned in the response from data-feed update members API.";
                            _logger.LogError("{ClassName}.{MethodName} - {FileName} No consumers were returned in the response from data-feed insert members API.", className, methodName, _fileName + "_Error");
                            await AddErrorForBatchRecords(memberCsvDtoList, msg);

                            return (etlConsumers, etlPersons);
                        }
                        else
                        {
                            await AddErrorsFromDataFeedResponse(membersResponseDto, actionType, etlExecutionContext.ETLStartTs, memberCsvDtoList);
                        }

                        //Set Enrollment status for cancelled consumer
                        await _consumerService.UpdateConsumerEnrollment(membersResponseDto, actionType);

                        // Uncomment and implement this line if needed:
                        // await ProcessPLD(tenant, etlExecutionContext, etlConsumers, etlPersons, membersResponseDto);

                        _logger.LogInformation("{ClassName}.{MethodName} - Completed processing.", className, methodName);
                        return (etlConsumers, etlPersons);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} -{FileName} Failed processing update memebers,ErrorCode:{Code}, ERROR: {Msg}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                        _logger.LogInformation("{ClassName}.{MethodName} - Failed processing updateMembers. AuthHeaders: {Headers}, Request: {Request}", className, methodName, JsonConvert.SerializeObject(authHeaders), JsonConvert.SerializeObject(memberEnrollmentRequestDto));
                        throw;
                    }
                }
                else
                {
                    var msg = $"{methodName}: Tenant not found or TenantCode is null for partner code {memberCsvDtoList[0].partner_code}. Member number: {memberCsvDtoList[0].partner_code}, Subscriber member number: {memberCsvDtoList[0].subscriber_mem_nbr}";
                    _logger.LogError("{ClassName}.{MethodName} - {FileName} Tenant not found or TenantCode is null for partner code: {Partner_code}. Member number: {Mnbr}, Subscriber member number:{SNmbr}", className, methodName, _fileName + "_Error", memberCsvDtoList[0].partner_code, memberCsvDtoList[0].mem_nbr, memberCsvDtoList[0].subscriber_mem_nbr);
                    throw new InvalidDataException(msg);

                }
            }
            catch (Exception ex)
            {
                var msg = $"{methodName}: Failed processing. Error: {ex.Message}. \n {ex}";
                _logger.LogError(ex, "{ClassName}.{MethodName} -{FileName} Failed processing Create memebers,ErrorCode:{Code}, ERROR: {Msg}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                await AddErrorForBatchRecords(memberCsvDtoList, msg);
            }

            // Ensure a return statement in case the tenant is null or tenant.TenantCode is null
            return (etlConsumers, etlPersons);
        }

        private async Task<(List<ETLConsumerModel>, List<ETLPersonModel>)> ProcessDeleteBatchAsync(List<MemberImportCSVDto> memberCsvDtoList, EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ProcessDeleteBatchAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing DeleteMembers with PartnerCode:{Code},Memnumbers:{Nbrs}", className, methodName, memberCsvDtoList.Select(e => e.partner_code).ToList(), memberCsvDtoList.Select(e => e.mem_nbr).ToList());


            var etlConsumers = new List<ETLConsumerModel>();
            var etlPersons = new List<ETLPersonModel>();

            try
            {

                var memberEnrDetailList = CreateMemberDtos(memberCsvDtoList, etlExecutionContext.SubscriberOnly);

                if (memberEnrDetailList == null)
                {
                    var msg = $"{methodName}: Member enrollment details are null.";
                    await AddErrorForBatchRecords(memberCsvDtoList, msg);
                    _logger.LogError("{ClassName}.{MethodName} -{FileName} Member enrollment details are null.", className, methodName, _fileName + "_Error");

                    return (etlConsumers, etlPersons);
                }

                var tenant = await _tenantRepo.FindOneAsync(x => x.PartnerCode == memberCsvDtoList[0].partner_code && x.DeleteNbr == 0);
                if (tenant != null && tenant.TenantCode != null)
                {
                    etlExecutionContext.TenantCode = tenant.TenantCode;
                    await _memberImportFileDataService.AddtenantforPostRun(memberEnrDetailList, ActionTypes.DeleteDescription);


                    var xApiKeySecret = await _secretHelper.GetTenantSecret(tenant.TenantCode, Constants.XApiKeySecret);
                    var authHeaders = new Dictionary<string, string>
                    {
                        { Constants.XApiKey, xApiKeySecret },
                    };

                    var customerRequestDto = new CustomerRequestDto
                    {
                        CustomerCode = etlExecutionContext.CustomerCode,
                        CustomerLabel = etlExecutionContext.CustomerLabel
                    };

                    TokenResponseDto tokenResponse;
                    try
                    {

                        tokenResponse = await _dataFeedClient.Post<TokenResponseDto>(Constants.Token, customerRequestDto, authHeaders);

                        if (!string.IsNullOrEmpty(tokenResponse.ErrorMessage) || string.IsNullOrEmpty(tokenResponse.JWT))
                        {
                            _logger.LogError("{ClassName}.{MethodName} -{FileName} Error Response token from Token API For Request:{Request}.,ErrorCode:{Code}, ERROR: {Message}", className, methodName, _fileName + "_Error", customerRequestDto.ToJson(), tokenResponse.ErrorCode, tokenResponse.ErrorMessage);

                            throw new InvalidDataException($"Invalid Token from {Constants.Token} API");

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} -{FileName} Error while requesting token from Token API.,ErrorCode:{Code}, ERROR: {Message}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                        throw;
                    }

                    authHeaders.Add(Constants.XApiSessionKey, tokenResponse.JWT);

                    var memberEnrollmentRequestDto = new MemberEnrollmentRequestDto
                    {
                        Members = memberEnrDetailList
                    };

                    MembersResponseDto membersResponseDto;
                    try
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Sending Delete request to data-feed/delete-members API.", className, methodName);
                        membersResponseDto = await _dataFeedClient.Post<MembersResponseDto>(Constants.DatafeedDeleteMembers, memberEnrollmentRequestDto, authHeaders);

                        if (membersResponseDto.ExtendedErrors.Any())
                        {
                            _logger.LogError("{ClassName}.{MethodName} -{FileName} Processing failed for the following MemberNbr(s) due to Errors:{Errors}", className, methodName, _fileName + "_Error", membersResponseDto.ExtendedErrors);

                        }

                        if (membersResponseDto.Consumers == null || !membersResponseDto.Consumers.Any())
                        {
                            var msg = $"{methodName}: No consumers were returned in the response from data-feed Delete members API.";
                            _logger.LogError("{ClassName}.{MethodName} -{FileName} No consumers were returned in the response from data-feed delete members API.", className, methodName, _fileName + "_Error");
                            await AddErrorForBatchRecords(memberCsvDtoList, msg);
                            return (etlConsumers, etlPersons);
                        }
                        else
                        {
                            await AddErrorsFromDataFeedResponse(membersResponseDto, ActionTypes.DeleteDescription, etlExecutionContext.ETLStartTs, memberCsvDtoList);
                        }

                        //Set Enrollment status for deleted consumer
                        await _consumerService.UpdateConsumerEnrollment(membersResponseDto, ActionTypes.DeleteDescription);

                        // Uncomment and implement this line if needed:
                        // await ProcessPLD(tenant, etlExecutionContext, etlConsumers, etlPersons, membersResponseDto);

                        _logger.LogInformation("{ClassName}.{MethodName} - Completed processing.", className, methodName);
                        return (etlConsumers, etlPersons);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} -{fileName} Failed processing Delete memebers,ErrorCode:{Code}, ERROR: {Msg}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                        _logger.LogInformation("{ClassName}.{MethodName} - Failed processing Delete Members. AuthHeaders: {Headers}, Request: {Request}", className, methodName, JsonConvert.SerializeObject(authHeaders), JsonConvert.SerializeObject(memberEnrollmentRequestDto));

                        throw;
                    }
                }
                else
                {
                    var msg = $"{methodName}: Tenant not found or TenantCode is null for partner code {memberCsvDtoList[0].partner_code}. Member number: {memberCsvDtoList[0].partner_code}, Subscriber member number: {memberCsvDtoList[0].subscriber_mem_nbr}";
                    _logger.LogError("{ClassName}.{MethodName} -{fileName} Tenant not found or TenantCode is null for partner code: {Partner_code}. Member number: {Mnbr}, Subscriber member number:{SNmbr}", className, methodName, _fileName + "_Error", memberCsvDtoList[0].partner_code, memberCsvDtoList[0].mem_nbr, memberCsvDtoList[0].subscriber_mem_nbr);
                    throw new InvalidDataException(msg);
                }
            }
            catch (Exception ex)
            {
                var msg = $"{methodName}: Failed processing. Error: {ex.Message}. \n {ex}";
                _logger.LogError(ex, "{ClassName}.{MethodName} -{FileName} Failed processing  memebers,ErrorCode:{Code}, ERROR: {Msg}", className, methodName, _fileName + "_Error", StatusCodes.Status500InternalServerError, ex.Message);
                await AddErrorForBatchRecords(memberCsvDtoList, msg);
            }

            // Ensure a return statement in case the tenant is null or tenant.TenantCode is null
            return (etlConsumers, etlPersons);
        }


        /// <summary>
        /// Processes PLD (Partner Level Data) for consumers.
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="etlExecutionContext"></param>
        /// <param name="etlConsumers"></param>
        /// <param name="etlPersons"></param>
        /// <param name="membersResponseDto"></param>
        /// <returns></returns>
        private async Task ProcessPLD(ETLTenantModel tenant, EtlExecutionContext etlExecutionContext, List<ETLConsumerModel> etlConsumers, List<ETLPersonModel> etlPersons, MembersResponseDto membersResponseDto)
        {

            try
            {
                if (!etlExecutionContext.EnablePldProcessing || string.IsNullOrEmpty(etlExecutionContext.PldFilePath))
                {
                    return; // PLD processing is not enabled or file path is not provided
                }

                // Process consumer with attrs using pld file
                List<ETLConsumerModel> pldConsumers = await ProcessConsumerAttrUsingPldFile(tenant.TenantCode, etlExecutionContext.PldFilePath);

                // add consumers to main list for processing cohorts if not already present in list
                var loadedConsumerCodes = membersResponseDto.Consumers.Select(x => x.Consumer.ConsumerCode).ToList();
                foreach (var pldConsumer in pldConsumers)
                {
                    if (!loadedConsumerCodes.Contains(pldConsumer.ConsumerCode))
                    {
                        var person = _personRepo.FindOneAsync(x => x.PersonId == pldConsumer.PersonId).Result;
                        if (person != null)
                        {
                            etlPersons.Add(person);
                            etlConsumers.Add(pldConsumer);
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerFileRecs"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        private MemberEnrollmentDetailDto[] CreateMemberDtos(List<MemberImportCSVDto> memberFileRecs, bool subscriberOnly = false)
        {
            const string methodName = nameof(CreateMemberDtos);

            var memberDtos = new List<MemberEnrollmentDetailDto>();

            foreach (var memberRec in memberFileRecs)
            {
                DateTime eligibilityEnd = DateTime.ParseExact(memberRec.eligibility_end, "MM/dd/yyyy", null);
                DateTime eligibilityStart = DateTime.ParseExact(memberRec.eligibility_start, "MM/dd/yyyy", null);
                DateTime dob = DateTime.ParseExact(memberRec.dob, "MM/dd/yyyy", null);
                var isSSOUser = memberRec.is_sso_user ?? false;
                int age;
                if (int.TryParse(memberRec.age, out int parsedAge) && parsedAge >= 18)
                {
                    age = parsedAge;
                }
                else
                {
                    // Fallback to calculating age using DOB
                    age = CalculateAge(dob);

                    if (age < 18)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - member age is less than 18 years ,member data:{memberRec}", className, methodName, memberRec.ToJson());

                        continue; // Skip members under 18
                    }
                }
                var memberDto = new MemberEnrollmentDetailDto
                {
                    MemberDetail = new MemberDetailDto
                    {
                        FirstName = memberRec.first_name,
                        LastName = memberRec.last_name,
                        LanguageCode = string.IsNullOrWhiteSpace(memberRec.language_code) ? "en-US" : memberRec.language_code,
                        MemberSince = DateTime.UtcNow,
                        Email = memberRec.email,
                        City = memberRec.city,
                        Country = memberRec.country ?? "US",
                        PostalCode = memberRec.postal_code,
                        PhoneNumber = memberRec.mobile_phone,
                        Region = " ",
                        Dob = dob,
                        Gender = GetGenderString(memberRec.gender),
                        MailingAddressLine1 = memberRec.mailing_address_line1,
                        MailingAddressLine2 = memberRec.mailing_address_line2,
                        MailingState = memberRec.mailing_state,
                        MailingCountryCode = memberRec.mailing_country_code,
                        HomePhoneNumber = memberRec.home_phone_number,
                        MiddleName = memberRec.middle_name,
                        HomeAddressLine1 = memberRec.home_address_line1,
                        HomeAddressLine2 = memberRec.home_address_line2,
                        HomeCity = memberRec.home_city,
                        HomeState = memberRec.home_state,
                        HomePostalCode = memberRec.home_postal_code,
                        Source = "ETL",
                        PersonUniqueIdentifier = memberRec.person_unique_identifier,
                        Age=age

                    },
                    EnrollmentDetail = new EnrollmentDetailDto
                    {
                        PartnerCode = memberRec.partner_code,
                        MemberNbr = memberRec.mem_nbr,
                        SubscriberMemberNbr = memberRec.subscriber_mem_nbr,
                        RegistrationTs = DateTime.UtcNow,
                        EligibleStartTs = eligibilityStart,
                        EligibleEndTs = eligibilityEnd,
                        SubscriberOnly = subscriberOnly,
                        SubsciberMemberNbrPrefix = memberRec.subscriber_mem_nbr_prefix,
                        MemberNbrPrefix = memberRec.mem_nbr_prefix,
                        RegionCode = memberRec.region_code,
                        PlanId = memberRec.plan_id,
                        PlanType = memberRec.plan_type,
                        SubgroupId = memberRec.subgroup_id,
                        IsSSOUser = isSSOUser,
                        MemberId= memberRec.member_id,
                        MemberType=memberRec.member_type,
                        ConsumerAttribute = memberRec.raw_data_json
                    }
                };

                memberDtos.Add(memberDto);
            }

            return memberDtos.ToArray();
        }
        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;

            if (dob.Date > today.AddYears(-age)) age--;

            return age;
        }
        private string GetGenderString(string gender)
        {
            return gender switch
            {
                "M" => "MALE",
                "F" => "FEMALE",
                "O" => "OTHER",
                "U" => "UNKNOWN",
                _ => string.Empty
            };
        }

        private async Task AddErrorsFromDataFeedResponse(MembersResponseDto membersResponseDto, string actionType, DateTime ETLStartTs, List<MemberImportCSVDto> memberCsvDtoList)
        {
            try
            {
                foreach (var rec in membersResponseDto.Consumers)
                {
                    var memberImportFileDataId = memberCsvDtoList
                         .Where(x => x.person_unique_identifier == rec.Person.PersonUniqueIdentifier
                                  && x.member_id == rec.Consumer.MemberId)
                         .Select(x => (long?)x.member_import_file_data_id)
                         .FirstOrDefault();

                    if (rec.ErrorMessage != null || rec.ErrorCode != null)
                    {
                        if (rec?.ErrorCode == StatusCodes.Status202Accepted)
                        {
                            if (memberImportFileDataId != null)
                            {
                                await _memberImportFileDataService.UpdateMemberImportFileDataRecordProcessingStatus(
                                    memberImportFileDataId.Value, (long)RecordProcessingStatusType.SUCCESS);
                            }

                            var hasTenant = _memberImportFileDataService.batchJobReportValidationJson.PostRun?.PerTenantData.FirstOrDefault(x => x.TenantCode == rec?.Consumer?.TenantCode);
                            if (hasTenant != null)
                            {
                                hasTenant.Counts.SuccessfulUpdate++;
                                _memberImportFileDataService.batchJobReportValidationJson.PostRun.CrossTenantData.Counts.SuccessfulUpdate += 1;
                            }
                        }
                        var key = rec?.Consumer.MemberNbr;
                        if (!string.IsNullOrEmpty(key))
                        {
                            if (memberImportFileDataId != null)
                            {
                                await _memberImportFileDataService.UpdateMemberImportFileDataRecordProcessingStatus(
                                    memberImportFileDataId.Value, (long)RecordProcessingStatusType.SUCCESS);
                            }
                            var recordNunber = _jobReportService.keyRecordErrorMap[key!]?.RecordNbr;
                            var errorNbr = rec.ErrorCode.HasValue ? rec.ErrorCode.Value : 400;
                            _jobReportService.CollectError(recordNunber ?? 0, errorNbr, rec.ErrorMessage, null);
                            _jobReportService.JobResultDetails.RecordsErrorCount++;
                            _jobReportService.JobResultDetails.RecordsProcessed++;
                        }
                    }
                    else
                    {
                        if (memberImportFileDataId != null)
                        {
                            await _memberImportFileDataService.UpdateMemberImportFileDataRecordProcessingStatus(
                                memberImportFileDataId.Value, (long)RecordProcessingStatusType.SUCCESS);
                        }
                        await _memberImportFileDataService.UpdateSuccessCount(actionType, rec?.Consumer?.TenantCode ?? string.Empty);
                        await _memberImportFileDataService.GetCounsumerPostCount(rec?.Consumer?.TenantCode ?? string.Empty, ETLStartTs);
                        await _memberImportFileDataService.VerifyCounsumerCount(rec?.Consumer?.TenantCode ?? string.Empty);
                        _jobReportService.JobResultDetails.RecordsSuccessCount++;
                        _jobReportService.JobResultDetails.RecordsProcessed++;

                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddErrorsFromDataFeedResponse -Error ocurred in collection error logs");
            }
        }

        private async Task AddErrorForBatchRecords(List<MemberImportCSVDto> memberCsvDtoList, string msg)
        {
            try
            {
                foreach (var member in memberCsvDtoList)
                {
                    if (member?.member_import_file_data_id != null)
                    {
                        await _memberImportFileDataService.UpdateMemberImportFileDataRecordProcessingStatus(
                            member.member_import_file_data_id.Value, (long)RecordProcessingStatusType.FAILED);
                    }

                    if (!string.IsNullOrEmpty(member?.mem_nbr) && _jobReportService.keyRecordErrorMap.TryGetValue(member.mem_nbr, out var recordError) && recordError != null)
                    {
                        var recordNumber = recordError.RecordNbr;
                        _jobReportService.CollectError(recordNumber, 400, msg, null);
                        _jobReportService.JobResultDetails.RecordsErrorCount++;
                        _jobReportService.JobResultDetails.RecordsProcessed++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddErrorForBatchRecords-Error ocurred in collection error logs");
            }
        }

        private async Task UpdateRecordProcessingStatusForBatchRecords(List<MemberImportCSVDto> memberCsvDtoList, string msg)
        {
            try
            {
                foreach (var data in memberCsvDtoList)
                {
                    if(data?.member_import_file_data_id != null)
                    {
                        await _memberImportFileDataService.UpdateMemberImportFileDataRecordProcessingStatus(data.member_import_file_data_id ?? 0, (long)RecordProcessingStatusType.FAILED);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateRecordProcessingStatusForBatchRecords-Error ocurred in collection error logs");
            }

        }

    }
}
