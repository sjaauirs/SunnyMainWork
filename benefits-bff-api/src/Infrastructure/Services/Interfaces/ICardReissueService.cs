using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;


namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface ICardReissueService
    {
        Task<ExecuteCardReissueResponseDto> ExecuteCardReissue(CardReissueRequestDto cardReissueRequestDto);
    }
}
