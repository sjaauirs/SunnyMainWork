using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Bff.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Bff.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly ILogger<EventService> _eventServiceLogger;
        private readonly IAwsQueueService _awsQueueService;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventServiceLogger"></param>
        /// <param name="awsQueueService"></param>
        public EventService(ILogger<EventService> eventServiceLogger, IAwsQueueService awsQueueService, IMapper mapper)
        {
            _eventServiceLogger = eventServiceLogger;
            _awsQueueService = awsQueueService;
            _mapper = mapper;
        }

        /// <summary>
        /// Posting an event to Queue
        /// </summary>
        /// <param name="PostEventRequestDto"></param>
        /// <returns>PostEventResponseDto</returns>
        public async Task<PostEventResponseDto> PostEvent(PostEventRequestDto postEventRequestDto)
        {
            var result = new PostEventResponseDto();
            try
            {
                var eventModel = _mapper.Map<PostEventRequestModel>(postEventRequestDto);
                _eventServiceLogger.LogInformation("Posting Event to the queue for ConsumerCode : {consumerCode}", postEventRequestDto.ConsumerCode);

                var (isSuccessful, message) = await _awsQueueService.PushEventToConsumerEventQueue(eventModel);
                if (isSuccessful)
                {
                    _eventServiceLogger.LogInformation("Event Posted to Queue successfully, EventCode {EventCode}" , eventModel.EventCode);  
                }
                else
                {
                    _eventServiceLogger.LogWarning("PostEvent: push to aws queue failed for ConsumerCode : {ConsumerCode}" , postEventRequestDto.ConsumerCode);
                    result.ErrorCode = StatusCodes.Status500InternalServerError;
                    result.ErrorMessage = message;
                    return result;
                }
            }
            catch (Exception ex)
            {
                _eventServiceLogger.LogError(ex, "PostEvent - Error :{msg}", ex.Message);
                throw;
            }
            return result;
        }

        public async Task<PostEventResponseDto> PostErrorEvent(ConsumerErrorEventDto consumerErrorEventDto)
        {
            var result = new PostEventResponseDto();
            try
            {
                _eventServiceLogger.LogInformation("Posting Event to the queue for Error : {Detail}", consumerErrorEventDto.Message.Detail);

                var (isSuccessful, message) = await _awsQueueService.PushMessageToErrortQueue(consumerErrorEventDto);
                if (isSuccessful)
                {
                    _eventServiceLogger.LogInformation("Event Posted to Queue successfully, EventCode {EventCode}", consumerErrorEventDto.EventCode);
                }
                else
                {
                    _eventServiceLogger.LogWarning("PostEvent: push to aws queue failed for EventCode : {EventCode}", consumerErrorEventDto.EventCode);
                    result.ErrorCode = StatusCodes.Status500InternalServerError;
                    result.ErrorMessage = message;
                    return result;
                }
            }
            catch (Exception ex)
            {
                _eventServiceLogger.LogError(ex, "PostEvent - Error :{msg}", ex.Message);
                throw;
            }
            return result;
        }
    }
}
