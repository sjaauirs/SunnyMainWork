using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class DataQueryController : ControllerBase
    {
        private readonly ILogger<DataQueryController> _dataQueryLogger;
        private readonly IDataQueryService _dataQueryService;
        const string className = nameof(DataQueryController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskLogger"></param>
        /// <param name="consumerTaskService"></param>
        /// <param name="subTaskService"></param>
        public DataQueryController(ILogger<DataQueryController> logger, IDataQueryService dataQueryService)
        {
            _dataQueryLogger = logger;
            _dataQueryService = dataQueryService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerTasksByIdRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-consumer-task-by-data-query")]
        public async Task<ActionResult<DataQueryResponseDto>> GetConsumerTask(DataQueryRequestDto consumerTasksRequestDto)
        {
            const string methodName = nameof(GetConsumerTask);
            try
            {
                _dataQueryLogger.LogInformation("{className}.{methodName}: API - Enter with request dto {request}", className, methodName, consumerTasksRequestDto.ToJson());

                var response = await _dataQueryService.GetConsumerTask(consumerTasksRequestDto);
                if (response.ErrorCode != null)
                {
                    _dataQueryLogger.LogWarning("{ClassName}.{MethodName}: Error occured with TenantCode:{TenantCode},Response:{Response}",
                    className, methodName, consumerTasksRequestDto.TenantCode, response);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _dataQueryLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new DataQueryResponseDto { ErrorCode=StatusCodes.Status500InternalServerError,ErrorMessage="Query execution failed"};
            }
        }
    }
}
