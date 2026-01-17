using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly IAdminClient _adminClient;
        private readonly ILogger<EventService> _logger;
        private readonly IFisClient _fisClient;
        const string className = nameof(EventService);


        public EventService(ILogger<EventService> eventServiceLogger, IAdminClient adminClient, IFisClient fisClient)
        {
            _logger = eventServiceLogger;
            _adminClient = adminClient;
            _fisClient = fisClient;
        }

        /// <summary>
        /// Post Event to Admin, using Admin client
        /// </summary>
        /// <param name="PostEventRequestDto"></param>
        /// <returns></returns>
        public async Task<PostEventResponseDto> PostEvent(PostEventRequestDto postEventRequestDto)
        {
            const string methodName = nameof(PostEvent);
            try
            {
                _logger.LogInformation("Sending Event Request to Admin for consumer code {consumerCode}", postEventRequestDto.ConsumerCode);
                var response = await _adminClient.Post<PostEventResponseDto>(AdminConstants.PostEventAPIUrl, postEventRequestDto);
                _logger.LogInformation("{className}.{methodName}: Posted Event Request to Admin successfully for consumerCode : {consumerCode}", className, methodName, postEventRequestDto.ConsumerCode);
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostEvent - Error :{msg}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates the pick a purse event.
        /// </summary>
        /// <param name="consumerAccountDto">The consumer account dto.</param>
        public async Task<PostEventResponseDto> CreatePickAPurseEvent(ConsumerAccountDto consumerAccountDto)
        {
            try
            {
                var walletTypesResponse = await FetchConsumerBenefitWalletTypes(consumerAccountDto);
                if (walletTypesResponse == null)
                    return new PostEventResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Consumer benefit wallet types not found"
                    };

                var selectedPurseLabels = walletTypesResponse.BenefitsWalletTypes.Select(x => x.PurseLabel).ToList();

                var eventRequest = BuildPostEventRequest(consumerAccountDto, selectedPurseLabels);
                return await PostEvent(eventRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - An unexpected error occurred.", className, nameof(CreatePickAPurseEvent));
                return new PostEventResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = $"Error occurred while creating pick a purse event, Error Message: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Fetches the consumer benefit wallet types.
        /// </summary>
        /// <param name="consumerAccountDto">The consumer account dto.</param>
        /// <returns></returns>
        private async Task<ConsumerBenefitsWalletTypesResponseDto?> FetchConsumerBenefitWalletTypes(ConsumerAccountDto consumerAccountDto)
        {
            var requestDto = new ConsumerBenefitsWalletTypesRequestDto
            {
                ConsumerCode = consumerAccountDto.ConsumerCode!,
                TenantCode = consumerAccountDto.TenantCode!
            };

            var response = await _fisClient.Post<ConsumerBenefitsWalletTypesResponseDto>(WalletConstants.GetConsumerBenefitWalletTypesAPIUrl, requestDto);
            if (response.ErrorCode != null)
            {
                LogError(nameof(FetchConsumerBenefitWalletTypes), "fetching consumer benefit wallet types", requestDto, response.ErrorCode, response.ErrorMessage);
                return null;
            }

            return response;
        }

        /// <summary>
        /// Builds the post event request.
        /// </summary>
        /// <param name="consumerAccountDto">The consumer account dto.</param>
        /// <param name="selectedPurseLabels">The selected purse labels.</param>
        /// <returns></returns>
        private static PostEventRequestDto BuildPostEventRequest(ConsumerAccountDto consumerAccountDto, List<string?>? selectedPurseLabels)
        {
            return new PostEventRequestDto
            {
                ConsumerCode = consumerAccountDto.ConsumerCode!,
                TenantCode = consumerAccountDto.TenantCode!,
                EventType = EventType.PICK_A_PURSE.ToString(),
                EventSubtype = EventSubType.NONE.ToString(),
                EventSource = AdminConstants.EventSourceActivateCardAPI,
                EventData = new { pickedPurseLabels = selectedPurseLabels }
            };
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="requestData">The request data.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        private void LogError(string methodName, string operation, object requestData, int? errorCode, string? errorMessage)
        {
            _logger.LogError(
                "{ClassName}.{MethodName} - Error occurred while {Operation}: {RequestData}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                className, methodName, operation, requestData.ToJson(), errorCode, errorMessage
            );
        }

    }
}
