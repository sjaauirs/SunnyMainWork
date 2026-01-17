using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class ExportWalletTypeTransferRuleMockDto : ExportWalletTypeTransferRuleDto
    {
        public ExportWalletTypeTransferRuleMockDto()
        {
            SourceWalletTypeCode = "wty-c008f49aa31f4acd9aa6e2114bfb820e";
            TargetWalletTypeCode = "wty-ecada21e57154928a2bb959e8365b8b4";
            WalletTypeTransferRuleId = 1;
            WalletTypeTransferRuleCode = "wtytr-c008f49aa31f4acd9aa6e2114bfb820e";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            SourceWalletTypeId = 1;
            TargetWalletTypeId = 2;
            TransferRule = "TransferRule";
            DeleteNbr = 0;
            CreateTs = DateTime.Now;            
        }

    }
}
