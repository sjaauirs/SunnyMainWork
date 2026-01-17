namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    /// <summary>
    /// RoleAccessDto
    /// </summary>
    public class RoleAccessDto
    {
        /// <summary>
        /// Customer code. If "ALL", then the role is admin (super admin).
        /// </summary>
        public string? CustomerCode { get; set; }

        /// <summary>
        /// The name of the Customer associated with the role.
        /// </summary>
        public string? CustomerName { get; set; }

        /// <summary>
        /// Sponsor code. If "ALL", then the role is customer_admin for the specific CustomerCode.
        /// </summary>
        public string? SponsorCode { get; set; }
        /// <summary>
        /// The name of the Sponsor associated with the role.
        /// </summary>
        public string? SponsorName { get; set; }

        /// <summary>
        /// Tenant code. If "ALL", then the role is sponsor_admin for the specific SponsorCode.
        /// </summary>
        public string? TenantCode { get; set; }
        /// <summary>
        /// The name of the Tenant associated with the role.
        /// </summary>
        public string? TenantName { get; set; }

        /// <summary>
        /// Role assigned. Possible values: subscriber, admin, customer_admin, sponsor_admin, tenant_admin, report_user.
        /// </summary>
        public string? Role { get; set; }
    }
}
