using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public class ConsumerAttributesRequestMockDto : ConsumerAttributesRequestDto
    {
        public ConsumerAttributesRequestMockDto()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";


            ConsumerAttributes = new ConsumerAttributeDetailDto[]
         {
          new ConsumerAttributeDetailDto
          {
              ConsumerCode = "cmr-bjuebdf-492f-46i4-bh55-5a2b0134cbc",
              GroupName = "SurveyTaskRewardCodes",
              AttributeName = "trw-11e8eeb6b0ec4cfa8aa6f82abdd4e4b9",
              AttributeValue = "1"
          },
         };
        }
    }
}