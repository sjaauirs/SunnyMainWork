using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    [Authorize]
    public class FundTransferController : ControllerBase
    {
        private readonly IFundTransferService _fundTransferService;
        private const string className = nameof(FundTransferController);
        private readonly ILogger<FundTransferController> _logger;
        public FundTransferController(ILogger<FundTransferController> logger, IFundTransferService fundTransferService)
        {
            _logger = logger;
            _fundTransferService = fundTransferService;
        }

        [HttpPost("fund-transfer")]
        public async Task<ActionResult> FundTransfer([FromBody] FundTransferRequestDto request)
        {
            const string methodName = nameof(FundTransfer);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Initiating FundTransfer for ConsumerCode: {ConsumerCode}, SourceWallet: {SourceWallet}, TargetWallet: {TargetWallet}, Amount: {amount}",
                    className, methodName, request.ConsumerCode, request.SourceWalletType, request.TargetWalletType, request.Amount);

                var response = await _fundTransferService.TransferFundsAsync(request);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: FundTransfer failed for ConsumerCode: {ConsumerCode}. ErrorCode: {ErrorCode}. Request Data: {RequestData}, Response Data: {ResponseData}",
                    className, methodName, request.ConsumerCode, response.ErrorCode, request.ToJson(), response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: FundTransfer completed successfully for ConsumerCode: {ConsumerCode}. Response Data: {ResponseData}",
                    className, methodName, request.ConsumerCode, response.ToJson());

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception during FundTransfer for ConsumerCode: {ConsumerCode}. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    className, methodName, request.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExportTenantAccountResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
