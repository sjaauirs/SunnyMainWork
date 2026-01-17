using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;


namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _adminLogger;
        private readonly IConsumerTaskService _consumerTaskService;
        private readonly IWalletService _walletService;
        private readonly IConsumerAccountService _consumerAccountService;
        private readonly ISweepstakesInstanceService _sweepstakesInstanceService;
        const string className = nameof(AdminController);
        public AdminController(ILogger<AdminController> adminLogger,
            IConsumerTaskService consumerTaskService,
            IWalletService walletService,
            IConsumerAccountService consumerAccountService,
            ISweepstakesInstanceService sweepstakesInstanceService)
        {
            _adminLogger = adminLogger;
            _consumerTaskService = consumerTaskService;
            _walletService = walletService;
            _consumerAccountService = consumerAccountService;
            _sweepstakesInstanceService = sweepstakesInstanceService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskUpdateRequestDto"></param> 
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("consumer/task-update")]
        public async Task<ActionResult<ConsumerTaskUpdateResponseDto>> UpdateConsumerTask([FromForm] TaskUpdateRequestDto taskUpdateRequestDto)
        {
            const string methodName = nameof(UpdateConsumerTask);
            try
            {
                _adminLogger.LogInformation("{className}.{methodName}: Task Update API started with request: {request}", className, methodName,taskUpdateRequestDto.ToJson());
                var updateConsumerResponse = await _consumerTaskService.UpdateConsumerTask(taskUpdateRequestDto);
                if (updateConsumerResponse.ErrorCode != null)
                {
                    _adminLogger.LogError("{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}, Request Data: {request}, Response Data:{response}", className, methodName, updateConsumerResponse.ErrorMessage, updateConsumerResponse.ErrorCode, taskUpdateRequestDto.ToJson(), updateConsumerResponse.ToJson());
                    return StatusCode(Convert.ToInt32(updateConsumerResponse.ErrorCode), updateConsumerResponse);
                }

                return updateConsumerResponse.ConsumerTask != null ? Ok(updateConsumerResponse) : NotFound(updateConsumerResponse);
            }
            catch (Exception ex)
            {
                _adminLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerTaskUpdateResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subtaskUpdateRequestDto"></param>
        /// <returns></returns>
        [HttpPost("consumer/complete-subtask")]
        public async Task<ActionResult<UpdateSubtaskResponseDto>> UpdateCompleteSubtask([FromBody] SubtaskUpdateRequestDto subtaskUpdateRequestDto)
        {
            const string methodName = nameof(UpdateCompleteSubtask);
            try
            {
                var completeSubtaskResponse = await _consumerTaskService.UpdateCompleteSubtask(subtaskUpdateRequestDto);
                _adminLogger.LogInformation("{className}.{methodName}: API started with Task Id:{taskId}", className, methodName, subtaskUpdateRequestDto.TaskId);
                if (completeSubtaskResponse.ErrorCode != null)
                {
                    _adminLogger.LogError("{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, completeSubtaskResponse.ErrorMessage, completeSubtaskResponse.ErrorCode);
                    return StatusCode(Convert.ToInt32(completeSubtaskResponse.ErrorCode), completeSubtaskResponse);
                }
                return Ok(completeSubtaskResponse);
            }
            catch (Exception ex)
            {
                _adminLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new UpdateSubtaskResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }

        }

        [HttpPost("wallet/clear-entries-wallet")]
        public async Task<ActionResult<BaseResponseDto>> ClearEntriesWallet([FromBody] ClearEntriesWalletRequestDto clearEntriesWalletRequestDto)
        {
            const string methodName = nameof(ClearEntriesWallet);
            try
            {
                _adminLogger.LogInformation("{className}.{methodName}: API - Started with Tenant code: {tenantCode}", className, methodName,
                     clearEntriesWalletRequestDto.TenantCode);
                var response = await _walletService.ClearEntriesWallet(clearEntriesWalletRequestDto);
                if (response?.ErrorCode != null)
                {
                    _adminLogger.LogError("{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, response.ErrorMessage, response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _adminLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.InnerException?.Message
                });
            }

        }

        [HttpPost("consumer/revert-all-transactions-and-tasks")]
        public async Task<ActionResult<BaseResponseDto>> RevertAllTransactionAndTasks(RevertTransactionsRequestDto revertTransactionsRequestDto)
        {
            const string methodName = nameof(RevertAllTransactionAndTasks);
            try
            {
                _adminLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {ConsumerCode}", className, methodName, revertTransactionsRequestDto.ConsumerCode);
                var response = await _walletService.RevertAllTransactionsAndTasksForConsumer(revertTransactionsRequestDto);
                if (response?.ErrorCode != null)
                {
                    _adminLogger.LogError("{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, response.ErrorMessage, response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _adminLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("redeem")]
        public async Task<ActionResult<PostRedeemCompleteResponseDto>> Redeem([FromBody] Wallet.Core.Domain.Dtos.PostRedeemStartRequestDto postRedeemStartRequestDto)
        {
            const string methodName = nameof(Redeem);
            try
            {
                _adminLogger.LogInformation("{className}.{methodName}: API - Started with Tenant code: {tenantCode}", className, methodName,
                     postRedeemStartRequestDto.TenantCode);
                var response = await _walletService.RedeemConsumerBalance(postRedeemStartRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _adminLogger.LogError("{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, response.ErrorMessage, response.ErrorCode);
                    return StatusCode((int)errorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _adminLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new PostRedeemCompleteResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                });
            }

        }

        [HttpPost("create-consumer-account")]
        public async Task<ActionResult<ConsumerAccountDto>> CreateConsumerAccount([FromBody] CreateConsumerAccountRequestDto requestDto)
        {
            const string methodName = nameof(CreateConsumerAccount);
            try
            {
                _adminLogger.LogInformation("{className}.{methodName}: API - Started with TenantCode: {tenantCode}, ConsumerCode: {consumerCode}", className, methodName,
                     requestDto.TenantCode, requestDto.ConsumerCode);
                var response = await _consumerAccountService.CreateConsumerAccount(requestDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _adminLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new PostRedeemCompleteResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                });
            }

        }

        [HttpPost("get-consumer-account")]
        public async Task<ActionResult<GetConsumerAccountResponseDto>> GetConsumerAccount([FromBody] GetConsumerAccountRequestDto requestDto)
        {
            const string methodName = nameof(GetConsumerAccount);
            try
            {
                _adminLogger.LogInformation("{className}.{methodName}: API: Received request to get consumer account. TenantCod: {TenantCod}, ConsumerCode: {ConsumerCode}", className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);
                var response = await _consumerAccountService.GetConsumerAccount(requestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _adminLogger.LogError("{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, response.ErrorMessage, response.ErrorCode);
                    return StatusCode((int)errorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _adminLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new PostRedeemCompleteResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                });
            }

        }
        [HttpPost("create-sweepstakes-instance")]
        public async Task<ActionResult<SweepstakesInstanceResponseDto>> CreateSweepstakesInstance([FromBody] SweepstakesInstanceRequestDto requestDto)
        {
            const string methodName = nameof(CreateSweepstakesInstance);
            try
            {
                _adminLogger.LogInformation("{className}.{methodName}: API - Started with SweepstakesId: {SweepstakesId}", className, methodName,
                     requestDto.SweepstakesId);
                var response = await _sweepstakesInstanceService.CreateSweepstakesInstance(requestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _adminLogger.LogError("{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, response.ErrorMessage, response.ErrorCode);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _adminLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new SweepstakesInstanceResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                });
            }

        }

        /// <summary>
        /// Get sweepstakes instance by sweepstakes instance code
        /// </summary>
        /// <param name="sweepstakesInstanceCode"></param>
        /// <returns></returns>
        [HttpGet("sweepstakes-instance")]
        public async Task<ActionResult<SweepstakesInstanceResponseDto>> GetSweepstakesInstance(string sweepstakesInstanceCode)
        {
            try
            {
                _adminLogger.LogInformation("GetSweepstakesInstance API - Started with Sweepstakes instance code: {SweepstakesInstanceCode}",
                     sweepstakesInstanceCode);
                var response = await _sweepstakesInstanceService.GetSweepstakesInstance(sweepstakesInstanceCode);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _adminLogger.LogError("GetSweepstakesInstance API - ERROR: {ErrorCode}", response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _adminLogger.LogError(ex, "GetSweepstakesInstance API - ERROR :{Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new SweepstakesInstanceResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                });
            }

        }
        [HttpPost("consumer-task")]
        public async Task<ActionResult<ConsumerTaskResponseUpdateDto>> PostConsumerTasks([FromBody] CreateConsumerTaskDto requestDto)
        {
            const string methodName = nameof(UpdateConsumerTask);
            try
            {
                _adminLogger.LogInformation("{className}.{methodName}: API started with Task Id:{taskId}", className, methodName, requestDto.TaskId);
                var updateConsumerResponse = await _consumerTaskService.PostConsumerTasks(requestDto);
                if (updateConsumerResponse.ErrorCode != null)
                {
                    _adminLogger.LogError("{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, updateConsumerResponse.ErrorMessage, updateConsumerResponse.ErrorCode);
                    return StatusCode(Convert.ToInt32(updateConsumerResponse.ErrorCode), updateConsumerResponse);
                }

                return updateConsumerResponse.ConsumerTask != null ? Ok(updateConsumerResponse) : NotFound(updateConsumerResponse);
            }
            catch (Exception ex)
            {
                _adminLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerTaskResponseUpdateDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }
    }
}