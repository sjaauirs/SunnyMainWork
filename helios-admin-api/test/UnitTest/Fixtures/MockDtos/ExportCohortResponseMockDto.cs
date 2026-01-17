using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class ExportCohortResponseMockDto : ExportCohortResponseDto
    {
        public ExportCohortResponseMockDto()
        {
            Cohort = new List<CohortDto>
            {
                new CohortDto
                {
                    CohortId = 1,
                    CohortCode = "C123",
                    CohortName = "Test Cohort 1",
                    CohortDescription = "This is a test cohort description 1.",
                    ParentCohortId = 0,
                    CohortRule = "Rule1",
                    CohortEnabled = true
                },
                new CohortDto
                {
                    CohortId = 2,
                    CohortCode = "C124",
                    CohortName = "Test Cohort 2",
                    CohortDescription = "This is a test cohort description 2.",
                    ParentCohortId = 1,
                    CohortRule = "Rule2",
                    CohortEnabled = false
                },
                new CohortDto
                {
                    CohortId = 3,
                    CohortCode = "C125",
                    CohortName = "Test Cohort 3",
                    CohortDescription = "This is a test cohort description 3.",
                    ParentCohortId = 1,
                    CohortRule = "Rule3",
                    CohortEnabled = true
                }
            };

        }
    }
}
