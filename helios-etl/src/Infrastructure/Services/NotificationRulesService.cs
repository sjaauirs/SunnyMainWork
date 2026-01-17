using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using System.Security.Cryptography;
using System.Text.Json;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class NotificationRulesService : INotificationRulesService
    {

        private readonly INotificationClient _notificationClient;
        private readonly IAdminClient _adminClient;
        private readonly ILogger<NotificationRulesService> _logger;
        private readonly INotificationRuleRepository _notificationRulesRepo;
        private readonly IConsumerNotificationRepo _consumerNotificationRepo;
        private readonly IHeliosEventPublisher<Dictionary<string, object>> _eventPublisher;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
        private const int BatchSize = 100;

        private const string className = nameof(NotificationRulesService);
        public NotificationRulesService(ILogger<NotificationRulesService> logger, INotificationClient notificationClient, IAdminClient adminClient,
            INotificationRuleRepository notificationRulesRepo,
            IHeliosEventPublisher<Dictionary<string, object>> eventPublisher, IMemoryCache memoryCache, IConsumerNotificationRepo consumerNotificationRepo)
        {
            _logger = logger;
            _notificationClient = notificationClient;
            _notificationRulesRepo = notificationRulesRepo;
            _eventPublisher = eventPublisher;
            _adminClient = adminClient;
            _cache = memoryCache;
            _consumerNotificationRepo = consumerNotificationRepo;
        }

        /// <summary>
        /// Asynchronously processes notification rules for a given tenant code.
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task ProcessNotificationRulesAsync(EtlExecutionContext etlExecutionContext)
        {
            const string MethodName = nameof(ProcessNotificationRulesAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Notification ETL process started for TenantCode: {TenantCode}.",
                className, MethodName, etlExecutionContext.TenantCode);

            try
            {
                var tenants = await GetTenantCodesAsync(etlExecutionContext.TenantCode);
                if (tenants == null || tenants.Count == 0)
                {
                    LogAndThrowInvalidTenantCode(MethodName, etlExecutionContext.TenantCode);
                }

                foreach (var tenant in tenants!)
                {
                    try
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Processing tenant: {TenantCode}",
                            className, MethodName, tenant.TenantCode);

                        var rules = await FetchNotificationRulesAsync(tenant.TenantCode!);
                        if (rules == null || rules.Count == 0)
                        {
                            _logger.LogWarning("{ClassName}.{MethodName} - No notification rules found for tenant: {TenantCode}",
                                className, MethodName, tenant.TenantCode);
                            continue;
                        }

                        foreach (var rule in rules)
                        {
                            try
                            {
                                _logger.LogInformation("{ClassName}.{MethodName} - Processing rule: {RuleCode} for tenant: {TenantCode}",
                                    className, MethodName, rule.NotificationRuleCode, tenant.TenantCode);

                                var contextConfig = DeserializeJson<NotificationContextConfig>(rule.ContextConfig);
                                var frequencyConfig = DeserializeJson<NotificationFrequencyConfig>(rule.FrequencyConfig);

                                if (contextConfig == null)
                                {
                                    _logger.LogWarning("{ClassName}.{MethodName} - Skipping rule: {RuleCode}, tenant: {TenantCode}. Reason: ContextConfig not found.",
                                        className, MethodName, rule.NotificationRuleCode, tenant.TenantCode);
                                    continue;
                                }

                                if (!await ShouldRunBasedOnFrequency(frequencyConfig, rule.NotificationRuleId))
                                {
                                    _logger.LogInformation("{ClassName}.{MethodName} - Skipping rule: {RuleCode} due to frequency constraints, tenant: {TenantCode}",
                                        className, MethodName, rule.NotificationRuleCode, tenant.TenantCode);
                                    continue;
                                }

                                var notificationEventType = await GetNotificationEventType(rule.NotificationEventTypeId);
                                if (notificationEventType == null || string.IsNullOrEmpty(notificationEventType.NotificationEventName))
                                {
                                    _logger.LogWarning("{ClassName}.{MethodName} - Skipping rule: {RuleCode}, tenant: {TenantCode}. Reason: NotificationEventType not found or invalid.",
                                        className, MethodName, rule.NotificationRuleCode, tenant.TenantCode);
                                    continue;
                                }

                                var notificationCategory = await GetNotificationCategory(notificationEventType.NotificationCategoryId);
                                if (notificationCategory == null || string.IsNullOrEmpty(notificationCategory.CategoryName))
                                {
                                    _logger.LogWarning("{ClassName}.{MethodName} - Skipping rule: {RuleCode}, tenant: {TenantCode}. Reason: NotificationCategory not found or invalid for eventTypeId: {EventTypeId}.",
                                        className, MethodName, rule.NotificationRuleCode, tenant.TenantCode, notificationEventType.NotificationEventTypeId);
                                    continue;
                                }

                                await ProcessEventRetrievalQuery(rule, contextConfig,  notificationCategory.CategoryName, notificationEventType.NotificationEventName);

                                _logger.LogInformation("{ClassName}.{MethodName} - Successfully processed rule: {RuleCode} for tenant: {TenantCode}",
                                    className, MethodName, rule.NotificationRuleCode, tenant.TenantCode);
                            }
                            catch (Exception ruleEx)
                            {
                                _logger.LogError(ruleEx, "{ClassName}.{MethodName} - Error processing rule. ErrorMessage: {ErrorMessage}, ErrorCode: {ErrorCode}, RuleData: {RuleData}, TenantCode: {TenantCode}. Skipping rule.",
                                    className, MethodName, ruleEx.Message, rule.ToJson(), tenant.TenantCode, StatusCodes.Status500InternalServerError);
                                continue;
                            }
                        }

                        _logger.LogInformation("{ClassName}.{MethodName} - Completed processing all rules for tenant: {TenantCode}",
                            className, MethodName, tenant.TenantCode);
                    }
                    catch (Exception tenantEx)
                    {
                        _logger.LogError(tenantEx, "{ClassName}.{MethodName} - Error processing tenant: {TenantCode}. ErrorMessage: {ErrorMessage}, ErrorCode: {ErrorCode}. Skipping tenant.",
                            className, MethodName, tenant.TenantCode, tenantEx.Message, StatusCodes.Status500InternalServerError);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Critical error during notification ETL process. ErrorMessage: {ErrorMessage}, ErrorCode: INTERNAL_ERROR.",
                    className, MethodName, ex.Message);
                throw;
            }

            _logger.LogInformation("{ClassName}.{MethodName} - Notification ETL process completed for TenantCode: {TenantCode}.",
                className, MethodName, etlExecutionContext.TenantCode);
        }

        /// <summary>
        /// Fetches the list of active notification rules for a given tenant.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        private async Task<List<NotificationRuleDto>> FetchNotificationRulesAsync(string tenantCode)
        {
            string methodName = nameof(FetchNotificationRulesAsync);
            var requestDto = new GetAllNotificationRulesRequestDto { TenantCode = tenantCode };
            var response = await GetActiveRulesAsync(requestDto);

            if (response?.NotificationRuleList == null || !response.NotificationRuleList.Any() || response.ErrorCode != null)
            {
                _logger.LogWarning("{ClassName}.{MethodName} - Error fetching notification rules or no active rules found. Request: {Request}, Response: {Response}",
                    className, methodName, requestDto.ToJson(), response?.ToJson());
                return new List<NotificationRuleDto>();
            }

            return (List<NotificationRuleDto>)response.NotificationRuleList;
        }

        /// <summary>
        /// Processes an event retrieval query for a given notification rule and context configuration.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="contextConfig"></param>
        /// <returns></returns>
        public async Task ProcessEventRetrievalQuery(NotificationRuleDto rule, NotificationContextConfig contextConfig, string eventType, string eventSubType)
        {
            const string methodName = nameof(ProcessEventRetrievalQuery);
            var processedContexts = new List<NotificationRuleProcessedContextDto>();
            int totalRecords = 0;
            int failedRecords = 0;
            string executionStatus = NotificationExecutionStatus.SUCCESS.ToString();
            List<string> failedMessages = new List<string>();

            try
            {
                int offset = 0; // Start at the first batch
                bool hasMoreRecords;

                do
                {
                    // Fetch next batch of records
                    var results = FetchPaginatedResultsAsync(rule.EventRetrievalQuery, offset, BatchSize);
                    if (results == null || results.Count == 0)
                    {
                        hasMoreRecords = false;
                        continue;
                    }
                    hasMoreRecords = results.Count > 0;
                    offset += results.Count; // Move to next batch

                    foreach (var result in results)
                    {
                        var notificationEventData = CreateNotificationEventData(result, contextConfig);

                        if (notificationEventData.ContainsKey("ConsumerCode") && notificationEventData["ConsumerCode"] is string consumerCode)
                        {
                            var consumerLatestNotification = await _consumerNotificationRepo.GetConsumerNotification(consumerCode, rule.TenantCode, rule.NotificationRuleId);
                            
                            if (consumerLatestNotification != null && consumerLatestNotification.CreateTs.AddSeconds(rule.Ttl) > DateTime.UtcNow)
                            {
                                _logger.LogInformation("{ClassName}.{MethodName} - Skipping notification for ConsumerCode: {ConsumerCode}, Rule: {RuleCode}. Reason: Notification already sent within ttl.",
                                    className, methodName, consumerCode, rule.NotificationRuleCode);
                                continue;
                            }
                        }

                        notificationEventData["NotificationRuleId"] = rule.NotificationRuleId;

                        var notificationEvent = new EventDto<Dictionary<string, object>>
                        {
                            Header = await CreateEventHeader(rule, notificationEventData, eventType, eventSubType),
                            Data = notificationEventData
                        };


                        var publishResult = await PublishMessageWithRetries(notificationEvent);

                        if (!publishResult.Published)
                        {
                            failedRecords++;
                            failedMessages.Add($"ConsumerCode: {notificationEvent.Header.ConsumerCode}, ContextId: {contextConfig?.ContextId}, ErrorMessage: {publishResult.ErrorMessage}");
                        }
                        else
                        {
                            processedContexts.Add(new NotificationRuleProcessedContextDto
                            {
                                ContextType = contextConfig?.ContextType,
                                ContextId = contextConfig?.ContextId,
                                RecordsCount = 1,
                                ProcessedAt = DateTime.UtcNow,
                                ContextAttributes = contextConfig?.ContextAttributes
                            });

                            totalRecords++;
                        }
                    }

                    // Log progress after every batch
                    _logger.LogInformation("{ClassName}.{MethodName} - Processed {TotalRecords} records so far for Rule: {RuleCode}",
                        className, methodName, totalRecords, rule.NotificationRuleCode);

                } while (hasMoreRecords);

                if (totalRecords > 0 && failedRecords > 0)
                {
                    executionStatus = NotificationExecutionStatus.PARTIAL_FAILURE.ToString();
                }
                else if (totalRecords == 0 && failedRecords > 0)
                {
                    executionStatus = NotificationExecutionStatus.FAILED.ToString();
                }
            }
            catch (Exception ex)
            {
                if (totalRecords > 0)
                {
                    executionStatus = NotificationExecutionStatus.PARTIAL_FAILURE.ToString();
                }
                else
                {
                    executionStatus = NotificationExecutionStatus.FAILED.ToString();
                }

                string errorMessage = $"Error processing rule {rule.NotificationRuleCode}: {ex.Message}";
                failedMessages.Add(errorMessage);
                _logger.LogError(ex, "{ClassName}.{MethodName} - {ErrorMessage}", className, methodName, errorMessage);
            }
            
            var groupedProcessedContexts = processedContexts
                .GroupBy(pc => pc.ContextType)
                .Select(group => new
                {
                    ContextType = group.Key,
                    RecordsCount = group.Count(),
                    ContextIds = group.Select(pc => pc.ContextId).Distinct().ToList() // Collect unique ContextIds
                })
                .ToList();

            var historyRecord = new CreateNotificationRuleHistoryRequestDto
            {
                NotificationRuleId = rule.NotificationRuleId,
                ExecutedTs = DateTime.UtcNow,
                ExecutionStatus = executionStatus,
                RecordsProcessed = JsonSerializer.Serialize(new
                {
                    contexts = groupedProcessedContexts,
                    totalRecords = totalRecords,
                    failedMessages = failedMessages
                }),
                CreateUser = Constants.CreateUserAsETL
            };

            await SaveExecutionHistoryAsync(historyRecord);
        }

        private async Task<PublishResultDto> PublishMessageWithRetries(EventDto<Dictionary<string, object>> notificationEvent)
        {
            int maxTries = NotificationConstants.MaxTries;
            PublishResultDto publishResult = new PublishResultDto
            {
                Published = false,
                ErrorMessage = string.Empty
            };

            while (maxTries > 0)
            {
                try
                {
                    publishResult = await _eventPublisher.PublishMessage(notificationEvent.Header, notificationEvent.Data);
                    if (publishResult.Published)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName}: Message published successfully. Retries left: {MaxTries}, NotificationEventData: {NotificationEventData}",
                            className, nameof(PublishMessageWithRetries), maxTries, notificationEvent.ToJson());
                        break;
                    }

                    _logger.LogWarning("{ClassName}.{MethodName}: Message publishing failed. Retries left: {MaxTries}, ErrorMessage: {ErrorMessage}, NotificationEventData: {NotificationEventData}",
                        className, nameof(PublishMessageWithRetries), maxTries, publishResult.ErrorMessage, notificationEvent.ToJson());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while publishing message. Retries left: {MaxTries}, NotificationEventData: {NotificationEventData}",
                        className, nameof(PublishMessageWithRetries), maxTries, notificationEvent.ToJson());
                }

                maxTries--;
                if (maxTries > 0)
                {
                    await Task.Delay(GetSecureRandomNumber(NotificationConstants.RetryMinWaitMS, NotificationConstants.RetryMaxWaitMS));
                }
            }

            return publishResult;
        }

        private int GetSecureRandomNumber(int minValue, int maxValue)
        {
            var buffer = new byte[4];
            RandomNumberGenerator.Fill(buffer);
            int result = BitConverter.ToInt32(buffer, 0) & int.MaxValue;
            return minValue + (result % (maxValue - minValue));
        }

        private List<Dictionary<string, object>>? FetchPaginatedResultsAsync(string query, int offset, int limit)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }
            try
            {
                // Remove trailing `;` if present
                string sanitizedQuery = query.TrimEnd().TrimEnd(';', ' ');
                string paginatedQuery = $"{sanitizedQuery} LIMIT {limit} OFFSET {offset}";
                var results = _notificationRulesRepo.ExecuteQuery(paginatedQuery);
                return results?.Select(x => x).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error executing paginated query. Query: {Query}, Offset: {Offset}, Limit: {Limit}",
           className, nameof(FetchPaginatedResultsAsync), query, offset, limit);
                return null;
            }

        }



        private Dictionary<string, object> CreateNotificationEventData(IDictionary<string, object> result, NotificationContextConfig? contextConfig)
        {
            var notificationEventData = result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? string.Empty);
            notificationEventData["ContextType"] = contextConfig?.ContextType ?? string.Empty;
            notificationEventData["ContextId"] = contextConfig?.ContextId ?? string.Empty;
            notificationEventData["AdditionalAttributes"] = contextConfig?.ContextAttributes ?? new Dictionary<string, string>();
            return notificationEventData;
        }

        private async Task<EventHeaderDto> CreateEventHeader(NotificationRuleDto rule, Dictionary<string, object> notificationEventData,
            string eventType, string eventSubType)
        {
            return new EventHeaderDto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventType = eventType,
                EventSubtype = eventSubType,
                PublishTs = DateTime.UtcNow,
                TenantCode = rule.TenantCode,
                ConsumerCode = notificationEventData.ContainsKey("ConsumerCode") ? notificationEventData["ConsumerCode"]?.ToString() ?? string.Empty : string.Empty,
                SourceModule = NotificationConstants.NotificationEventTSourceModule
            };
        }

        private async Task<string> GetEventType(NotificationRuleDto rule)
        {
            var notificationEventType = await GetNotificationEventType(rule.NotificationEventTypeId);
            return notificationEventType?.NotificationEventName ?? string.Empty;
        }

        private T? DeserializeJson<T>(string json) where T : class
        {
            return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<T>(json);
        }


        /// <summary>
        /// Determines if a notification rule should run based on execution frequency and last execution history.
        /// </summary>
        private async Task<bool> ShouldRunBasedOnFrequency(NotificationFrequencyConfig? frequencyConfig, long ruleId)
        {
            const string MethodName = nameof(ShouldRunBasedOnFrequency);
            var nowUtc = DateTime.UtcNow;

            // Schedule check: run only if current time is within 5 minutes of the scheduled time
            if (frequencyConfig?.Schedule.HasValue == true)
            {
                var scheduledTime = frequencyConfig.Schedule.Value;
                var nowTime = TimeOnly.FromDateTime(nowUtc);
                if (Math.Abs((nowTime - scheduledTime).TotalMinutes) > 5)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Skipping rule {RuleId} due to schedule time mismatch. Now: {Now}, Schedule: {Schedule}", className, MethodName, ruleId, nowTime, scheduledTime);
                    return false;
                }
            }

            if (frequencyConfig?.Interval?.ToUpper() == nameof(NotificationFrequencyInterval.ADHOC) && frequencyConfig.Date.HasValue && frequencyConfig.Date.Value != DateOnly.FromDateTime(nowUtc))
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Skipping ADHOC rule {RuleId} as today does not match the scheduled date {ScheduledDate}", className, MethodName, ruleId, frequencyConfig.Date.Value);
                return false;
            }

            // Fetch last execution from notification_rule_history
            var getAllNotificationRuleHistoryResponse = await GetAllNotificationRuleHistory(ruleId);

            if (getAllNotificationRuleHistoryResponse == null || getAllNotificationRuleHistoryResponse.ErrorCode == StatusCodes.Status404NotFound)
            {
                return true;
            }
            if (getAllNotificationRuleHistoryResponse.ErrorCode != null)
            {
                _logger.LogWarning("{ClassName}.{MethodName} - Error occurred while fetching notification rule history by rule id, RuleId:{RuleId}, ErrorResponse: {Response}", className, MethodName, ruleId, getAllNotificationRuleHistoryResponse?.ToJson());
                return false;
            }
            var lastExecution = getAllNotificationRuleHistoryResponse?.NotificationRuleHistoryList?.OrderByDescending(x => x.ExecutedTs)?.FirstOrDefault(x => x.ExecutedTs != null);
            
            if (lastExecution == null)
            {
                _logger.LogInformation("{ClassNanme}.{MethodName} - Rule {RuleId} has no execution history. Allowing execution.", className, MethodName, ruleId);
                return true; // No prior execution, allow running
            }

            var lastExecutedUtc = lastExecution.ExecutedTs!.Value;
            _logger.LogInformation("{ClassName}.{MethodName} - Rule {RuleId} last executed at {LastExecutedUtc}", className, MethodName, ruleId, lastExecutedUtc);

            switch (frequencyConfig?.Interval?.ToUpper())
            {
                case nameof(NotificationFrequencyInterval.DAILY):
                    if (lastExecutedUtc.Date == nowUtc.Date)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Skipping rule {RuleId} (DAILY) as it was already executed today at {LastExecutedUtc}", className, MethodName, ruleId, lastExecutedUtc);
                        return false;
                    }
                    _logger.LogInformation("{ClassName}.{MethodName} - Running rule {RuleId} (DAILY) as it has not been executed today.", className, MethodName, ruleId);
                    return true;

                case nameof(NotificationFrequencyInterval.WEEKLY):
                    if (frequencyConfig.Day == (int)nowUtc.DayOfWeek && lastExecutedUtc.Date.AddDays(7) > nowUtc.Date)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Skipping rule {RuleId} (WEEKLY) as it was already executed this week at {LastExecutedUtc}", className, MethodName, ruleId, lastExecutedUtc);
                        return false;
                    }
                    _logger.LogInformation("{ClassName}.{MethodName} - Running rule {RuleId} (WEEKLY) as today is the scheduled execution day.", className, MethodName, ruleId);
                    return frequencyConfig.Day == (int)nowUtc.DayOfWeek;

                case nameof(NotificationFrequencyInterval.MONTHLY):
                    if (frequencyConfig.Day == nowUtc.Day && lastExecutedUtc.Month == nowUtc.Month && lastExecutedUtc.Year == nowUtc.Year)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Skipping rule {RuleId} (MONTHLY) as it was already executed this month at {LastExecutedUtc}", className, MethodName, ruleId, lastExecutedUtc);
                        return false;
                    }
                    _logger.LogInformation("{ClassName}.{MethodName} - Running rule {RuleId} (MONTHLY) as today matches the scheduled execution day.", className, MethodName, ruleId);
                    return frequencyConfig.Day == nowUtc.Day;

                case nameof(NotificationFrequencyInterval.ADHOC):
                    if (frequencyConfig.Date.HasValue && DateOnly.FromDateTime(lastExecutedUtc) == DateOnly.FromDateTime(nowUtc))
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Skipping ADHOC rule {RuleId} as it was already executed today at {LastExecutedUtc}", className, MethodName, ruleId, lastExecutedUtc);
                        return false;
                    }
                    _logger.LogInformation("{ClassName}.{MethodName} - Running rule {RuleId} (ADHOC). Adhoc rules are always allowed to execute.", className, MethodName, ruleId);
                    return true;

                default:
                    _logger.LogWarning("{ClassName}.{MethodName} - Rule {RuleId} has an invalid interval {Interval}. Defaulting to execute.", className, MethodName, ruleId, frequencyConfig.Interval);
                    return true;
            }
        }

        private async Task<DateTime?> GetLastExecutionTime(long ruleId)
        {
            var response = await GetAllNotificationRuleHistory(ruleId);
            if (response == null || response.ErrorCode == StatusCodes.Status404NotFound)
            {
                return null; // No history found
            }
            if (response.ErrorCode != null)
            {
                _logger.LogWarning("{ClassName}.{MethodName} - Error fetching history for RuleId: {RuleId}, Response: {Response}",
                    className, nameof(GetLastExecutionTime), ruleId, response.ToJson());
                return null;
            }

            return response.NotificationRuleHistoryList?
                .Where(x => x.ExecutedTs != null)
                .OrderByDescending(x => x.ExecutedTs)
                .FirstOrDefault()?.ExecutedTs;
        }

        private void LogAndThrowInvalidTenantCode(string methodName, string? tenantCode)
        {
            _logger.LogError("{ClassName}.{MethodName} - Invalid tenant code in job params: {TenantCode}. ErrorCode: {Code}",
                className, methodName, tenantCode, StatusCodes.Status500InternalServerError);

            throw new ETLException(ETLExceptionCodes.NullValue, $"Invalid tenant code in job params: {tenantCode}. ErrorCode: {StatusCodes.Status500InternalServerError}");
        }

        private async Task<GetAllNotificationRuleResponseDto> GetActiveRulesAsync(GetAllNotificationRulesRequestDto getAllNotificationRulesRequestDto)
        {
            return await _notificationClient.Post<GetAllNotificationRuleResponseDto>(NotificationConstants.GetAllNotificationRulesAPIUrl, getAllNotificationRulesRequestDto);
        }

        private async Task<GetAllNotificationRuleHistoryResponseDto> GetAllNotificationRuleHistory(long ruleId)
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _notificationClient.Get<GetAllNotificationRuleHistoryResponseDto>($"{NotificationConstants.GetAllNotificationRuleHistoryAPIUrl}?notificationRuleId={ruleId}", parameters);
        }

        public async Task<NotificationEventTypeResponseDto> GetNotificationEventTypeById(long notificationEventTypeId)
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _notificationClient.Get<NotificationEventTypeResponseDto>($"{NotificationConstants.GetAllNotificationRuleHistoryAPIUrl}/{notificationEventTypeId}", parameters);
        }


        private async Task<NotificationEventTypeDto?> GetNotificationEventType(long notificationEventTypeId)
        {
            string cacheKey = $"NotificationEventType_{notificationEventTypeId}";

            // Check if the response is already cached
            if (_cache.TryGetValue(cacheKey, out NotificationEventTypeDto? cachedResponse))
            {
                return cachedResponse; // Return cached value
            }

            IDictionary<string, long> parameters = new Dictionary<string, long>();
            var response = await _notificationClient.Get<NotificationEventTypeResponseDto>(
                $"{NotificationConstants.GetNotificationEventTypeAPIUrl}?notificationEventTypeId={notificationEventTypeId}",
                parameters
            );

            if (response == null || response?.ErrorCode != null)
            {
                _logger.LogError("{ClassName}.{MethodName}: Error occurred while retrieving Notification Event Type. notificationEventTypeId: {NotificationEventTypeId}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                    className, nameof(GetNotificationEventType), notificationEventTypeId, response?.ToJson(), response?.ErrorCode);
            }

            // Cache the response if it's valid
            if (response?.NotificationEventType != null)
            {
                _cache.Set(cacheKey, response?.NotificationEventType, _cacheDuration);
            }

            return response?.NotificationEventType;
        }

        private async Task<NotificationCategoryDto?> GetNotificationCategory(long? notificationCategoryId)
        {
            const string methodName = nameof(GetNotificationCategory);
           
            if(notificationCategoryId == null)
            {
                return null;
            }

            string cacheKey = $"NotificationCategory_{notificationCategoryId}";

            if (_cache.TryGetValue(cacheKey, out NotificationCategoryDto? cachedResponse))
            {
                return cachedResponse;
            }

            var response = await _notificationClient.Get<GetAllNotificationCategoriesResponseDto>(
                NotificationConstants.GetAllCatetoriesAPIUrl,
                new Dictionary<string, long>()
            );

            if (response == null || response.ErrorCode != null)
            {
                _logger.LogError("{ClassName}.{MethodName}: Error retrieving Notification Categories. Response: {ResponseData}, ErrorCode: {ErrorCode}",
                    className, methodName, response?.ToJson(), response?.ErrorCode);
                return null;
            }

            if (response?.NotificationCategoriesList == null || response?.NotificationCategoriesList?.Count == 0)
            {
                _logger.LogWarning("{ClassName}.{MethodName}: No Notification Categories found. Response: {ResponseData}",
                    className, methodName, response?.ToJson());
                return null;
            }

            var notificationCategory = response?.NotificationCategoriesList?.FirstOrDefault(x => x.NotificationCategoryId == notificationCategoryId && x.DeleteNbr == 0);
            if (notificationCategory == null)
            {
                return null;
            }

            _cache.Set(cacheKey, notificationCategory, _cacheDuration);
            return notificationCategory;
        }



        public async Task SaveExecutionHistoryAsync(CreateNotificationRuleHistoryRequestDto createNotificationRuleHistoryRequestDto)
        {
            const string MethodName = nameof(SaveExecutionHistoryAsync);
            var response = await _notificationClient.Post<NotificationRuleHistoryResponseDto>(NotificationConstants.CreateNotificationRuleHistoryAPIUrl, createNotificationRuleHistoryRequestDto);
            if (response == null || response?.ErrorCode != null)
            {
                _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating Notification Rule History. notificationRuleId: {NotificationRuleHistoryId}, Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                    className, MethodName, createNotificationRuleHistoryRequestDto.NotificationRuleId, createNotificationRuleHistoryRequestDto.ToJson(), response?.ToJson(), response?.ErrorCode);
            }
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
    }
}
