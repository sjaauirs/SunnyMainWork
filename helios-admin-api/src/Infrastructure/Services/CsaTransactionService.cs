using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Enums;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class CsaTransactionService : ICsaTransactionService
    {
        private readonly IFisClient _fisClient;
        private readonly IWalletClient _walletClient;
        private readonly ILogger<CsaTransactionService> _logger;
        private const string className = nameof(CsaTransactionService);
        public CsaTransactionService(ILogger<CsaTransactionService> logger, IFisClient fisClient, IWalletClient walletClient)
        {
            _fisClient = fisClient;
            _walletClient = walletClient;
            _logger = logger;
        }

        /// <summary>
        /// Handles the disposal of a CSA transaction by updating its status, 
        /// validating tenant account details, and processing wallet transactions.
        /// </summary>
        /// <param name="csaTransactionRequestDto">
        /// The request DTO containing details about the CSA transaction, such as tenant code, consumer code, and transaction code.
        /// </param>
        /// <returns>
        /// A <see cref="BaseResponseDto"/> containing the result of the operation.
        /// Returns a success response if all steps succeed, or an error response if any step fails.
        /// </returns>
        /// <exception cref="Exception">Thrown if an unexpected error occurs during the operation.</exception>
        public async Task<CsaTransactionResponseDto> DisposeCsaTransaction(CsaTransactionRequestDto csaTransactionRequestDto)
        {
            const string methodName = nameof(DisposeCsaTransaction);

            try
            {
                _logger.LogInformation(
                    "{ClassName}.{MethodName} - Started processing dispose CSA transaction with TenantCode:{TenantCode}, ConsumerCode:{ConsumerCode}",
                    className, methodName, csaTransactionRequestDto.TenantCode, csaTransactionRequestDto.ConsumerCode);

                //  Update CSA transaction status
                var csaTransactionResponseDto = await UpdateCsaTransactionStatus(csaTransactionRequestDto);
                if (csaTransactionResponseDto.ErrorCode != null)
                {
                    string errorMessage = GenerateErrorMessage(methodName, "Error occurred while updating CSA transaction status.", csaTransactionRequestDto, csaTransactionResponseDto);
                    return LogAndCreateErrorResponse(errorMessage, csaTransactionResponseDto.ErrorCode);
                }

                // Validate CSA transaction status
                if (!IsStatusApproved(csaTransactionResponseDto))
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Successfully updated csa transaction status.Skipping wallet transactions Because Status is :{Status}", className, methodName, csaTransactionResponseDto.CsaTransactionDto.Status);
                    return csaTransactionResponseDto;
                }

                // Fetch tenant account
                var tenantAccount = await GetTenantAccount(csaTransactionRequestDto);
                if (string.IsNullOrEmpty(tenantAccount?.TenantConfigJson))
                {
                    string errorMessage = $"{className}.{methodName} - Tenant account config JSON is null or empty with TenantCode: {csaTransactionRequestDto.TenantCode}";
                    return LogAndCreateErrorResponse(errorMessage, StatusCodes.Status404NotFound);
                }

                // Process wallet transactions
                var walletResponse = await ProcessCsaWalletTransactions(tenantAccount.TenantConfigJson, csaTransactionResponseDto.CsaTransactionDto);
                if (walletResponse.ErrorCode != null)
                {
                    _logger.LogError("{Classname}.{MethodName} - Error occurred while processing wallet transactions with TenantCode:{TenantCode}, TransactionCode:{Code},response:{Response}",
                       className, methodName, csaTransactionRequestDto.CsaTransactionCode, csaTransactionRequestDto.TenantCode, walletResponse.ToJson());
                    // Handle wallet transaction failure by resetting CSA status
                    csaTransactionRequestDto.Status = CsaTransactionStatus.NEW.ToString();
                    csaTransactionResponseDto = await UpdateCsaTransactionStatus(csaTransactionRequestDto);

                    if (csaTransactionResponseDto.ErrorCode != null)
                    {
                        string errorMessage = GenerateErrorMessage(methodName, "Error occurred while resetting CSA transaction status.", csaTransactionRequestDto, csaTransactionResponseDto);
                        return LogAndCreateErrorResponse(errorMessage, csaTransactionResponseDto.ErrorCode);
                    }
                    return new CsaTransactionResponseDto() { ErrorCode = walletResponse.ErrorCode, ErrorMessage = "Error occurred while processing wallet transactions" };
                }
                _logger.LogInformation("{ClassName}.{MethodName} - Successfully updated CSA transaction with TenantCode:{TenantCode}, ConsumerCode:{ConsumerCode}", className, methodName, csaTransactionResponseDto?.CsaTransactionDto?.TenantCode,
                    csaTransactionResponseDto?.CsaTransactionDto?.ConsumerCode);

                return csaTransactionResponseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - An unexpected exception occurred.", className, methodName);
                throw;
            }
        }

        private async Task<CsaTransactionResponseDto> UpdateCsaTransactionStatus(CsaTransactionRequestDto csaTransactionRequestDto)
        {
            return await _fisClient.Post<CsaTransactionResponseDto>(Constant.FisCsatransaction, csaTransactionRequestDto);
        }

        private async Task<TenantAccountDto> GetTenantAccount(CsaTransactionRequestDto csaTransactionRequestDto)
        {
            return await _fisClient.Post<TenantAccountDto>(
                Constant.GetTenantAccount,
                new TenantAccountCreateRequestDto { TenantCode = csaTransactionRequestDto.TenantCode });
        }

        private async Task<BaseResponseDto> ProcessCsaWalletTransactions(string tenantConfigJson, CsaTransactionDto csaTransactionDto)
        {
            if (csaTransactionDto == null)
            {
                return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound };
            }
            var csaWalletTransactionsRequestDto = new CsaWalletTransactionsRequestDto
            {
                WalletId = csaTransactionDto.WalletId,
                Amount = csaTransactionDto.Amount,
                ConsumerCode = csaTransactionDto.ConsumerCode ?? string.Empty,
                TenantCode = csaTransactionDto.TenantCode ?? string.Empty,
                TenantConfig = tenantConfigJson,
                Description = csaTransactionDto.Description ?? string.Empty,
            };
            return await _walletClient.Post<BaseResponseDto>(Constant.WalletTransactions, csaWalletTransactionsRequestDto);

        }

        private static bool IsStatusApproved(CsaTransactionResponseDto csaTransactionResponseDto)
        {
            return !string.IsNullOrEmpty(csaTransactionResponseDto?.CsaTransactionDto?.Status) &&
                   csaTransactionResponseDto.CsaTransactionDto.Status == CsaTransactionStatus.APPROVED.ToString();
        }

        private CsaTransactionResponseDto LogAndCreateErrorResponse(string errorMessage, int? errorCode)
        {
            _logger.LogError(errorMessage);
            return new CsaTransactionResponseDto
            {
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }

        private static string GenerateErrorMessage(string methodName, string context, CsaTransactionRequestDto request, CsaTransactionResponseDto response)
        {
            return $"{className}.{methodName} - {context} TenantCode:{request.TenantCode}, TransactionCode:{request.CsaTransactionCode}, Response:{response.ToJson()}";
        }

    }
}
