using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class GetTenantByPartnerCodeResponseMockDto : GetTenantByPartnerCodeResponseDto
    {
        public GetTenantByPartnerCodeResponseMockDto()
        {
            Tenant = new TenantDto()
            {
                TenantId = 1,
                SponsorId = 1,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                PlanYear = 2023,
                PeriodStartTs = DateTime.UtcNow,
                PartnerCode = "par-7e92b06aa4fe405198d27d2427bf3de4",
                RecommendedTask = false,
                TenantAttribute = "success",
                CreateTs = DateTime.Now,
                UpdateTs = DateTime.Now,
                CreateUser = "sunny",
                UpdateUser = "sunny rewards",
                DeleteNbr = 0,

            };
        }
    }
    
}
