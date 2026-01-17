using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class WalletTypeController : ControllerBase
    {
        private readonly ILogger<WalletTypeController> _walletTypeLogger;
        public readonly IWalletTypeService _walletTypeService;

        public const string className = nameof(WalletTypeController);

        public WalletTypeController(ILogger<WalletTypeController> walletTypeLogger, IWalletTypeService walletTypeService)
        {
            _walletTypeLogger = walletTypeLogger;
            _walletTypeService = walletTypeService;
        }
        /// <summary>
        /// Creates WalletType with the given data
        /// </summary>
        /// <param name="walletTypeDto">WalletType request to create walletType</param>
        /// <returns>BaseResponse with status codes</returns>
        [HttpPost("wallet-type")]
        public async Task<ActionResult<BaseResponseDto>> CreateWalletType([FromBody] WalletTypeDto walletTypeDto)
        {
            const string methodName = nameof(CreateWalletType);
            try
            {
                var walletTypeResponse = await _walletTypeService.CreateWalletType(walletTypeDto);
                if (walletTypeResponse?.ErrorCode != null)
                {
                    _walletTypeLogger.LogError("{ClassName}.{MethodName} - Error occurred while Create WalletType with Request: {walletTypeRequest},ErrorCode:{ErrorCode}",
                       className, methodName, walletTypeDto.ToJson(), walletTypeResponse.ErrorCode);
                    return StatusCode((int)walletTypeResponse.ErrorCode, walletTypeResponse);
                }
                _walletTypeLogger.LogInformation("{ClassName}.{MethodName} - WalletType Created successfully with WalletTypeCode:{WalletTypeCode}",
                   className, methodName, walletTypeDto.WalletTypeCode);

                return Ok(walletTypeResponse);
            }
            catch (Exception ex)
            {
                _walletTypeLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating WalletType with,WalletTypeCode:{WalletTypeCode},ErrorCode:{ErrorCode},ERROR:{Msg}",
                       className, methodName, walletTypeDto.WalletTypeCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });

            }
        }
        /// <summary>
        /// Gets all the wallet types available in database
        /// </summary>
        /// <returns>List of wallet types</returns>
        [HttpGet("wallet-types")]
        public async Task<ActionResult<GetWalletTypeResponseDto>> GetAllWalletTypes()
        {
            const string methodName = nameof(GetAllWalletTypes);
            try
            {
                _walletTypeLogger.LogInformation("{ClassName}.{MethodName}: Started fetching all the wallet types", className, methodName);
                var response = await _walletTypeService.GetAllWalletTypes();
                if (response?.WalletTypes?.Count == 0)
                {
                    _walletTypeLogger.LogWarning("{ClassName}.{MethodName}: No wallet types are available", className, methodName);
                }
                _walletTypeLogger.LogInformation("{ClassName}.{MethodName}: Successfully fetched all the wallet types", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _walletTypeLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while get all walletTypes. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}, StackTrace: {StackTrace}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetWalletTypeResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Retrieves the wallet type information based on the provided WalletTypeDto.
        /// </summary>
        /// <param name="walletTypeDto">The data transfer object containing wallet type details.</param>
        /// <returns>
        /// Returns an HTTP 200 response with the wallet type details if found.  
        /// Returns an HTTP 404 response if the wallet type is not found.  
        /// Returns an HTTP 500 response if an exception occurs.
        /// </returns>
        /// <exception cref="Exception">Logs and handles any unexpected exceptions during execution.</exception>
        [HttpPost("wallet-type/wallet-type-code")]
        public async Task<ActionResult<WalletTypeResponseDto>> GetWalletTypeCode([FromBody] WalletTypeDto walletTypeDto)
        {
            const string methodName = nameof(GetWalletTypeCode);
            try
            {
                _walletTypeLogger.LogInformation("{ClassName}.{MethodName}: API - Started with WalletTypeCode : {WalletTypeCode}", className, methodName, walletTypeDto.WalletTypeCode);

                var walletTypeResponse = await _walletTypeService.GetWalletTypeCode(walletTypeDto);

                if (walletTypeResponse == null || walletTypeResponse?.WalletTypeId <= 0)
                {
                    _walletTypeLogger.LogError("{ClassName}.{MethodName} - Error occurred while fetching WalletType with Request: {WalletTypeRequest},ErrorCode:{ErrorCode}",
                       className, methodName, StatusCodes.Status404NotFound, walletTypeDto.ToJson());
                    return StatusCode(StatusCodes.Status404NotFound, new WalletTypeResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        WalletTypeDto = walletTypeDto
                    });
                }

                _walletTypeLogger.LogInformation("{ClassName}.{MethodName} - WalletType Fetched successfully with WalletTypeCode:{WalletTypeCode}",
                   className, methodName, walletTypeDto.WalletTypeCode);

                return Ok(new WalletTypeResponseDto() { WalletTypeDto = walletTypeResponse });
            }
            catch (Exception ex)
            {
                _walletTypeLogger.LogError(ex, "{ClassName}.{MethodName}: - Error occurred while fetching wallet type,ErrorCode:{Code}, ERROR:{Error}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new WalletTypeResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    WalletTypeDto = walletTypeDto
                });
            }
        }
    }
}
