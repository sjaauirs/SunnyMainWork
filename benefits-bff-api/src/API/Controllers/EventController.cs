using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace SunnyRewards.Helios.Bff.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _eventLogger;
        private readonly IEventService _eventService;
        private const string className = nameof(EventController);

        /// <summary>
        /// Controller Action to send an Event to a Queue
        /// </summary>
        /// <param name="PostEventRequestDto"></param>
        /// <returns></returns>
        public EventController(ILogger<EventController> eventLogger, IEventService eventService)
        {
            _eventLogger = eventLogger;
            _eventService = eventService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PostEventRequestDto"></param>
        /// <returns></returns>
        [HttpPost("post-event")]
        public async Task<ActionResult<PostEventResponseDto>> PostEvents([FromBody] PostEventRequestDto postEventRequestDto)
        {
            const string methodName = nameof(PostEvents);
            try
            {
                _eventLogger.LogInformation("Post Event API - Started With Request : {request}", postEventRequestDto.ToJson());
                var response = await _eventService.PostEvent(postEventRequestDto);
                if (response.ErrorCode != null)
                {
                    _eventLogger.LogError("{ClassName}.{MethodName}: Error occurred while sending Event. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, postEventRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _eventLogger.LogInformation("{ClassName}.{MethodName}: JobReport event sent successfully , ConsumerCode: {consumerCode}", className, methodName, postEventRequestDto.ConsumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _eventLogger.LogError(ex, "ERROR - msg: {msg}", ex.Message);
                return new PostEventResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }

        }

    }
}