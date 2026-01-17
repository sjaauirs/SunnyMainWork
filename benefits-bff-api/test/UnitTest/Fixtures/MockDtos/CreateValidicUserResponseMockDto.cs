using Sunny.Benefits.Bff.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class CreateValidicUserResponseMockDto : CreateValidicUserResponseDto
    {
        public CreateValidicUserResponseMockDto()
        {
            Id = "test-id";
            Uid = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5";
            Status = "active";
            Created_at = DateTime.UtcNow.AddMinutes(-10);
            Updated_at = DateTime.UtcNow;

            Marketplace = new Marketplace
            {
                Token = "test-marketplace-token",
                Url = "https://test-url.com"
            };

            Mobile = new Mobile
            {
                Token = "test-mobile-token"
            };
        }
    }
}
