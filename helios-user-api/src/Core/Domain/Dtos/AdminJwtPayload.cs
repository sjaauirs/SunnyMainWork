using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    /// <summary>
    /// Represents the payload of a JWT (JSON Web Token) for an admin user.
    /// Inherits common properties from BaseResponseDto.
    /// </summary>
    public class AdminJwtPayload : BaseResponseDto
    {
        /// <summary>
        /// Gets or sets the consumer code associated with the admin user.
        /// This uniquely identifies the user within the system.
        /// </summary>
        public string? ConsumerCode { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time of the JWT.
        /// Indicates when the token is no longer valid.
        /// </summary>
        public DateTime Expiry { get; set; }

        /// <summary>
        /// Gets or sets the environment where the token is issued.
        /// This could represent production, staging, or development environments.
        /// </summary>
        public string? Environment { get; set; }

        /// <summary>
        /// Gets or sets the list of Access Control objects (ACL).
        /// Each RoleAccessDto specifies a permission or role assigned to the user.
        /// Default value is an empty list to avoid null reference exceptions.
        /// </summary>
        public IList<RoleAccessDto> Acl { get; set; } = [];
    }
}
