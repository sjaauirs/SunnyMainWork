using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RulesEngine.Models;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ConsumerCohortEventProcessor : IConsumerCohortEventProcessor
    {
        private const string startLogTemplate = "{className}.{methodName}: Started processing.. Request: {Request}";
        private const string endLogTemplate = "{className}.{methodName}: Ended processing successfully.";
        private const string errorLogTemplate = "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}";

        private readonly ILogger<ConsumerCohortEventProcessor> _logger;
        private readonly IUserClient _userClient;
        private readonly ICohortClient _cohortClient;
        private readonly IConsumerCohortRuleProcessor _consumerCohortRuleProcessor;
        const string className = nameof(ConsumerCohortEventProcessor);
        public ConsumerCohortEventProcessor(
            ILogger<ConsumerCohortEventProcessor> logger,
            IUserClient userClient, ICohortClient cohortClient, IConsumerCohortRuleProcessor consumerCohortRuleProcessor)
        {
            _logger = logger;
            _userClient = userClient;
            _cohortClient = cohortClient;
            _consumerCohortRuleProcessor = consumerCohortRuleProcessor;
        }

        public async Task<bool> ProcessEvent(EventDto<CohortEventDto> eventRequest) 
        {
            const string methodName = nameof(ProcessEvent);

            try
            {

                var consumerInfo = await GetConsumer(eventRequest.Header.ConsumerCode!);
                var personInfo = await GetpersonInfo(consumerInfo.Consumer.PersonId);

                var cohortRuleInput = new CohortRuleInput() { Person = personInfo, Consumer = consumerInfo.Consumer };

                var cohorts = await GetCohorts(consumerInfo.Consumer.TenantCode!);

                var consumerCohorts = await GetConsumerCohorts(consumerInfo.Consumer);

                

                var cohortsWithValidRules = (cohorts.Cohort ?? Enumerable.Empty<CohortDto>())
                .Select(c => new { Cohort = c, Rule = TryDeserializeRule(c.CohortRule) })
                .Where(x => x.Rule?.RuleExpr != null && x.Rule.RuleExpr != "{}")
                .ToList();

                var cohortsWithValidArrayRules = (cohorts.Cohort ?? Enumerable.Empty<CohortDto>())
                .Select(c => new { Cohort = c, Rule = TryDeserializeArrayRule(c.CohortRule) })
                .Where(x => x.Rule?.RuleExpr != null && x.Rule.RuleExpr.Any())
                .ToList();


                //convert all strings rules into array rules

                var allCohortsWithRule = cohortsWithValidArrayRules
                 .Concat(
                     cohortsWithValidRules.Select(x => new
                     {
                         Cohort = x.Cohort,
                         Rule = new CohortRuleArrayJson
                         {
                             RuleExpr = new List<string> { x.Rule!.RuleExpr!.Trim() },
                             SuccessExpr = string.IsNullOrWhiteSpace(x.Rule!.SuccessExpr) ? "true" : x.Rule!.SuccessExpr
                         }
                     })
                 )
                 .ToList();

                if (allCohortsWithRule.Count == 0)
                {
                    _logger.LogInformation("{Class}.{Method}: No cohorts with valid rules to evaluate.", className, methodName);
                    return true; // nothing to do
                }

                var existingCohortNames = (consumerCohorts?.Cohorts ?? Enumerable.Empty<CohortDto>())
                   .Select(c => c.CohortName)
                   .Where(n => !string.IsNullOrWhiteSpace(n))
                   .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var toAdd = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var toRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var ch in allCohortsWithRule)
                {
                    var cohortName = ch.Cohort.CohortName;
                    if (string.IsNullOrWhiteSpace(cohortName)) continue;

                    var eval = await _consumerCohortRuleProcessor.EvaluateRule(cohortName, ch.Rule!, cohortRuleInput);

                    bool isMember = existingCohortNames.Contains(cohortName);
                    bool shouldBeMember = eval.RulesExecutionResult;

                    if (shouldBeMember && !isMember) toAdd.Add(cohortName);
                    else if (!shouldBeMember && isMember) toRemove.Add(cohortName);
                }

                if (toAdd.Count == 0 && toRemove.Count == 0)
                {
                    _logger.LogInformation("{Class}.{Method}: No cohorts to add or remove for consumer {ConsumerCode}.",
                        className, methodName, consumerInfo.Consumer.ConsumerCode);
                    return false;
                }

                var allOk = true;

                // Apply adds
                foreach (var cohortName in toAdd)
                {
                    var addReq = new CohortConsumerRequestDto
                    {
                        ConsumerCode = consumerInfo.Consumer.ConsumerCode!,
                        CohortName = cohortName,
                        TenantCode = consumerInfo.Consumer.TenantCode!,
                        EventSource = "",
                        EventId = eventRequest.Data.EventId ?? Guid.NewGuid().ToString("N"),
                        ProcessedBy = className
                    };

                    var addResp = await AddConsumerCohort(addReq);
                    if (addResp.ErrorCode != null)
                    {
                        _logger.LogWarning(
                            "{Class}.{Method}: Failed to add consumer {Consumer} to cohort {Cohort}. ErrorCode: {ErrorCode}, Message: {Message}",
                            className, methodName, consumerInfo.Consumer.ConsumerCode, cohortName, addResp.ErrorCode, addResp.ErrorMessage);
                        allOk = false;
                    }
                    else
                    {
                        _logger.LogInformation("{Class}.{Method}: Added consumer {Consumer} to cohort {Cohort}.",
                            className, methodName, consumerInfo.Consumer.ConsumerCode, cohortName);
                    }
                }


                // Apply removes
                foreach (var cohortName in toRemove)
                {
                    var removeReq = new CohortConsumerRequestDto
                    {
                        ConsumerCode = consumerInfo.Consumer.ConsumerCode!,
                        CohortName = cohortName,
                        TenantCode = consumerInfo.Consumer.TenantCode!,
                        EventSource = "",
                        EventId = eventRequest.Data.EventId ?? Guid.NewGuid().ToString("N"),
                        ProcessedBy = className
                    };

                    var removeResp = await RemoveConsumerCohort(removeReq);
                    if (removeResp.ErrorCode != null)
                    {
                        _logger.LogWarning(
                            "{Class}.{Method}: Failed to remove consumer {Consumer} from cohort {Cohort}. ErrorCode: {ErrorCode}, Message: {Message}",
                            className, methodName, consumerInfo.Consumer.ConsumerCode, cohortName, removeResp.ErrorCode, removeResp.ErrorMessage);
                        allOk = false;
                    }
                    else
                    {
                        _logger.LogInformation("{Class}.{Method}: Removed consumer {Consumer} from cohort {Cohort}.",
                            className, methodName, consumerInfo.Consumer.ConsumerCode, cohortName);
                    }
                }

                return allOk;

            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "{Class}.{Method}: Failed to deserialize EventData. Input: {EventData}", className, methodName, eventRequest);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}: Unexpected error occurred. Message: {Message}", className, methodName, ex.Message);
                return false;
            }
        }

        private async Task<GetConsumerResponseDto> GetConsumer(string consumerCode)
        {
            var GetConsumerRequestDto = new GetConsumerRequestDto() { ConsumerCode = consumerCode };
            const string methodName = nameof(GetConsumer);
            var consumer = await _userClient.Post<GetConsumerResponseDto>("consumer/get-consumer", GetConsumerRequestDto);
            if (consumer?.Consumer == null)
            {
                _logger.LogError("{className}.{methodName}: Consumer Details Not Found. Consumer Code:{consumer}, Error Code:{errorCode}", className, methodName, consumerCode, StatusCodes.Status404NotFound);
                throw new Exception($"Consumer Not found for ConsumerCode: {consumerCode}");
            }
            _logger.LogInformation("{className}.{methodName}: Retrieved Consumer Details Successfully for " +
                "ConsumerCode : {ConsumerCode}", className, methodName, consumerCode);

            return consumer;
        }

        private async Task<PersonDto> GetpersonInfo(long personId)
        {
            const string methodName = nameof(GetpersonInfo);
            var person = await _userClient.GetById<PersonDto>("person/", personId);
            if (person == null || string.IsNullOrEmpty(person.FirstName) || string.IsNullOrEmpty(person.LastName))
            {
                _logger.LogError("{className}.{methodName}: Person Details Not Found. Person Id:{Id}, Error Code:{errorCode}", className, methodName, personId, StatusCodes.Status404NotFound);
                throw new Exception($"Person Not found for personId: {personId}");
            }
            _logger.LogInformation("{className}.{methodName}: Retrieved Person Details Successfully for" +
                " PersonId : {PersonId}", className, methodName, personId);

            return person;
        }


        private async Task<TenantCohortResponseDto> GetCohorts(string tenantCode)
        {
            var requestDto = new TenantCohortRequestDto() { TenantCode = tenantCode };
            const string methodName = nameof(GetCohorts);
            _logger.LogInformation(startLogTemplate, className, methodName, requestDto.ToJson());

            var response = await _cohortClient.Post<TenantCohortResponseDto>("cohort/get-tenant-cohorts", requestDto);
            _logger.LogInformation(endLogTemplate, className, methodName);
           

            if (response.ErrorCode != null)
            {
                throw new Exception($"Error getting cohorts for a tenant tenantCode: {tenantCode}, Api Error : {response.ErrorCode} -{response.ErrorMessage}");
            }
            return response;

        }

        private async Task<CohortsResponseDto> GetConsumerCohorts(ConsumerDto consumer)
        {
            var requestDto = new ConsumerCohortsRequestDto() { ConsumerCode = consumer.ConsumerCode!, TenantCode = consumer.TenantCode! };
            const string methodName = nameof(GetConsumerCohorts);
            _logger.LogInformation(startLogTemplate, className, methodName, requestDto.ToJson());

            try
            {
                var response = await _cohortClient.Post<CohortsResponseDto>("consumer-cohorts", requestDto);
                _logger.LogInformation(endLogTemplate, className, methodName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorLogTemplate, className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new CohortsResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Internal Server Error" };
            }

        }


        /// <summary>
        /// Method to call cohort API for Add consumer from cohort_consumer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<BaseResponseDto> AddConsumerCohort(CohortConsumerRequestDto cohortConsumerRequestDto)
        {
            const string methodName = nameof(AddConsumerCohort);
            try
            {
                if (cohortConsumerRequestDto == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Invalid data format for adding consumer to cohort", className, methodName);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Invalid data format"
                    };
                }

                _logger.LogInformation("{ClassName}.{MethodName}: API call to add consumer to cohort started", className, methodName);

                var cohortResponse = await _cohortClient.Post<BaseResponseDto>("add-consumer", cohortConsumerRequestDto);

                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error while adding consumer to cohort. ErrorCode: {ErrorCode}", className, methodName, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Consumer successfully added to cohort", className, methodName);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while adding to cohort. ErrorMessage: {ErrorMessage}", className, methodName, ex.Message);
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }



        /// <summary>
        /// Method to call cohort API for Remove consumer from cohort_consumer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<BaseResponseDto> RemoveConsumerCohort(CohortConsumerRequestDto cohortConsumerRequestDto)
        {
            const string methodName = nameof(RemoveConsumerCohort);
            try
            {
                if (cohortConsumerRequestDto == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Invalid data format for removing consumer from cohort", className, methodName);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Invalid data format"
                    };
                }

                _logger.LogInformation("{ClassName}.{MethodName}: API call to remove consumer from cohort started", className, methodName);

                var cohortResponse = await _cohortClient.Post<BaseResponseDto>("remove-consumer", cohortConsumerRequestDto);

                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error while removing consumer from cohort. ErrorCode: {ErrorCode}", className, methodName, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Consumer successfully removed from cohort", className, methodName);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while removing from cohort. ErrorMessage: {ErrorMessage}", className, methodName, ex.Message);
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        private CohortRuleJson? TryDeserializeRule(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<CohortRuleJson>(json , opts);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize CohortRule: {Json}", json);
                return null;
            }
        }

        private CohortRuleArrayJson? TryDeserializeArrayRule(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<CohortRuleArrayJson>(json, opts);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize CohortRule: {Json}", json);
                return null;
            }
        }

        public async Task<bool> ProcessEvent(object eventDto)
        {
            if (eventDto is EventDto<CohortEventDto> typed)
                return await ProcessEvent(typed);

            if (eventDto is EventDto<dynamic> dyn)
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                CohortEventDto payload;

                if (dyn.Data is JsonElement je)
                {
                    payload = je.Deserialize<CohortEventDto>(opts)
                              ?? throw new InvalidOperationException("Failed to deserialize body to CohortEventDto");
                }
                else
                {
                   throw new InvalidOperationException("Failed to deserialize body to CohortEventDto");
                }

                var fixedDto = new EventDto<CohortEventDto>
                {
                    Header = dyn.Header,
                    Data = payload
                };

                return await ProcessEvent(fixedDto);
            }

            throw new ArgumentException(
                $"Invalid eventDto type. Expected EventDto<{nameof(CohortEventDto)}> but got {eventDto?.GetType().FullName}.");
        }
    }

    }
