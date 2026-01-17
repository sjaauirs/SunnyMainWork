using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class FlowStepService : IFlowStepService
    {
        private readonly ILogger<FlowStepService> _logger;
        private readonly ITenantClient _tenantClient;
        private readonly ICohortConsumerService _cohortConsumerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICommonHelper _commonHelper;
        private readonly IFlowStepProcessor _flowStepProcessor;
        private const string _className = nameof(FlowStepService);

        public FlowStepService(ILogger<FlowStepService> logger, ITenantClient tenantClient, 
            ICohortConsumerService cohortConsumerService,IHttpContextAccessor httpContextAccessor,ICommonHelper commonHelper,
            IFlowStepProcessor flowStepProcessor)
        {
            _logger = logger;
            _tenantClient = tenantClient;
            _cohortConsumerService = cohortConsumerService;
            _httpContextAccessor = httpContextAccessor;
            _commonHelper = commonHelper;
            _flowStepProcessor = flowStepProcessor;
        }

        public async Task<FlowResponseDto> GetFlowSteps(FlowRequestDto flowRequestDto)
        {
            const string methodName = nameof(GetFlowSteps);
            _logger.LogInformation("{ClassName}.{MethodName}: Fetching consumer cohorts for TenantCode: {TenantCode}",
                 _className, methodName, flowRequestDto.TenantCode);

            try
            {
                if (string.IsNullOrEmpty(flowRequestDto.ConsumerCode))
                {
                    flowRequestDto.ConsumerCode = _commonHelper.GetUserConsumerCodeFromHttpContext();
                }

                var consumerCohorts = await _cohortConsumerService.GetConsumerAllCohorts(flowRequestDto.TenantCode,flowRequestDto.ConsumerCode);

                if (consumerCohorts == null || consumerCohorts.Cohorts.Count == 0)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: No cohorts mapped for TenantCode: {TenantCode}",
                        _className, methodName, flowRequestDto.TenantCode);

                    flowRequestDto.CohortCodes = new List<string>();
                }
                else
                {
                    flowRequestDto.CohortCodes = consumerCohorts.Cohorts
                        .Where(x => !string.IsNullOrEmpty(x.CohortCode))
                        .Select(x => x.CohortCode!)
                        .ToList();

                    _logger.LogInformation("{ClassName}.{MethodName}: Retrieved {CohortCount} cohorts for ConsumerCode: {ConsumerCode}",
                        _className, methodName, flowRequestDto.CohortCodes.Count, flowRequestDto.ConsumerCode);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Calling API: {ApiUrl} with ConsumerCode: {ConsumerCode}",
                    _className, methodName, CommonConstants.GetFlowStepsAPIUrl, flowRequestDto.ConsumerCode);

                var response = await _tenantClient.Post<FlowResponseDto>(CommonConstants.GetFlowStepsAPIUrl, flowRequestDto);

                response.Steps = await _flowStepProcessor.ProcessSteps(response?.Steps, flowRequestDto.ConsumerCode);

                _logger.LogInformation("{ClassName}.{MethodName}: API call completed. Response ErrorCode: {ErrorCode}",
                    _className, methodName, response?.ErrorCode);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
