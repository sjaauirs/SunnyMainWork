using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Constants;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services
{
    public class WalletTypeTransferRuleService : IWalletTypeTransferRuleService
    {
        private readonly IWalletTypeTransferRuleRepo _walletTypeTransferRuleRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly ILogger<WalletTypeTransferRuleService> _logger;
        const string className = nameof(WalletTypeTransferRuleService);

        public WalletTypeTransferRuleService(IWalletTypeTransferRuleRepo walletTypeTransferRuleRepo,
            IWalletTypeRepo walletTypeRepo,
            ILogger<WalletTypeTransferRuleService> logger)
        {
            _walletTypeTransferRuleRepo = walletTypeTransferRuleRepo;
            _walletTypeRepo = walletTypeRepo;
            _logger = logger;
        }

        /// <summary>
        /// get walletType transfer rule by tenant code
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ExportWalletTypeTransferRuleResponseDto> ExportWalletTypeTransferRules(ExportWalletTypeTransferRuleRequestDto request)
        {
            const string methodName = nameof(ExportWalletTypeTransferRules);
            try
            {
                var transferRules = await _walletTypeTransferRuleRepo.GetWalletTypeTransferRules(request.TenantCode);
                if (transferRules.Count <= 0)
                {
                    _logger.LogError("{className}.{methodName}: WalletType Transfer Rules not found with the TenantCode: {TenantCode}",
                        className, methodName, request.TenantCode);
                    return new ExportWalletTypeTransferRuleResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"WalletType Transfer Rules not found with the TenantCode: {request.TenantCode}"
                    };
                }
                return new ExportWalletTypeTransferRuleResponseDto
                {
                    WalletTypeTransferRules = transferRules
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export Wallet Transfer Rule. TenantCode: {TenantCode}",
                    request.TenantCode);

                throw;
            }
        }


        /// <summary>
        /// Import walletType transfer rule by tenant code
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> ImportWalletTypeTransferRules(ImportWalletTypeTransferRuleRequestDto importWalletTypeTransferRuleRequest)
        {
            const string methodName = nameof(ImportWalletTypeTransferRules);
            try
            {
                int errorCount = 0;

                foreach (var transferRule in importWalletTypeTransferRuleRequest.WalletTypeTransferRules)
                {
                    transferRule.TenantCode = importWalletTypeTransferRuleRequest.TenantCode;
                    var existingRule = await _walletTypeTransferRuleRepo.FindOneAsync(x => x.TenantCode == transferRule.TenantCode &&
                    x.WalletTypeTransferRuleCode == transferRule.WalletTypeTransferRuleCode && x.DeleteNbr == 0);

                    var sourceWalletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == transferRule.SourceWalletTypeCode
                    && x.DeleteNbr == 0);
                    if (sourceWalletType == null)
                    {
                        _logger.LogError("{className}.{methodName}: Source WalletType not found with the walletTypeCode: {walletTypeCode}",
                            className, methodName, transferRule.SourceWalletTypeCode);
                        errorCount++;
                        continue;
                    }

                    var targetWalletType = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == transferRule.TargetWalletTypeCode
                    && x.DeleteNbr == 0);
                    if (targetWalletType == null)
                    {
                        _logger.LogError("{className}.{methodName}: Target WalletType not found with the walletTypeCode: {walletTypeCode}",
                            className, methodName, transferRule.TargetWalletTypeCode);
                        errorCount++;
                        continue;
                    }
                    if (existingRule == null)
                    {
                        existingRule = await _walletTypeTransferRuleRepo.FindOneAsync(x => x.TenantCode == transferRule.TenantCode &&
                        x.SourceWalletTypeId == sourceWalletType.WalletTypeId && x.DeleteNbr == 0);
                    }

                    if (existingRule != null)
                    {
                        existingRule.SourceWalletTypeId = sourceWalletType.WalletTypeId;
                        existingRule.TargetWalletTypeId = targetWalletType.WalletTypeId;
                        existingRule.TenantCode = transferRule.TenantCode;
                        existingRule.TransferRule = transferRule.TransferRule;
                        existingRule.UpdateTs = DateTime.UtcNow;
                        existingRule.UpdateUser = WalletConstants.ImportUser;
                        await _walletTypeTransferRuleRepo.UpdateAsync(existingRule);
                    }
                    else
                    {
                        var guid = Guid.NewGuid();
                        var newWalletTypeTransferRuleCode = $"wtr-{guid:N}";
                        var newTransferRule = new WalletTypeTransferRuleModel
                        {
                            WalletTypeTransferRuleCode = newWalletTypeTransferRuleCode,
                            TenantCode = transferRule.TenantCode,
                            SourceWalletTypeId = sourceWalletType.WalletTypeId,
                            TargetWalletTypeId = targetWalletType.WalletTypeId,
                            TransferRule = transferRule.TransferRule,
                            CreateTs = DateTime.UtcNow,
                            CreateUser = WalletConstants.ImportUser,
                            DeleteNbr = 0
                        };
                        await _walletTypeTransferRuleRepo.CreateAsync(newTransferRule);
                    }
                }
                return new BaseResponseDto
                {
                    ErrorCode = errorCount > 0 ? StatusCodes.Status206PartialContent : null,
                    ErrorMessage = errorCount > 0 ? $"{errorCount} errors occurred while importing transfer rules." : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import Wallet Transfer Rule.");
                throw;
            }
        }
    }
}
