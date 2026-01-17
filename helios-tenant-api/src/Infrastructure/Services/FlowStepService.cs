using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Services
{
    public class FlowStepService : IFlowStepService
    {
        public readonly ILogger<FlowStepService> _logger;
        public readonly IFlowStepRepo _flowStepRepo;
        public readonly IMapper _mapper;
        public const string _className = nameof(FlowStepService);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="flowStepRepo"></param>
        /// <param name="mapper"></param>
        public FlowStepService(ILogger<FlowStepService> logger, IFlowStepRepo flowStepRepo, IMapper mapper)
        {
            _logger = logger;
            _flowStepRepo = flowStepRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Get Flow Steps
        /// </summary>
        /// <param name="flowRequestDto"></param>
        /// <returns></returns>
        public async Task<FlowResponseDto> GetFlowSteps(FlowRequestDto flowRequestDto)
        {
            const string methodName = nameof(GetFlowSteps);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Service - Started With request : {request}",
                    _className, methodName, flowRequestDto.ToJson());

                flowRequestDto.CohortCodes ??= [];
                flowRequestDto.FlowId ??= 0;

                if (string.IsNullOrEmpty(flowRequestDto.FlowName) && flowRequestDto.FlowId == 0)
                {
                    _logger.LogError("{className}.{methodName}: - ERROR :{msg}", _className, methodName, "Either FlowName or FlowId must be provided");
                    return new FlowResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Either FlowName or FlowId must be provided"
                    };
                }

                var response = _flowStepRepo.GetFlowSteps(flowRequestDto);
                if (response == null)
                {
                    _logger.LogError("{className}.{methodName}: - ERROR :{msg}", _className, methodName, "No flow found");
                    return new FlowResponseDto
                    {
                        ErrorMessage = "No flow found"
                    };
                }
                _logger.LogInformation("{className}.{methodName}: Service - Completed Successfully With tenant code : {tenantCode}, flowId: {flowId}",
                    _className, methodName, flowRequestDto.TenantCode, flowRequestDto.FlowId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: - ERROR :{msg}", _className, methodName, ex.Message);
                throw;
            }
        }
    }
}
