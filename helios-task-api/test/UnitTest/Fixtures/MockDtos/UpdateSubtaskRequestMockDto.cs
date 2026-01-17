using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class UpdateSubtaskRequestMockDto : UpdateSubtaskRequestDto
    {
        public UpdateSubtaskRequestMockDto()
        {
            ConsumerTaskId = 2;
            CompletedTaskId = 1;
        }
    }
}