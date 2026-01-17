using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    /// <summary>
    /// AccessControlListResponseDTO
    /// </summary>
    public class AccessControlListResponseDTO : BaseResponseDto
    {
        /// <summary>
        /// Indicates if the user has Super Admin privileges.
        /// </summary>
        public bool IsSuperAdmin { get; set; }

        /// <summary>
        /// Indicates if the user is a Subscriber.
        /// </summary>
        public bool IsSubscriber { get; set; }

        /// <summary>
        /// Indicates if the user is a Report User.
        /// </summary>
        public bool IsReportUser { get; set; }

        /// <summary>
        /// List of customer codes where both sponsor_code and tenant_code are "All".
        /// </summary>
        public IList<string>? CustomerAdminCustomerCodes { get; set; }

        /// <summary>
        /// List of sponsor codes where tenant_code is "All".
        /// </summary>
        public IList<string>? SponsorAdminSponsorCodes { get; set; }

        /// <summary>
        /// List of specific tenant codes where the current user is a Tenant Admin.
        /// </summary>
        public IList<string>? TenantAdminTenantCodes { get; set; }
    }

}
