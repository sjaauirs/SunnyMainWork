using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Processes Consumers.
    /// </summary>
    /// <param name="etlExecutionContext"></param>
    /// <returns></returns>
    public interface IConsumerService
    {
        /// <summary>
        /// Delete Consumers
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        Task DeleteConsumers(EtlExecutionContext etlExecutionContext);
        Task<List<MemberEnrollmentDetailDto>> GetUpdatedInsurancePeriod(List<MemberEnrollmentDetailDto> memberCsvDtoList);
            /// <summary>
            /// Update consumer Enrollment status
            /// </summary>
            /// <param name="membersResponseDto"></param>
            /// <param name="actionType"></param>
            /// <returns></returns>
        Task UpdateConsumerEnrollment(MembersResponseDto membersResponseDto, String actionType);
    }
}
