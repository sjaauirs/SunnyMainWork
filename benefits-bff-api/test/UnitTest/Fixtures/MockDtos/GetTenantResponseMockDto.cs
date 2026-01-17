using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class GetTenantResponseMockDto : GetTenantResponseDto
    {
        public GetTenantResponseMockDto()
        {
            var tenant = new TenantDto
            {
                TenantId = 1,
                SponsorId = 1,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                PlanYear = 2023,
                PeriodStartTs = DateTime.UtcNow,
                PartnerCode = "par-7e92b06aa4fe405198d27d2427bf3de4",
                RecommendedTask = false,
                TenantAttribute = "success",
                SelfReport = true,
                EnableServerLogin = false,
                DirectLogin = false,
                TenantName = "SunnyTenant",
                ApiKey = "19afb4b6-dvtu-5869-ndvr-025d2a617b78",
                CreateTs = DateTime.Now,
                UpdateTs = DateTime.Now,
                CreateUser = "sunny",
                UpdateUser = "sunny rewards",
                DeleteNbr = 0

            };
        }
    }
}
