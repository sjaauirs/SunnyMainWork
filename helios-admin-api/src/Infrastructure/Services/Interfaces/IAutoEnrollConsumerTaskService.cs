using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IAutoEnrollConsumerTaskService
    {
        /// <summary>
        /// Enrolls the consumer task.
        /// </summary>
        /// <param name="autoEnrollConsumerTaskRequest">The automatic enroll consumer task request.</param>
        /// <returns></returns>
        BaseResponseDto EnrollConsumerTask(AutoEnrollConsumerTaskRequestDto autoEnrollConsumerTaskRequest);
    }
}
