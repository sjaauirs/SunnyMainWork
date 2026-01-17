using Microsoft.Extensions.DependencyInjection;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public interface IEventProcessorFactory
    {
        IEventProcessor? GetEventProcessor(string eventType);
        IEventDtoProcessor? GetEventDtoProcessor(string eventType);
        IConsumerTaskEventProcesser GetConsumerTaskEventProcesser();
        IAgreementsVerifiedEventProcessor GetAgreementsVerifiedEventProcessor();
    }


    public class EventProcessorFactory : IEventProcessorFactory
    {

        private  readonly IServiceProvider _serviceProvider;

        public EventProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEventProcessor? GetEventProcessor(string eventType)
        {
            try
            {

                return eventType switch
                {
                    nameof(EventType.TASK_TRIGGER) => _serviceProvider.GetRequiredService<ITaskTriggerEventProcessor>(),
                    nameof(EventType.PICK_A_PURSE) => _serviceProvider.GetRequiredService<IPickAPurseEventProcessor>(),
                    nameof(EventType.CONSUMER_HISTORY) => _serviceProvider.GetRequiredService<IConsumerUpdateEventProcessor>(),
                    _ => null
                };
            }
            catch(Exception)
            {
                return null;
            }
        }

        public IEventDtoProcessor? GetEventDtoProcessor(string eventType)
        {
            try
            {
                return eventType switch
                {
                    nameof(EventType.COHORT_EVENT) => _serviceProvider.GetRequiredService<IConsumerCohortEventProcessor>(),
                    // add more event types here:
                    // nameof(EventType.SOMETHING) => _sp.GetRequiredService<ISomethingEventProcessor>(),
                    _ => null
                };
            }
            catch { return null; }
        }

        public IConsumerTaskEventProcesser GetConsumerTaskEventProcesser()
        {
            return _serviceProvider.GetRequiredService<IConsumerTaskEventProcesser>();
        }

        public IAgreementsVerifiedEventProcessor GetAgreementsVerifiedEventProcessor()
        {
            return _serviceProvider.GetRequiredService<IAgreementsVerifiedEventProcessor>();
        }
    }
}
