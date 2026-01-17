using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockModel
{
    public class TenantMockModel : TenantModel
    {
        public TenantMockModel()
        {
            TenantId = 1;
            SponsorId = 2;
            TenantCode = "ten-8d9e6f00eec8436a8251d55ff74b1642";
            PlanYear = 2023;
            PeriodStartTs = DateTime.Now;
            PeriodEndTs = DateTime.Now.AddYears(1);
            PartnerCode = "par-6f222db8ad104cfdbaf59d3c334b2586";
            CreateTs = DateTime.Now;
            UpdateTs = DateTime.Now;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
            TenantAttribute = "{}";
            redemption_vendor_name_0 = "PRIZEOUT";
            redemption_vendor_partner_id_0 = "90a22a0b44c8467db202f492cd40fc8b";    
            
}
    }
}
