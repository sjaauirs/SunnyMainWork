using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class CreateCohortRequestMockDto : CreateCohortRequestDto
    {
        public CreateCohortRequestMockDto()
        {
            CohortCode = "coh-09200f026c574153b1ef2e7bae5875a9";
            CohortName = "pneu_unvaccinated65up";
            CohortDescription = "Age 65+ AND (never received any pneumococcal conjugate vaccine OR whose previous vaccination history is unknown)";
            ParentCohortId = 4;
            CohortRule = "success";
            CohortEnabled = true;
            CreateUser = "sunny";

        }
    }
}
