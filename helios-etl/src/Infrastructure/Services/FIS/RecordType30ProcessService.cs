using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Logs;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS;
using Microsoft.Extensions.Configuration;
using log4net.Core;
using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.FIS
{
    public class RecordType30ProcessService : IRecordType30ProcessService
    {
        private readonly IFlatFileReader _flatFileReader;
        private readonly IPersonRepo _personRepo;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IConsumerAccountService _consumerAccountService;
        private static int CONSUMER_ACCOUNT_CREATE_CHUNK_SIZE = 100;
        private readonly IS3FISFileLogger _s3FISFileLogger;
        private readonly ILogger<RecordType30ProcessService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IJobReportService _jobReportService;
        const string className = nameof(RecordType30ProcessService);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="flatFileReader"></param>
        /// <param name="logger"></param>
        /// <param name="personRepo"></param>
        /// <param name="consumerRepo"></param>
        /// <param name="consumerAccountService"></param>
        /// <param name="s3FISFileLogger"></param>
        public RecordType30ProcessService(IFlatFileReader flatFileReader, ILogger<RecordType30ProcessService> logger,
            IPersonRepo personRepo, IConsumerRepo consumerRepo, IConsumerAccountService consumerAccountService,
            IS3FISFileLogger s3FISFileLogger, IConfiguration configuration, IJobReportService jobReportService)
        {
            _flatFileReader = flatFileReader;
            _logger = logger;
            _personRepo = personRepo;
            _consumerRepo = consumerRepo;
            _consumerAccountService = consumerAccountService;
            _s3FISFileLogger = s3FISFileLogger;
            _configuration = configuration;
            _jobReportService = jobReportService;
        }

        /// <summary>
        /// Process30RecordFile
        /// </summary>
        /// <param name="line"></param>
        /// <param name="modelObject"></param>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task<ETLConsumerAccountModel> Process30RecordFile(string line, FISCardHolderDataDto modelObject, EtlExecutionContext etlExecutionContext)
        {
            var fisCardHolderData = _flatFileReader.ReadFlatFileRecord((FISCardHolderDataDto)modelObject,
                            line, FISCardHolderDataDto.FieldConfigurationMap);
            ETLConsumerAccountModel consumerAccount = null;

            if (fisCardHolderData != null && fisCardHolderData.CardRecordStatusCode >= FISBatchConstants.CARD_RECORD_MIN_ERROR_STATUS_CODE
                && fisCardHolderData.CardRecordStatusCode <= FISBatchConstants.CARD_RECORD_MAX_ERROR_STATUS_CODE)
            {
                var errorMessage = @$"Error processing 30 record type file: lastName = {fisCardHolderData.LastName}, person_id = {fisCardHolderData.SSN}, 
                                    status = {fisCardHolderData.CardRecordStatusCode}, error message = {fisCardHolderData.ProcessingMessage}";                
                consumerAccount = await CreateErroredConsumerAccountRequest(fisCardHolderData, etlExecutionContext);
                await _s3FISFileLogger.AddErrorLogs(new S3FISLogContext
                {
                    LogFileName = etlExecutionContext.FISRecordFileName,
                    Message = errorMessage,
                    TenantCode = etlExecutionContext?.TenantCode, 
                    throwEtlError = false
                });
            }
            else
            {
                consumerAccount = await CreateConsumerAccountRequest(fisCardHolderData, etlExecutionContext);
            }
            return consumerAccount;
        }

        /// <summary>
        /// CreateConsumerAccount
        /// </summary>
        /// <param name="consumerAccounts"></param>
        /// <returns></returns>
        public async Task<CreateConsumerAccountResponse> CreateConsumerAccount(List<ETLConsumerAccountModel> consumerAccounts)
        {
            // generate 30 record type for each consumer
            bool done = false;
            int index = 0;
            int totalCards = 0;
            int max = consumerAccounts.Count;
            var createdConsumerAccounts = new List<ETLConsumerAccountModel>();

            var createConsumerAccountResponse = new CreateConsumerAccountResponse();

            while (!done)
            {
                var chunkRequest = consumerAccounts.Skip(index).Take(CONSUMER_ACCOUNT_CREATE_CHUNK_SIZE).ToList();
                var consumerAccountResponse = await _consumerAccountService.MergeConsumerAccountAsync(chunkRequest); // Existing Error

                if (consumerAccountResponse == null || consumerAccountResponse.Count == 0)
                {
                    var errorMessage = $"Consumer account creation failed. index:{index}, chunkSize:{CONSUMER_ACCOUNT_CREATE_CHUNK_SIZE}";
                    chunkRequest.ForEach(c =>
                    {
                        if( String.IsNullOrEmpty(_jobReportService.keyRecordErrorMap[c.ConsumerCode!].ErrorMessage))
                        _jobReportService.keyRecordErrorMap[c.ConsumerCode!].ErrorMessage = errorMessage;
                    });
                    createConsumerAccountResponse.ErrorRecords.AddRange(chunkRequest);
                    _logger.LogError(errorMessage);
                }
                else
                {
                    createdConsumerAccounts.AddRange(consumerAccountResponse);
                }

                totalCards += chunkRequest.Count;
                index += chunkRequest.Count;

                if (chunkRequest.Count < CONSUMER_ACCOUNT_CREATE_CHUNK_SIZE)
                {
                    done = true;
                }
                if(index >= max)
                {
                    break;
                }
            }
            createConsumerAccountResponse.SuccessRecords.AddRange(createdConsumerAccounts);
            return createConsumerAccountResponse;
        }

        /// <summary>
        /// CreateConsumerAccountRequest
        /// </summary>
        /// <param name="fisCardHolderData"></param>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        private async Task<ETLConsumerAccountModel> CreateConsumerAccountRequest(FISCardHolderDataDto fisCardHolderData,
            EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(CreateConsumerAccountRequest);
            var consumer = await GetConsumerInfo(fisCardHolderData, etlExecutionContext);
            var consumerAccount = new ETLConsumerAccountModel
            {
                ConsumerCode = consumer.ConsumerCode,
                TenantCode = consumer.TenantCode,
                ClientUniqueId = fisCardHolderData.PANProxyClientUniqueID,
                CreateUser = Constants.CreateUser
            };
            return consumerAccount;
        }

        private async Task<ETLConsumerAccountModel> CreateErroredConsumerAccountRequest(FISCardHolderDataDto fisCardHolderData,
            EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(CreateErroredConsumerAccountRequest);
            var consumer = await GetConsumerInfo(fisCardHolderData, etlExecutionContext);
            var consumerAccount = new ETLConsumerAccountModel
            {
                ConsumerCode = consumer.ConsumerCode,
                TenantCode = consumer.TenantCode,
                CardIssueStatus = BenefitsConstants.EligibleCardIssueStatus,
                ClientUniqueId = fisCardHolderData.PANProxyClientUniqueID,
                CreateUser = Constants.CreateUser
            };
            return consumerAccount;
        }

        private async Task<ETLConsumerModel> GetConsumerInfo(FISCardHolderDataDto fisCardHolderData,
            EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(GetConsumerInfo);
            var personId = ExtractPersonId(fisCardHolderData.SSN);
            var person = await _personRepo.FindOneAsync(personId);
            if (person == null)
            {
                _logger.LogWarning($"{methodName}: Invalid SSN (personId): {personId}");
                await _s3FISFileLogger.AddErrorLogs(new S3FISLogContext
                {
                    LogFileName = etlExecutionContext.FISRecordFileName,
                    Message = $"No matching member found on our side with person_id = {personId}",
                    TenantCode = etlExecutionContext?.TenantCode
                });
                return null;
            }
            var consumer = _consumerRepo.FindOneAsync(x => x.PersonId == person.PersonId
                     && x.DeleteNbr == 0).Result;
            if (consumer == null)
            {
                _logger.LogWarning($"{methodName}: No Consumer found with personId: {personId}");
                await _s3FISFileLogger.AddErrorLogs(new S3FISLogContext
                {
                    LogFileName = etlExecutionContext.FISRecordFileName,
                    Message = $"No matching consumer found on our side with person_id = {personId}",
                    TenantCode = etlExecutionContext?.TenantCode
                });

                _logger.LogError($"{className}.{methodName}: No Consumer found with personId: {personId} ,Error Code:{StatusCodes.Status404NotFound}");

                return null;
            }
            return consumer;
        }

        private long ExtractPersonId(string ssnStr)
        {
            if (!long.TryParse(ssnStr, out var ssn))
            {
                throw new ArgumentException("Invalid SSN value");
            }

            if (!long.TryParse(_configuration.GetSection("SSNStartFrom").Value, out var sSNStartFrom))
            {
                throw new InvalidOperationException("Invalid configuration value for SSNStartFrom");
            }

            if (ssn < sSNStartFrom)
            {
                throw new InvalidOperationException("Invalid SSN value; unable to extract personId");
            }

            return ssn - sSNStartFrom;
        }
    }
}
