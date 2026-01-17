using Grpc.Net.Client.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Constant = SunnyRewards.Helios.Admin.Core.Domain.Constants.Constant;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ConsumerPurseCohortAssignmentService : IConsumerPurseCohortAssignmentService
    {
        private readonly ILogger<ConsumerPurseCohortAssignmentService> _logger;
        private readonly IFisClient _fisClient;
        private readonly IConsumerPurseAssignmentService _consumerPurseAssignmentService;

        private readonly IConsumerCohortHelper _consumerCohortHelper;
        private const string ClassName = nameof(ConsumerPurseCohortAssignmentService);

        public ConsumerPurseCohortAssignmentService(ILogger<ConsumerPurseCohortAssignmentService> logger, IFisClient fisClient, IConsumerCohortHelper consumerCohortHelper, IConsumerPurseAssignmentService consumerPurseAssignmentService)
        {
            _logger = logger;
            _fisClient = fisClient;
            _consumerCohortHelper = consumerCohortHelper;
            _consumerPurseAssignmentService = consumerPurseAssignmentService;
        }

        public bool ConsumerPurseCohortAssignment(ConsumerDto consumerDto)
        {
            const string MethodName = nameof(ConsumerPurseCohortAssignment);

            try
            {
                // --- Step 1: Input Validation ---
                if (string.IsNullOrWhiteSpace(consumerDto.TenantCode) ||
                    string.IsNullOrWhiteSpace(consumerDto.ConsumerCode) ||
                    string.IsNullOrWhiteSpace(consumerDto.PlanId))
                {
                    var errorMessage = "One or more required parameters are missing.";
                    _logger.LogError(
                        "{ClassName}.{MethodName}: {ErrorMessage}. Tenant={Tenant}, Consumer={Consumer}, Plan={Plan}, ErrorCode={ErrorCode}",
                        ClassName, MethodName, errorMessage, consumerDto.TenantCode, consumerDto.ConsumerCode, consumerDto.PlanId, StatusCodes.Status400BadRequest);

                    return false;
                }
                bool Ssbci = GetIsSsbciFlag(consumerDto.ConsumerAttribute ?? string.Empty);
                _logger.LogInformation(
                    "{ClassName}.{MethodName}: Starting cohort assignment. Tenant={Tenant}, Consumer={Consumer}, Plan={Plan}",
                    ClassName, MethodName, consumerDto.TenantCode, consumerDto.ConsumerCode, consumerDto.PlanId);

                // --- Step 2: Fetch Mappings ---
                var planCohortPurseMappings = GetPlanCohortPurseMappingAsync(consumerDto.PlanId);
                var mappings = planCohortPurseMappings?.PlanCohortPurseMappings?.ToList();

                var defaultCohortPurseMappings = GetPlanCohortPurseMappingAsync(Constant.DefaultPlanId);
                var defaultMappings = defaultCohortPurseMappings?.PlanCohortPurseMappings?.ToList();

                if (!(mappings?.Any() ?? false) && !(defaultMappings?.Any() ?? false))
                {
                    _logger.LogWarning(
                        "{ClassName}.{MethodName}: No cohort purse mappings found. Tenant={Tenant}, Consumer={Consumer}, Plan={Plan}",
                        ClassName, MethodName, consumerDto.TenantCode, consumerDto.ConsumerCode, consumerDto.PlanId);

                    return false;
                }


                var mappingsToAdd = mappings?.Where(x => x.Ssbci == Ssbci).ToList();
                var mappingsToRemove = mappings?.Where(x => x.Ssbci != Ssbci).ToList();

                _logger.LogInformation(
                    "{ClassName}.{MethodName}: Found {AddCount} mappings to add and {RemoveCount} to remove. Tenant={Tenant}, Consumer={Consumer}, Plan={Plan}",
                    ClassName, MethodName, mappingsToAdd?.Count, mappingsToRemove?.Count,
                    consumerDto.TenantCode, consumerDto.ConsumerCode, consumerDto.PlanId);

                // --- Step 3: Get Existing Consumer Cohorts ---
                _logger.LogInformation(
                    "{ClassName}.{MethodName}: Fetching current cohorts for Consumer={Consumer}, Tenant={Tenant}",
                    ClassName, MethodName, consumerDto.ConsumerCode, consumerDto.TenantCode);

                var consumerCohorts = _consumerCohortHelper.GetConsumerCohorts(new ConsumerCohortsRequestDto
                {
                    TenantCode = consumerDto.TenantCode,
                    ConsumerCode = consumerDto.ConsumerCode
                }).GetAwaiter().GetResult();

                if (consumerCohorts?.Cohorts == null)
                {
                    _logger.LogError(
                        "{ClassName}.{MethodName}: Failed to fetch cohorts. Tenant={Tenant}, Consumer={Consumer}",
                        ClassName, MethodName, consumerDto.TenantCode, consumerDto.ConsumerCode);
                    consumerCohorts = new CohortsResponseDto
                    {
                        Cohorts = new List<CohortDto>()
                    };
                }

                // --- Step 4: Calculate Diff ---
                var cohortsToAdd = mappingsToAdd?
                    .Where(x => !consumerCohorts.Cohorts.Any(c => c.CohortName == x.CohortName))
                    .ToList();

                var cohortsToRemove = mappingsToRemove?
                    .Where(x => consumerCohorts.Cohorts.Any(c => c.CohortName == x.CohortName))
                    .ToList();
               

                if (defaultMappings != null && defaultMappings.Any())
                {
                    if(cohortsToAdd == null)
                    {
                        cohortsToAdd = new();
                    }
                    cohortsToAdd.AddRange(defaultMappings);
                }
                
                if (!cohortsToAdd.Any() && !cohortsToRemove.Any())
                {
                    _logger.LogInformation(
                        "{ClassName}.{MethodName}: No cohort changes required. Tenant={Tenant}, Consumer={Consumer}, Plan={Plan}",
                        ClassName, MethodName, consumerDto.TenantCode, consumerDto.ConsumerCode, consumerDto.PlanId);
                    return false;
                }

                if (cohortsToAdd?.Count > 0)
                {
                    // --- Step 5: Process Additions ---
                    foreach (var mapping in cohortsToAdd)
                    {
                        _logger.LogInformation(
                            "{ClassName}.{MethodName}: Adding consumer to cohort {CohortName}, Tenant={Tenant}, Consumer={Consumer}, Purse={PurseNumber}",
                            ClassName, MethodName, mapping.CohortName, consumerDto.TenantCode, consumerDto.ConsumerCode, mapping.FisPurseNumber);

                        HandleCohortChange(consumerDto, mapping, MethodName, isAdd: true).GetAwaiter().GetResult();
                    }
                }

                // --- Step 6: Process Removals ---
                if (cohortsToRemove?.Count > 0)
                {
                    foreach (var mapping in cohortsToRemove)
                    {
                        _logger.LogInformation(
                            "{ClassName}.{MethodName}: Removing consumer from cohort {CohortName}, Tenant={Tenant}, Consumer={Consumer}, Purse={PurseNumber}",
                            ClassName, MethodName, mapping.CohortName, consumerDto.TenantCode, consumerDto.ConsumerCode, mapping.FisPurseNumber);

                      HandleCohortChange(consumerDto, mapping, MethodName, isAdd: false , mappingsToAdd).GetAwaiter().GetResult(); ;
                    }
                }

                _logger.LogInformation(
                    "{ClassName}.{MethodName}: Completed cohort assignment successfully. Tenant={Tenant}, Consumer={Consumer}, Plan={Plan}",
                    ClassName, MethodName, consumerDto.TenantCode, consumerDto.ConsumerCode, consumerDto.PlanId);

                return true;
            }
            catch (Exception ex)
           {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName}: Exception occurred during cohort assignment. Tenant={Tenant}, Consumer={Consumer}, Plan={Plan}",
                    ClassName, MethodName, consumerDto.TenantCode, consumerDto.ConsumerCode, consumerDto.PlanId);
                return false;
            }
        }


        private async Task<BaseResponseDto> HandleCohortChange(ConsumerDto consumerDto, PlanCohortPurseMappingDto mapping, string methodName, bool isAdd , List<PlanCohortPurseMappingDto>? flexMapping = null)
        {
            var action = isAdd ? Constant.Add : Constant.Remove;
            var cohortRequest = new CohortConsumerRequestDto
            {
                ConsumerCode = consumerDto.ConsumerCode!,
                CohortName = mapping.CohortName,
                TenantCode = consumerDto.TenantCode!,
                EventSource = "ConsumerPurseCohortAssignment",
                EventId = Guid.NewGuid().ToString("N"),
                ProcessedBy = ClassName
            };

            try
            {
                _logger.LogDebug(
                    "{ClassName}.{MethodName}: Attempting to {Action} Consumer={Consumer} Cohort={Cohort}, Tenant={Tenant}",
                    ClassName, methodName, action, consumerDto.ConsumerCode, mapping.CohortName, consumerDto.TenantCode);

                var cohortResp = isAdd
                    ? _consumerCohortHelper.AddConsumerCohort(cohortRequest).GetAwaiter().GetResult()
                    : _consumerCohortHelper.RemoveConsumerCohort(cohortRequest).GetAwaiter().GetResult();

                if (cohortResp?.ErrorCode != null)
                {
                    _logger.LogError(
                        "{ClassName}.{MethodName}: Failed to {Action} Consumer={Consumer} Cohort={Cohort}. ErrorCode={ErrorCode}, Message={Message}",
                        ClassName, methodName, action, consumerDto.ConsumerCode, mapping.CohortName, cohortResp.ErrorCode, cohortResp.ErrorMessage);
                    return new BaseResponseDto() { ErrorCode = cohortResp?.ErrorCode };
                }

                _logger.LogInformation(
                    "{ClassName}.{MethodName}: Successfully {Action}ed Consumer={Consumer} Cohort={Cohort}",
                    ClassName, methodName, action, consumerDto.ConsumerCode, mapping.CohortName);

                // Purse assignment
                var purseResp = await ConsumerPurseAssignment(
                    consumerDto.TenantCode!,
                    consumerDto.ConsumerCode!,
                    mapping.PurseWalletTypeCode,
                    mapping.FisPurseNumber,
                    action , flexMapping);

                if (purseResp?.ErrorCode != null)
                {
                    _logger.LogError(
                        "{ClassName}.{MethodName}: Purse assignment failed. Action={Action}, Consumer={Consumer}, Purse={PurseNumber}, ErrorCode={ErrorCode}, Message={Message}",
                        ClassName, methodName, action, consumerDto.ConsumerCode, mapping.FisPurseNumber, purseResp.ErrorCode, purseResp.ErrorMessage);
                    return new BaseResponseDto() { ErrorCode = purseResp?.ErrorCode };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName}: Exception during {Action} of Consumer={Consumer}, Cohort={Cohort}",
                    ClassName, methodName, action, consumerDto.ConsumerCode, mapping.CohortName);

                return new BaseResponseDto() { ErrorCode = 500 , ErrorMessage = ex.Message};
            }

            return new BaseResponseDto();
        }

        public GetPlanCohortPurseMappingResponseDto? GetPlanCohortPurseMappingAsync(string planId)
        {
            Dictionary<string, long> parameters = new Dictionary<string, long>();
            return _fisClient.Get<GetPlanCohortPurseMappingResponseDto>($"{Constant.GetPlanCohortPurseMappingAPIUrl}/{planId}", parameters).GetAwaiter().GetResult();

        }

        public async Task<BaseResponseDto> ConsumerPurseAssignment(string tenantCode, string consumerCode, string purseWalletTypeCode, string? purseNumber, string actionType , List<PlanCohortPurseMappingDto>? flexMapping = null)
        {
            return await _consumerPurseAssignmentService.ConsumerPurseAssignment(tenantCode, consumerCode, purseWalletTypeCode, int.Parse(purseNumber!), actionType , flexMapping);
        }

        private  bool GetIsSsbciFlag(string consumerAttributJson)
        {
            if (string.IsNullOrWhiteSpace(consumerAttributJson))
                return false;

            try
            {
                var obj = JObject.Parse(consumerAttributJson);

                // If property not found → default false
                JToken token = obj["is_ssbci"];
                if (token == null)
                    return false;

                return token.Value<bool>();
            }
            catch
            {
                // If invalid JSON → treat as false
                return false;
            }
        }
    }
}
