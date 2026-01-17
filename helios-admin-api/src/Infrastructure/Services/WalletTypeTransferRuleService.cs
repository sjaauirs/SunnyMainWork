using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class WalletTypeTransferRuleService : IWalletTypeTransferRuleService
    {

        private readonly IWalletClient _walletClient;

        public WalletTypeTransferRuleService(IWalletClient walletClient)
        {
            _walletClient = walletClient;
        }

        public async Task<BaseResponseDto> ImportWalletTypeTranferRule(ImportWalletTypeTransferRuleRequestDto importWalletTypeTransferRuleRequest)
        {
            return await _walletClient.Post<BaseResponseDto>(Constant.WalletTypeTransferRuleImportAPIUrl, importWalletTypeTransferRuleRequest);
        }
    }
}
