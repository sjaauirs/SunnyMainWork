using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{

    public interface ITaskTriggerEventProcessor : IEventProcessor
    {
    }

    //Hander for TaskTriggerEvent
    public class TaskTriggerEventProcessor : ITaskTriggerEventProcessor
    {
        private readonly ILogger<TaskTriggerEventProcessor> _logger;
        private readonly IAutoEnrollConsumerTaskService _autoEnrollConsumerTaskService;
        private readonly IEventProcessorHelper _eventProcessorHelper;
        const string className = nameof(TaskTriggerEventProcessor);

        public TaskTriggerEventProcessor(ILogger<TaskTriggerEventProcessor> logger,
            IAutoEnrollConsumerTaskService autoEnrollConsumerTaskService,
           IEventProcessorHelper eventProcessorHelper)
        {
            _logger = logger;
            _autoEnrollConsumerTaskService = autoEnrollConsumerTaskService;
            _eventProcessorHelper = eventProcessorHelper;
        }
        public async Task<bool> ProcessEvent(PostEventRequestModel eventRequest)
        {

            var autoEnrollConsumerTaskRequestDto = new AutoEnrollConsumerTaskRequestDto
            {
                ConsumerCode = eventRequest.ConsumerCode,
                TenantCode = eventRequest.TenantCode,
            };

            var postedEventData = new PostedEventData
            {
                EventSubtype = eventRequest.EventSubtype,
                EventType = eventRequest.EventType,
            };

            var argInstances = new Dictionary<string, object>
        {
            { nameof(AutoEnrollConsumerTaskRequestDto), autoEnrollConsumerTaskRequestDto },
            { nameof(AutoEnrollConsumerTaskService), _autoEnrollConsumerTaskService },
            { nameof(PostedEventData), postedEventData }
        };

            try
            {
                return await _eventProcessorHelper.ProcessEventAsync(eventRequest, argInstances, className);
            }
            catch (Exception )
            {
                throw;
            }
        }

    }
}
