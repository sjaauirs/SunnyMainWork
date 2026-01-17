using SunnyRewards.Helios.ETL.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponseDto> GetXAPISessionToken(string tenantCode, CustomerRequestDto customerRequestDto);
    }
}
