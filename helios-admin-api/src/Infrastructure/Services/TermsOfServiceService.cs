using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TermsOfServiceService : ITermsOfServiceService
    {
        public readonly ILogger<TermsOfServiceService> _logger;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TermsOfServiceService);

        public TermsOfServiceService(ILogger<TermsOfServiceService> logger, ITaskClient taskClient)
        {
            _logger = logger;
            _taskClient = taskClient;
        }

        public async Task<BaseResponseDto> CreateTermsOfService(CreateTermsOfServiceRequestDto createTermsOfServiceRequestDto)
        {
            const string methodName = nameof(CreateTermsOfService);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create TermsOfService process started for ServiceId: {Id}", className, methodName, createTermsOfServiceRequestDto.TermsOfServiceId);

                var response = await _taskClient.Post<BaseResponseDto>(Constant.CreateTermsOfServiceUrl, createTermsOfServiceRequestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating TermsOfService, ServiceId: {Id}, ErrorCode: {ErrorCode}", className, methodName, createTermsOfServiceRequestDto.TermsOfServiceId, response.ErrorCode);
                    return response;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: TermsOfService created successfully, ServiceId: {Id}", className, methodName, createTermsOfServiceRequestDto.TermsOfServiceId);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating TermsOfService. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}
