using Amazon.CloudWatch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class FlowStepController : ControllerBase
    {
        private readonly ILogger<FlowStepController> _logger;
        public readonly IFlowStepService _flowStepService;
        private const string ErrorLogTemplate = "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}";
        const string _className = nameof(FlowStepController);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerLogger"></param>
        /// <param name="customerService"></param>
        public FlowStepController(ILogger<FlowStepController> logger, IFlowStepService flowStepService)
        {
            _logger = logger;
            _flowStepService = flowStepService;
        }

        /// <summary>
        /// Get Flow Steps
        /// </summary>
        /// <param name="flowRequestDto"></param>
        /// <returns></returns>
        [HttpPost("flows/steps")]
        public async Task<ActionResult<FlowResponseDto>> GetFlowSteps([FromBody] FlowRequestDto flowRequestDto)
        {
            const string methodName = nameof(GetFlowSteps);
            try
            {
                _logger.LogInformation("{className}.{methodName}: API - Started With request : {request}",
                    _className, methodName, flowRequestDto.ToJson());
                var response = await _flowStepService.GetFlowSteps(flowRequestDto);
                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    400 => BadRequest(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorLogTemplate, _className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError,new FlowResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                });
            }
        }
    }
}

