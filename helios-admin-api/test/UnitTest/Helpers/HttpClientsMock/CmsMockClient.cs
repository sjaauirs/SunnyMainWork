using Moq;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock
{
    public class CmsMockClient : Mock<ICmsClient>
    {
        public CmsMockClient()
        {

            Setup(client => client.Post<ExportCmsResponseDto>(Constant.CohortExportAPIUrl, It.IsAny<ExportCmsResponseDto>()))
               .ReturnsAsync(new ExportCmsResponseDto());

        }
    }
}
