using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
namespace Sunny.Benefits.Bff.Infrastructure.Helpers.Interface
{
    public interface IPersonHelper
    {
        Task<bool> UpdateOnBoardingState(UpdateOnboardingStateDto updateOnboardingStateDto);
        Task<GetPersonAndConsumerResponseDto?> GetPersonDetails(GetConsumerRequestDto getConsumerRequestDto);
        Task<bool> ValidatePersonIsVerified(GetConsumerRequestDto consumerCode);

        Task<ConsumerResponseDto> UpdateConsumer(long consumerId, ConsumerDto consumerDto, string auth0UserName);
        Task<bool> UpdateOnBoardingTask(ConsumerDto consumerDto);
        Task<TenantDto> GetTenantByTenantCode(string tenantCode);
    }
}