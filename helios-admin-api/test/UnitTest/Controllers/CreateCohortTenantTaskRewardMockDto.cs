using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class CreateCohortTenantTaskRewardMockDto : CreateCohortTenantTaskRewardDto
    {
        public CreateCohortTenantTaskRewardMockDto()
        {
            CohortCode = "coh-09200f026c574153b1ef2e7bae5875a9";
            CohortTenantTaskReward = new PostCohortTenantTaskRewardDto()
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                TaskRewardCode = "trw-a8befaf4fb2640d69f53d91eaa868648",
                Recommended = true,
                Priority = -10,
                CreateUser = "sunny",
  
            };
        }
    }
}
