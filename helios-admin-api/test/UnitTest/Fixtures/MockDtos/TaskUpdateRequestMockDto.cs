using SunnyRewards.Helios.Admin.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public  class TaskUpdateRequestMockDto : TaskUpdateRequestDto
    {
        public TaskUpdateRequestMockDto()
        {
            ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            TaskId = 2;
            TaskStatus = "completed";
            TaskCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            PartnerCode = "par-6f222db8ad104cfdbaf59d3c334b2586";
            MemberId = "60be2228-04f5-417f-9d33-0d1d78d7cb76";
            SupportLiveTransferToRewardsPurse = true;
            ImageName = "Sample";
            ImageType = "PRODUCT";
        }
    }
}
