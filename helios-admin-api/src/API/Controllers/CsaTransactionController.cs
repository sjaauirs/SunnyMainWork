using Microsoft.AspNetCore.Mvc;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class CsaTransactionController : ControllerBase
    {
        private readonly ICsaTransactionService _csaTransactionService;
        private readonly ILogger<CsaTransactionController> _logger;
        private const string className = nameof(CsaTransactionController);
        public CsaTransactionController(ILogger<CsaTransactionController> logger, ICsaTransactionService csaTransactionService)
        {
            _csaTransactionService = csaTransactionService;
            _logger = logger;
        }
        /// <summary>
        /// Disposes a CSA transaction based on the provided request data.
        /// </summary>
        /// <param name="csaTransactionRequestDto">The DTO containing the CSA transaction request details, such as TenantCode and CsaTransactionCode.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the response for the CSA transaction disposal.
        /// - Returns <see cref="OkObjectResult"/> if the operation is successful.
        /// - Returns an error response with the appropriate HTTP status code if an error occurs.
        /// </returns>
        /// <remarks>
        /// This method logs the start, success, and failure of the transaction processing. 
        /// In case of an exception, it returns a 500 Internal Server Error response.
        /// </remarks>
        /// <exception cref="Exception">Logs the exception details and returns a generic error response if an unhandled exception occurs.</exception>

        [HttpPost("dispose-csa-transaction")]
        public async Task<IActionResult> DisposeCsaTransaction(CsaTransactionRequestDto csaTransactionRequestDto)
        {
            const string methodName = nameof(DisposeCsaTransaction);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing csatransaction status with TenantCode:{Code},TransactionCode:{TransactionCode}"
                       , className, methodName, csaTransactionRequestDto.TenantCode, csaTransactionRequestDto.CsaTransactionCode);

                var response = await _csaTransactionService.DisposeCsaTransaction(csaTransactionRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{Classname}.{MethodName} - Error occurred while processing csatransaction status with TenantCode:{TenantCode}, TransactionCode:{Code},response:{Response}",
                        className, methodName, csaTransactionRequestDto.CsaTransactionCode, csaTransactionRequestDto.TenantCode, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName} - Successfully processed csatransaction status with TenantCode:{Code},TransactionCode:{TransactionCode}"
                    , className, methodName, csaTransactionRequestDto.TenantCode, csaTransactionRequestDto.CsaTransactionCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Classname}.{MethodName} - Error occurred while processing csatransaction status with TenantCode:{TenantCode}, TransactionCode:{Code},ERROR:{Err}",
                  className, methodName, csaTransactionRequestDto.CsaTransactionCode, csaTransactionRequestDto.TenantCode, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
