using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ICardOperationService
    {
        Task<ConsumerCardsStatusResponseDto> GetConsumerCardsStatus(GetCardStatusRequestDto requestDto);
    }
}
