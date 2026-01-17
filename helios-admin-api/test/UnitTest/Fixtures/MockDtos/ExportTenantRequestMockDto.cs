using SunnyRewards.Helios.Admin.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class ExportTenantRequestMockDto : ExportTenantRequestDto
    {
        public ExportTenantRequestMockDto()
        {
            TenantCode = Guid.NewGuid().ToString();
            ExportOptions = ["COHORT", "TASK", "TRIVIA", "CMS", "FIS", "SWEEPSTAKES"];
        }
    }
}
