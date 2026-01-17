using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public class GetTenantByTenantCodeResponseMockDto : GetTenantByTenantCodeResponseDto
    {
        public GetTenantByTenantCodeResponseMockDto()
        {
            TenantId = 1;
            SponsorId = 2;
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            PlanYear = 2023;
            PeriodStartTs = DateTime.Now;
            PartnerCode = "par-45i741-df2e-4f5-ab26-670d444f1c";
            RecommendedTask = true;
            TenantAttribute = "success";
            redemption_vendor_name_0 = "";
            redemption_vendor_partner_id_0 = "";
            ApiKey = "154928a2bb959e8365b8b4";
            SelfReport = true;
            DirectLogin = true;
            EnableServerLogin = true;
            TenantName = "sunny";
            CreateTs = DateTime.Now;
            UpdateTs = DateTime.Now;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 1;
        }
    }
}
