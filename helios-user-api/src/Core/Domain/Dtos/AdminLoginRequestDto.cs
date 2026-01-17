using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class AdminLoginRequestDto
    {
        /// <summary>
        /// Gets or sets the consumer code associated with the admin user.
        /// This uniquely identifies the user within the system.
        /// </summary>
        public string? ConsumerCode { get; set; }
    }
}