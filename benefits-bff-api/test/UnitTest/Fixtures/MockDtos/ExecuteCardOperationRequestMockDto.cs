using Sunny.Benefits.Bff.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class ExecuteCardOperationRequestMockDto : ExecuteCardOperationRequestDto
    {
        public ExecuteCardOperationRequestMockDto()
        {
            TenantCode = Guid.NewGuid().ToString();
            ConsumerCode = Guid.NewGuid().ToString();
            CardOperation = "Freeze";
        }
    }
}
