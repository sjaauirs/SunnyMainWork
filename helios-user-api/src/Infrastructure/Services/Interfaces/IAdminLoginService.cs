using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IAdminLoginService
    {
        /// <summary>
        /// Creates a JWT token for the given consumer code.
        /// </summary>
        /// <param name="adminLoginRequestDto">The consumer code.</param>
        /// <returns>An instance of <see cref="AdminLoginResponseDto"/> containing the token or an error message.</returns>
        Task<AdminLoginResponseDto> GenerateAdminTokenAsync(AdminLoginRequestDto adminLoginRequestDto);
    }
}