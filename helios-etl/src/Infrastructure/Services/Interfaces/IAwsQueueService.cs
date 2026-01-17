using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IAwsQueueService
    {
        /// <summary>
        /// Send message to aws queue service
        /// </summary>
        /// <param name="taskUpdateDto"></param>
        /// <returns>bool = true/false for result, message = information message</returns>
        Task<(bool, string)> PushToTaskUpdateQueue(EtlTaskUpdateDto taskUpdateDto);
        Task<(bool, string)> PushToBatchJobRecordQueue(ETLBatchJobRecordQueueRequestDto requestDto);
        Task<(bool, string)> PushToMemberImportEventDlqQueue(MemberEnrollmentDetailDto memberEnrollmentDetailDto);
    }
}
