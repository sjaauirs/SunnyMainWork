using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Bff.Infrastructure.Services.Interfaces
{
    public interface IAwsQueueService
    {
        /// <summary>
        /// Post event to consumer Event queue
        /// </summary>
        /// <param name="postEventRequestDto"></param>
        /// <returns></returns>
        Task<(bool, string)> PushEventToConsumerEventQueue(PostEventRequestModel postEventRequestModel);
        /// <summary>
        /// Post event to consumer Event dead letter queue
        /// </summary>
        /// <param name="postEventRequestDto"></param>
        Task<(bool, string)> PushEventToConsumerEventDeadLetterQueue(string postEventRequestModel);

        /// <summary>
        /// Return name of consumer event queue
        /// </summary>
        /// <param name="postEventRequestDto"></param>
        Task<string> GetAwsConsumerEventQueueUrl();

        int GetMaxNumberOfMessages();
        int GetWaitTimeSeconds();

        Task<string> GetAwsConsumerCohortQueueUrl();
        Task<(bool, string)> PushEventToConsumerCohortEventDeadLetterQueue(string postEventRequestModel);

        Task<(bool, string)> PushMessageToErrortQueue(ConsumerErrorEventDto consumerErrorEventDto);
    }
}
