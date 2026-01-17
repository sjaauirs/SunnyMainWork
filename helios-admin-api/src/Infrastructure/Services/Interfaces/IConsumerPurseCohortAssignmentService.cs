using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IConsumerPurseCohortAssignmentService
    {
        bool ConsumerPurseCohortAssignment(ConsumerDto consumerDto);
    }
}
