using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IConsumerService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerRequestDto"></param>
        /// <returns></returns>
        Task<GetConsumerResponseDto> GetConsumerData(GetConsumerRequestDto consumerRequestDto);

        /// <summary>
        /// Retrieves Consumer matching the given Tenant+MemNbr
        /// </summary>
        /// <param name="consumerRequestDto"></param>
        /// <returns></returns>
        Task<GetConsumerByMemIdResponseDto> GetConsumerByMemId(GetConsumerByMemIdRequestDto consumerRequestDto);
        /// <summary>
        /// CreateConsumers
        /// </summary>
        /// <param name="consumersCreateRequestDto"></param>
        /// <returns></returns>
        Task<List<ConsumerDataResponseDto>> CreateConsumers(IList<ConsumerDataDto> consumersCreateRequestDto);

        /// <summary>
        /// UpdateConsumers
        /// </summary>
        /// <param name="consumersUpdateRequestDto"></param>
        /// <param name="isCancel"></param>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        Task<List<ConsumerDataResponseDto>> UpdateConsumers(IList<ConsumerDataDto> consumersUpdateRequestDto, bool isCancel = false, bool isDelete = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerAttributesRequestDto"></param>
        /// <returns></returns>
        Task<ConsumerAttributesResponseDto> ConsumerAttributes(ConsumerAttributesRequestDto consumerAttributesRequestDto);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<GetConsumerByEmailResponseDto> GetConsumerByEmail(string email);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumer"></param>
        /// <returns></returns>
        Task<ConsumerModel> updateRegisterFlag(ConsumerDto consumer);

        /// <summary>
        /// Retrieves consumer and person details based on the tenant code provided.
        /// </summary>
        /// <param name="consumerByTenantRequestDto">Contains tenant code, search term, and pagination parameters.</param>
        /// <returns>A paginated list of consumer and person details that match the search criteria.</returns>
        /// <remarks>This method performs optional search, and pagination.</remarks>
        Task<ConsumersAndPersonsListResponseDto> GetConsumersByTenantCode(GetConsumerByTenantRequestDto consumerByTenantRequestDto);
        /// <summary>
        /// Updates the consumer
        /// </summary>
        /// <param name="consumerRequestDto">The data transfer object containing consumer details to be updated.</param>
        /// <returns>
        /// A <see cref="ConsumerResponseDto"/> containing the updated consumer details or error information if the update fails.
        /// </returns>
        Task<ConsumerResponseDto> UpdateConsumerAsync(long consumerId, ConsumerRequestDto consumerRequestDto);

        Task<ConsumerResponseDto> UpdateOnboardingState(UpdateOnboardingStateDto updateOnboardingStateDto);

        Task<GetConsumerByPersonUniqueIdentifierResponseDto> GetConsumerByPersonUniqueIdentifier(string personUniqueIdentifier);

        Task<ConsumersAndPersonsListResponseDto> GetConsumersByConsumerCodes(GetConsumerByConsumerCodes getConsumerByConsumerCodes);

        Task<ConsumerResponseDto> UpdateAgreementStatus(UpdateAgreementStatusDto updateAgreementStatusDto);

        Task<ConsumerResponseDto> UpdateEnrollmentStatus(UpdateEnrollmentStatusRequestDto requestDto);

        Task<ConsumerPersonResponseDto> GetConsumersByMemberNbrAndRegionCode(string memberNbr, string regionCode);
        Task<ConsumersAndPersonsListResponseDto> GetConsumerByDOB(GetConsumerByTenantCodeAndDOBRequestDto consumerRequestDto);

        Task<BaseResponseDto> UpdateConsumerSubscriptionStatus(ConsumerSubscriptionStatusRequestDto requestDto);
    }
}