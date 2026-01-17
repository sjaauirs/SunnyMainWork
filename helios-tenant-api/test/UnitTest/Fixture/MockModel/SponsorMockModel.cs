using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockModel
{
    public class SponsorMockModel : SponsorModel
    {
        public SponsorMockModel()
        {
            SponsorId = 1;
            SponsorCode = "spo-c008f49aa31f4acd9aa6e2114bfb820e";
            SponsorName = "United-DEP1";
            SponsorDescription = "United-DEPARTMENT ONE";
            CustomerId = 1;
            CreateTs = DateTime.Now;
            UpdateTs = DateTime.Now;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
        }
    }
}