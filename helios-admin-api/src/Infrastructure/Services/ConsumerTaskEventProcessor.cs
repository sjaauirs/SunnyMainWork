using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ConsumerTaskEventProcessor : IConsumerTaskEventProcesser
    {

        private readonly ILogger<ConsumerTaskEventProcessor> _logger;
        private readonly IConsumerTaskEventService _consumerTaskEventService;
        private readonly IConsumerEventProcessorHelper _eventProcessorHelper;
        private readonly IVault _vault;
        private async Task<string> GetEnvironmentName() => await _vault.GetSecret(Constants.ENV);
        const string className = nameof(ConsumerTaskEventProcessor);


        public ConsumerTaskEventProcessor(ILogger<ConsumerTaskEventProcessor> logger,
           IConsumerTaskEventService consumerTaskEventService,
           IConsumerEventProcessorHelper eventProcessorHelper,IVault vault)
        {
            _logger = logger;
            _consumerTaskEventService = consumerTaskEventService;
            _eventProcessorHelper = eventProcessorHelper;
            _vault = vault;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventRequest"></param>
        /// <returns></returns>
        public async Task<bool> ProcessEvent(EventDto<ConsumerTaskEventDto> eventRequest)
        {
            const string methodName = nameof(ProcessEvent);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} : Started processing ConsumerCode:{ConsumeCode},TenantCode:{TenantCode}",
                        className, methodName, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);
                var currentEnv = await GetEnvironmentName();

                var (IsValid, ErrorMessage) = ValidateEventRequest(eventRequest, methodName, currentEnv);

                if (!IsValid)
                {
                    throw new InvalidOperationException(ErrorMessage);
                }
                
                var consumerTaskEventRequestDto = new ConsumerTaskEventRequestDto
                {
                    ConsumerCode = eventRequest.Header.ConsumerCode ?? string.Empty,
                    TenantCode = eventRequest.Header.TenantCode ?? string.Empty,
                };

                var postedEventData = new PostedEventData
                {
                    EventSubtype = eventRequest.Header.EventSubtype,
                    EventType = eventRequest.Header.EventType,
                };

                var argInstances = new Dictionary<string, object>
                {
                     { nameof(ConsumerTaskEventRequestDto), consumerTaskEventRequestDto },
                     { nameof(ConsumerTaskEventService), _consumerTaskEventService },
                     { nameof(PostedEventData), postedEventData }
                };

                var eventResult =  await _eventProcessorHelper.ProcessEventAsync(eventRequest, argInstances, className);

                _logger.LogInformation("{ClassName}.{MethodName} : Successfully processed ConsumerCode:{ConsumeCode},TenantCode:{TenantCode}",
                        className, methodName, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);

                return eventResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"{ClassName}.{MethodName} : Error occurred while processing for ConsumerCode:{ConsumeCode},TenantCode:{TenantCode},Error:{Error}",
                        className, methodName, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode,ex.Message);
                throw;
            }
            
        }
        private (bool,string) ValidateEventRequest(EventDto<ConsumerTaskEventDto> eventRequest, string methodName, string currentEnvironment)
        {
            if (eventRequest.Data == null)
            {
                _logger.LogWarning("{ClassName}.{MethodName}: Received event with null data. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    className, methodName, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);
                return (false, "Received event with null data.");
            }

            if (!eventRequest.Header.EventType.Equals(Constant.ConsumerTask))
            {
                _logger.LogWarning("{ClassName}.{MethodName}: Discarded event due to mismatched EventType. Expected: {ExpectedEventType}, Received: {ReceivedEventType}. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    className, methodName, Constant.ConsumerTask, eventRequest.Header.EventType, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);
                return (false, "Mismatched EventType. EventType should be consumer task");
            }

            if (eventRequest.Header.Environment is not null && !eventRequest.Header.Environment.Equals(currentEnvironment))
            {
                _logger.LogWarning("{ClassName}.{MethodName}: Environment mismatch. Expected: {ExpectedEnvironment}, Received: {ReceivedEnvironment}. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    className, methodName, currentEnvironment, eventRequest.Header.Environment, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);
                return (false, "Environment mismatch.");
            }

            if (!eventRequest.Header.EventSubtype.Equals(Constant.ConsumerTaskUpdate))
            {
                _logger.LogWarning("{ClassName}.{MethodName}: Invalid Event Subtype. Expected: {ExpectedSubtype}, Received: {ReceivedSubtype}. EventType: {EventType}",
                    className, methodName, Constant.ConsumerTaskUpdate, eventRequest.Header.EventSubtype, eventRequest.Header.EventType);
                return (false, "Invalid Event Subtype.");
            }

            if (!eventRequest.Data.Status.Equals(Constant.TaskStatus.Completed))
            {
                _logger.LogWarning("{ClassName}.{MethodName}: Task status is not 'Completed'. Received Status: {Status}. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    className, methodName, eventRequest.Data.Status, eventRequest.Header.ConsumerCode, eventRequest.Header.TenantCode);
                return (false, "Task status is not 'Completed'");
            }

            return (true,null);
        }

    }
} 