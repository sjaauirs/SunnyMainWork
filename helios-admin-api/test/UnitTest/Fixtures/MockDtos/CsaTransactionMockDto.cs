using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class CsaTransactionMockDto : CsaTransactionDto
    {
        public CsaTransactionMockDto()
        {
            CsaTransactionId = 1;
            CsaTransactionCode = "cst-5eb1c645deea42f0845dfe01f1e957de";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cmr-0aeb18b134ad4e1c9a07c01b14dd0d8b";
            Amount = 78.50;
            WalletId = 11382;
            TransactionRefId = "B48AFC58 - 5A85 - 498F - 9141 - 8018A44906F9";
            Description = "ACH Manual Value Load - Credit Cardholder";
            Status = "APPROVED";
            MonetaryTransactionId = 694;
        }
    }
}
