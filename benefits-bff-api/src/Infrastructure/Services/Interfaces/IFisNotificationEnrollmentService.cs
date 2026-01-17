using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Fis.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IFisNotificationEnrollmentService
    {
        public Task<FisEnrollNotificationsResponseDto> SetNotificationsEnrollmentAsync(FisSetEnrollNotificationsRequestDto requestDto);
        public Task<FisGetNotificationsEnrollmentResponseDto> GetNotificationsEnrollmentAsync(FisGetNotificationsEnrollmentRequestDto requestDto);
        public Task<FisGetClientConfigResponseDto> GetClientConfigAsync(FisGetNotificationsEnrollmentRequestDto requestDto);
    }
}
