using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    /// <summary>
    /// Represents the response returned after an admin login attempt.
    /// Inherits common response properties from BaseResponseDto.
    /// </summary>
    public class AdminLoginResponseDto : BaseResponseDto
    {
        /// <summary>
        /// Gets or sets the consumer code associated with the logged-in user.
        /// This uniquely identifies the user within the system.
        /// </summary>
        public string? ConsumerCode { get; set; }

        /// <summary>
        /// Gets or sets the JWT (JSON Web Token) issued upon successful login.
        /// This token is used for authentication and authorization in subsequent requests.
        /// </summary>
        public string? Jwt { get; set; }

        /// <summary>
        /// Gets or sets the list of Access Control objects (ACL).
        /// Each RoleAccessDto represents a specific role or permission assigned to the user.
        /// Default value is an empty list to avoid null reference exceptions.
        /// </summary>
        public List<RoleAccessDto> Acl { get; set; } = [];
    }
}
