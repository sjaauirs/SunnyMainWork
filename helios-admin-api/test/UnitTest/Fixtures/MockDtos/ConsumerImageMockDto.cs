using Sunny.Benefits.Cms.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class ConsumerImageMockDto:ConsumerImageDto
    {
        public ConsumerImageMockDto()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerImageCode = "cic-ecada21e57154928a2bb959e8365b";
            ImageType = "Product";
            ImagePath = "images/ten-ecada21e57154928a2bb959e8365b8b4/cmr-7c476a48e3324999a02e8830a93948f1/IMG_0004_a9adacf150f64886afe55bb87b1f1636.jpg";
        }
    }
}
