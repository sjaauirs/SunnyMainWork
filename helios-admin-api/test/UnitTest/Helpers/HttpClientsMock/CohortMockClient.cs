using Moq;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock
{
    public class CohortMockClient : Mock<ICohortClient>
    {
        public CohortMockClient()
        {

            Setup(client => client.Post<ExportCohortResponseDto>(Constant.CohortExportAPIUrl, It.IsAny<ExportCohortRequestDto>()))
               .ReturnsAsync(new ExportCohortResponseMockDto());

            Setup(client => client.Post<BaseResponseDto>(Constant.CreateCohortAPIUrl, It.IsAny<CreateCohortRequestDto>()))
               .ReturnsAsync(new BaseResponseDto());

        }
    }
}
