using Sunny.Benefits.Bff.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IValidicService
    {
        Task<CreateValidicUserResponseDto> CreateValidicUser(CreateValidicUserRequestDto request);
    }
}