using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;
        const string _className = nameof(TransactionController);

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="transactionService"></param>
        public TransactionController(ILogger<TransactionController> logger, ITransactionService transactionService)
        {
            _logger = logger;
            _transactionService = transactionService;
        }

        /// <summary>
        /// Exports tenant data based on the provided request.
        /// </summary>
        /// <param name="request">The export tenant request DTO containing the necessary parameters.</param>
        /// <returns>An IActionResult containing the export file or an error response.</returns>
        [HttpPost("rewards-wallets-transactions")]
        public async Task<IActionResult> GetRewardsWalletTransactions(GetWalletTransactionRequestDto walletTransactionRequest)
        {
            var methodName = nameof(GetRewardsWalletTransactions);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with consumerCode: {consumerCode}", _className, methodName, walletTransactionRequest.ConsumerCode);

                var response = await _transactionService.GetWalletTransactions(walletTransactionRequest);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while fetching rewards wallet transactions, consumerCode: {consumerCode}, ErrorCode: {ErrorCode}", 
                        _className, methodName, walletTransactionRequest.ConsumerCode, response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while fetching rewards wallet transactions. ErrorMessage: {ErrorMessage}, request: {request}", 
                    _className, methodName, StatusCodes.Status500InternalServerError, walletTransactionRequest.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, new ExportTenantResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
