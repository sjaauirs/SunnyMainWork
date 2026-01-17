using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Text.Json;
using AutoMapper;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class CohortingEventingService : ICohortingEventingService
    {
        private readonly string _redshiftConnectionString;
        private readonly IRedshiftDataReader _redshiftDataReaderService;
        private readonly ILogger<CohortingEventingService> _logger;
        private readonly ITenantRepo _tenantRepo;
        private readonly IMapper _mapper;
        private readonly IEventingWrapperService _eventingWrapper;
        private const string ClassName = nameof(CohortingEventingService);

        public CohortingEventingService(
            ILogger<CohortingEventingService> logger,
            Helpers.Interfaces.ISecretHelper secretHelper,
            IRedshiftDataReader redshiftDataReaderService,
            ITenantRepo tenantRepo,
            IMapper mapper,
            IEventingWrapperService eventingWrapper)
        {
            _logger = logger;
            _redshiftConnectionString = secretHelper.GetRedshiftConnectionString().Result;
            _redshiftDataReaderService = redshiftDataReaderService;
            _tenantRepo = tenantRepo;
            _mapper = mapper;
            _eventingWrapper = eventingWrapper;
        }

        public async Task CohortingEventingAsync(EtlExecutionContext etlExecutionContext, string jobId)
        {
            const string methodName = nameof(CohortingEventingAsync);
            try
            {
                int totalProcessed = 0;
                int iteration = 0;

                while (true)
                {
                    iteration++;

                    var tenant = await _tenantRepo.FindOneAsync(x => x.PartnerCode == etlExecutionContext.PartnerCode && x.DeleteNbr == 0);

                    if (tenant == null)
                    {
                        _logger.LogError("{ClassName}.{MethodName}: Tenant not found for PartnerCode: {PartnerCode}",
                            ClassName, methodName, etlExecutionContext.PartnerCode);
                        continue;
                    }

                    // Fetch unclaimed rows from redshift for the latest file
                    var unclaimedRows = await _redshiftDataReaderService.FetchAndClaimCohortBatchAsync(
                        _redshiftConnectionString,
                        etlExecutionContext.PartnerCode, jobId,
                        etlExecutionContext.BatchSize);

                    if (unclaimedRows == null || !unclaimedRows.Any())
                    {
                        _logger.LogInformation("{ClassName}.{MethodName}: No more unclaimed rows found after {Iterations} iterations. Total processed: {Count}.",
                            ClassName, methodName, iteration, totalProcessed);
                        break;
                    }

                    _logger.LogInformation("{ClassName}.{MethodName}: Processing batch #{BatchNumber} with {Count} claimed rows.",
                        ClassName, methodName, iteration, unclaimedRows.Count);

                    // Process rows and prepare messages
                    var messages = new List<(string EventMessage, long RowId, string EventId, string EventType, string PersonUniqueIdentifier)>();

                    foreach (var row in unclaimedRows)
                    {
                        totalProcessed++;

                        try
                        {
                            row.TenantCode = tenant.TenantCode;
                            row.CustomerLabel = etlExecutionContext.CustomerLabel;
                            row.CustomerCode = etlExecutionContext.CustomerCode;
                            // Build event header
                            var eventHeaderDto = new EventHeaderDto
                            {
                                EventId = Guid.NewGuid().ToString("N"),
                                EventType = AdminConstants.CohortingEvent,
                                EventSubtype = AdminConstants.CohortingEventSubType,
                                PublishTs = DateTime.UtcNow,
                                SourceModule = "ETL"
                            };

                            string eventMessage = JsonSerializer.Serialize(new EventDto<RedShiftCohortDataDto>
                            {
                                Header = eventHeaderDto,
                                Data = row
                            });

                            messages.Add((eventMessage, row.ConsumerCohortImportId, eventHeaderDto.EventId, eventHeaderDto.EventType, row.PersonUniqueIdentifier));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "{ClassName}.{MethodName}: Error preparing event for ConsumerCohortImportId: {ConsumerCohortImportId}",
                                ClassName, methodName, row.ConsumerCohortImportId);

                            await _redshiftDataReaderService.MarkCohortPublishStatusAsync(
                                _redshiftConnectionString,
                                row.ConsumerCohortImportId,
                                "ERROR");
                        }
                    }

                    // Publish messages
                    var results = await _eventingWrapper.PublishMessagesInParallelAsync(messages, jobId, AdminConstants.CohortingEventTopicName);

                    // Update statuses in RedShift
                    var dbUpdates = results
                        .Select(r => (r.RowId, r.Published ? "PUBLISHED" : "ERROR"))
                        .ToList();

                    await _redshiftDataReaderService.MarkCohortPublishStatusBatchAsync(_redshiftConnectionString, dbUpdates);

                    _logger.LogInformation(
                        "{ClassName}.{MethodName}: Completed batch #{BatchNumber}. Total processed so far: {Total}.",
                        ClassName, methodName, iteration, totalProcessed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error: {ErrorMessage}",
                    ClassName, methodName, ex.Message);
                throw;
            }
        }
    }
}
