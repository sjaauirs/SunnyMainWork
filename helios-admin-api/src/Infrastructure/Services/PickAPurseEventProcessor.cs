using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using System.Text.Json;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{ 

    public interface IPickAPurseEventProcessor : IEventProcessor
    {
    }


    public class PickAPurseEventProcessor : IPickAPurseEventProcessor
    {
        private readonly ILogger<PickAPurseEventProcessor> _logger;
        private readonly IEventProcessorHelper _eventProcessorHelper;
        private readonly IOnBoardingInitialFundingService _onBoardingInitialFundingService;
        const string className = nameof(PickAPurseEventProcessor);
        public PickAPurseEventProcessor(
            ILogger<PickAPurseEventProcessor> logger,
            IEventProcessorHelper eventProcessorHelper,
            IOnBoardingInitialFundingService onBoardingInitialFundingService)
        {
            _logger = logger;
            _eventProcessorHelper = eventProcessorHelper;
            _onBoardingInitialFundingService = onBoardingInitialFundingService;
        }

        public async Task<bool> ProcessEvent(PostEventRequestModel eventRequest)
        {
            try
            {
                var pickParseEventData = JsonSerializer.Deserialize<PickedPurseEventDataDto>(eventRequest.EventData);
                if (pickParseEventData != null)
                {
                    var initialFundingRequestDto = new InitialFundingRequestDto
                    {
                        ConsumerCode = eventRequest.ConsumerCode,
                        TenantCode = eventRequest.TenantCode,
                        SelectedPurses = pickParseEventData.pickedPurseLabels,
                    };





                    var argInstances = new Dictionary<string, object>
                        {
                            { nameof(InitialFundingRequestDto), initialFundingRequestDto },
                            { nameof(OnBoardingInitialFundingService), _onBoardingInitialFundingService }
                        };

                    return await _eventProcessorHelper.ProcessEventAsync(eventRequest, argInstances, className);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }   
    }
}
