using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Logs;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Globalization;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class IngestConsumerAttrService : IIngestConsumerAttrService
    {
        private readonly ILogger<IngestConsumerAttrService> _logger;
        private readonly IConsumerRepo _consumerRepo;
        private readonly ISession _session;
        private readonly IS3FileLogger _s3FileLogger;
        private readonly IDataFeedClient _dataFeedClient;
        private readonly ITenantRepo _tenantRepo;
        private const string className = nameof(IngestConsumerAttrService);
        public IngestConsumerAttrService(ILogger<IngestConsumerAttrService> logger, IConsumerRepo consumerRepo,
            ISession session, IS3FileLogger s3FileLogger, IDataFeedClient dataFeedClient, ITenantRepo tenantRepo)
        {
            _logger = logger;
            _consumerRepo = consumerRepo;
            _session = session;
            _s3FileLogger = s3FileLogger;
            _dataFeedClient = dataFeedClient;
            _tenantRepo = tenantRepo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerAttrFileContent"></param>
        /// <returns></returns>
        public async Task<List<ETLConsumerModel>> Ingest(string tenantCode, byte[] consumerAttrFileContent)
        {
            const string methodName = nameof(Ingest);

            _logger.LogInformation("{ClassName}.{MethodName} - Started processing Ingest for TenantCode:{Code}", className, methodName, tenantCode);
            List<ETLConsumerModel> consumers = new();
            try
            {
                using var reader = new StreamReader(new MemoryStream(consumerAttrFileContent));
                List<EtlConsumerAttrCsvRecordDto>? currentMemberRecs = new();

                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    var batchSize = 100;
                    var buffer = new List<EtlConsumerAttrCsvRecordDto>();

                    while (await csv.ReadAsync())
                    {
                        var consumerAttrDto = csv.GetRecord<EtlConsumerAttrCsvRecordDto>();

                        if (consumerAttrDto == null) continue;
                        if (consumerAttrDto.group_name.ToLower().Trim() == "pld") continue;

                        buffer.Add(consumerAttrDto);

                        if (buffer.Count == batchSize)
                        {
                            var processedConsumers = await ProcessBatchAsync(tenantCode, buffer);
                            if (processedConsumers?.Count > 0)
                            {
                                consumers.AddRange(processedConsumers);
                            }
                            buffer.Clear();
                        }
                    }

                    // Process the remaining records in the last batch
                    if (buffer.Count > 0)
                    {
                        var processedConsumers = await ProcessBatchAsync(tenantCode, buffer);
                        if (processedConsumers?.Count > 0)
                        {
                            consumers.AddRange(processedConsumers);
                        }
                    }
                }

                return consumers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing Ingest for TenantCode:{Code},ErrorCode:{Code},ERROR:{Msg}", className, methodName, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                // this log using for s3FileLogger
                await _s3FileLogger.AddErrorLogs(new S3LogContext()
                {
                    Message = ex.Message,
                    TenantCode = tenantCode,
                    Ex = ex
                });
            }
            return consumers;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        async Task<List<ETLConsumerModel>> ProcessBatchAsync(string tenantCode, List<EtlConsumerAttrCsvRecordDto> batch)
        {
            return await Process(tenantCode, batch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currMemNbr"></param>
        /// <param name="currentMemberRecs"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<List<ETLConsumerModel>> Process(string tenantCode, List<EtlConsumerAttrCsvRecordDto> currentMemberRecs)
        {
            const string methodName = nameof(Process);
            var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);
            if (tenant == null)
            {
                _logger.LogError("{ClassName}.{MethodName} - Tenant not found with TenantCode:{Code}", className, methodName, tenantCode);
            }

            ETLMemberAttrRequestDto memberAttrRequestDto = new ETLMemberAttrRequestDto();
            memberAttrRequestDto.MemberAttributes = new List<ETLMemberAttributeDetailDto>();

            memberAttrRequestDto.PartnerCode = tenant.PartnerCode;

            foreach (var csvAttr in currentMemberRecs)
            {
                memberAttrRequestDto.MemberAttributes.Add(new ETLMemberAttributeDetailDto()
                {
                    AttributeName = csvAttr.attr_name,
                    AttributeValue = csvAttr.attr_value,
                    GroupName = csvAttr.group_name,
                    MemberNbr = csvAttr.mem_nbr
                });
            }

            var authHeaders = new Dictionary<string, string>
            {
                { "X-API-KEY", tenant.ApiKey }
            };

            _logger.LogInformation("{ClassName}.{MethodName} - Invoking datafeed/member-attributes endpoint with MemberNumbers:{Numbers},GroupName:{Name}",
                className, methodName, memberAttrRequestDto.MemberAttributes.Select(e => e.MemberNbr).ToList(), memberAttrRequestDto.MemberAttributes.Select(e => e.GroupName).ToList());

            var membersAttrResponseDto = await _dataFeedClient.Post<ETLMemberAttributesResponseDto>("data-feed/member-attributes",
                memberAttrRequestDto, authHeaders);

            List<ETLConsumerModel> consumerModels = new List<ETLConsumerModel>();
            if (membersAttrResponseDto != null && membersAttrResponseDto.Consumer != null)
            {
                foreach (var consumerDto in membersAttrResponseDto.Consumer)
                {
                    consumerModels.Add(new ETLConsumerModel
                    {
                        ConsumerId = consumerDto.ConsumerId,
                        ConsumerCode = consumerDto.ConsumerCode,
                        PersonId = consumerDto.PersonId,
                        TenantCode = consumerDto.TenantCode,
                        Eligible = consumerDto.Eligible,
                        EligibleEndTs = consumerDto.EligibleEndTs,
                        EligibleStartTs = consumerDto.EligibleStartTs,
                        RegistrationTs = consumerDto.RegistrationTs,
                        Registered = consumerDto.Registered,
                        MemberNbr = consumerDto.MemberNbr,
                        ConsumerAttribute = consumerDto.ConsumerAttribute
                    });

                }
            }
            return consumerModels;
        }
    }
}


