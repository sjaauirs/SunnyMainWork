using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Wallet.Api.Controllers
{
    [Route("api/v1/transaction/")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _transactionLogger;
        private readonly ITransactionService _transactionService;
        private readonly ICsaWalletTransactionsService _csaWalletTransactionsService;

        const string className = nameof(TransactionController);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionLogger"></param>
        /// <param name="transactionService"></param>
        public TransactionController(ILogger<TransactionController> transactionLogger, ITransactionService transactionService, ICsaWalletTransactionsService csaWalletTransactionsService)
        {
            _transactionLogger = transactionLogger;
            _transactionService = transactionService;
            _csaWalletTransactionsService = csaWalletTransactionsService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recentTransactionDto"></param>
        /// <returns></returns>
        [HttpPost("recent")]
        public async Task<ActionResult<GetRecentTransactionResponseDto>> GetRecentTransactions([FromBody] GetRecentTransactionRequestDto recentTransactionDto)
        {
            const string methodName = nameof(GetRecentTransactions);
            try
            {
                _transactionLogger.LogInformation("{className}.{methodName}:  API - Started with WalletId : {WalletId}", className, methodName, recentTransactionDto.WalletId);
                var response = await _transactionService.GetTransactionDetails(recentTransactionDto);

                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {

                _transactionLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new GetRecentTransactionResponseDto();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postGetTransactionsRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-transactions")]
        public async Task<ActionResult<PostGetTransactionsResponseDto>> GetTransaction(PostGetTransactionsRequestDto postGetTransactionsRequestDto)
        {
            const string methodName = nameof(GetTransaction);
            try
            {
                _transactionLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {ConsumerCode}", className, methodName, postGetTransactionsRequestDto.ConsumerCode);
                var response = await _transactionService.GetTransaction(postGetTransactionsRequestDto);

                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    403 => StatusCode(StatusCodes.Status403Forbidden, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(ex, "{className}.{methodName}: API ERROR - msg : {msg}", className, methodName, ex.Message);
                throw;
            }
        }

        [HttpPost("revert-all-transactions")]
        public async Task<ActionResult<BaseResponseDto>> RevertAllTransaction(RevertTransactionsRequestDto revertTransactionsRequestDto)
        {
            const string methodName = nameof(RevertAllTransaction);
            try
            {
                _transactionLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {ConsumerCode}", className, methodName, revertTransactionsRequestDto.ConsumerCode);
                var response = await _transactionService.RevertAllTransaction(revertTransactionsRequestDto);

                return response.ErrorCode switch
                {
                    400 => BadRequest(response),
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "Internal Server Error"
                });
            }
        }
        /// <summary>
        /// Processes CSA wallet transactions for a given request.
        /// </summary>
        /// <param name="csaWalletTransactionsRequestDto">The request data transfer object containing the details of the CSA wallet transaction.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the outcome of the operation. 
        /// Returns:
        /// - <see cref="StatusCodeResult"/> with the appropriate status code and error message if an error occurs.
        /// - <see cref="OkObjectResult"/> if the transaction is successfully processed.
        /// </returns>
        /// <exception cref="Exception">
        /// Logs the exception and returns a 500 Internal Server Error status code if an unhandled error occurs during the processing.
        /// </exception>
        [HttpPost("csa-value-load-transactions")]
        public async Task<IActionResult> ProcessCsaWalletTransactions(CsaWalletTransactionsRequestDto csaWalletTransactionsRequestDto)
        {
            const string methodName = nameof(ProcessCsaWalletTransactions);
            try
            {
                var response = await _csaWalletTransactionsService.HandleCsaWalletTransactions(csaWalletTransactionsRequestDto);
                if (response?.ErrorCode != null)
                {
                    _transactionLogger.LogError("{ClassName}.{MethodName} - Error occurred while processing csa wallet transactions with Request: {TenantCode},ConsumerCode:{Code},ErrorCode:{ErrorCode}",
                       className, methodName, csaWalletTransactionsRequestDto.ToJson(), csaWalletTransactionsRequestDto.ConsumerCode, response.ErrorCode);

                    return StatusCode((int)response.ErrorCode, response);
                }
                _transactionLogger.LogInformation("{ClassName}.{MethodName} - Successfully created csa wallet transactions with TenantCode:{TenantCode},ConsumerCode:{Code}", className, methodName,
                   csaWalletTransactionsRequestDto.TenantCode, csaWalletTransactionsRequestDto.ConsumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing csa wallet transactions with Request: {TenantCode},ConsumerCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                       className, methodName, csaWalletTransactionsRequestDto.ToJson(), csaWalletTransactionsRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });

            }
        }
        /// <summary>
        /// Fetches reward wallets and corresponding transactions for a given consumer, limited by the specified count.
        /// </summary>
        /// <param name="walletTransactionRequestDto">The request DTO containing the consumer code and number of transactions to retrieve.</param>
        /// <returns>A response DTO containing a list of wallets and their associated transactions for the specified consumer.</returns>
        [HttpPost("rewards-wallets-transactions")]
        public async Task<IActionResult> GetRewardWalletTransactions([FromBody] GetWalletTransactionRequestDto walletTransactionRequestDto)
        {
            const string methodName = nameof(ProcessCsaWalletTransactions);
            try
            {
                var response = await _transactionService.GetRewardWalletTransactions(walletTransactionRequestDto);

                _transactionLogger.LogInformation("{ClassName}.{MethodName} - Successfully processed wallet transactions with ConsumerCode:{Code}", className, methodName,
                   walletTransactionRequestDto.ConsumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing wallet transactions with Request: {TenantCode},ConsumerCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                       className, methodName, walletTransactionRequestDto.ToJson(), walletTransactionRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetWalletTransactionResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }


        [HttpPost("create-transactions")]
        public async Task<IActionResult> CreateWalletTransactions([FromBody] CreateTransactionsRequestDto createTransactionsRequestDto)
        {
            const string methodName = nameof(CreateWalletTransactions);
            try
            {
                var response = await _transactionService.CreateWalletTransactions(createTransactionsRequestDto);

                _transactionLogger.LogInformation("{ClassName}.{MethodName} - Successfully processed wallet transactions with ConsumerCode:{Code}", className, methodName,
                   createTransactionsRequestDto.ConsumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing wallet transactions with Request: {TenantCode},ConsumerCode:{Code},ErrorCode:{ErrorCode},ERROR:{Msg}",
                       className, methodName, createTransactionsRequestDto.ToJson(), createTransactionsRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetWalletTransactionResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpPost("remove-transactions")]
        public async Task<IActionResult> RemoveWalletTransactions([FromBody] RemoveTransactionsRequestDto removeTransactionsRequestDto)
        {
            const string methodName = nameof(CreateWalletTransactions);
            try
            {
                var response = await _transactionService.RemoveWalletTransactions(removeTransactionsRequestDto);

                _transactionLogger.LogInformation("{ClassName}.{MethodName} - Successfully removes wallet transactions with transactionDetail:{Code}", className, methodName,
                   removeTransactionsRequestDto.TransactionDetailId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _transactionLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing wallet transactions with Request: {Req},ErrorCode:{ErrorCode},ERROR:{Msg}",
                       className, methodName, removeTransactionsRequestDto.ToJson(), StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}


