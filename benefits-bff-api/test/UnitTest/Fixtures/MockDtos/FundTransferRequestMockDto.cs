using Sunny.Benefits.Bff.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class FundTransferRequestMockDto : FundTransferRequestDto
    {
        public FundTransferRequestMockDto()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cmr-1ab1f990bdf44ca789bc6963a18004c2";
            SourceWalletType = "wat-2d62dcaf2aa4424b9ff6c2ddb5895077";
            TargetWalletType = "wat-35b3ac62d74b4119a5f630c9b6446035";
            Amount = 10;
            PurseLabel = "OTC";
        }
    }
}
