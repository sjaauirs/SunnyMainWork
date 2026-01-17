using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Mappings
{
    public class SponsorModelMap : BaseMapping<SponsorModel>
    {
        public SponsorModelMap()
        {
            Schema("tenant");
            Table("sponsor"); // Replace "sponsor_table" with your actual database table name

            Id(x => x.SponsorId).Column("sponsor_id").GeneratedBy.Identity();
            Map(x => x.CustomerId).Column("customer_id");
            Map(x => x.SponsorCode).Column("sponsor_code");
            Map(x => x.SponsorName).Column("sponsor_name");
            Map(x => x.SponsorDescription).Column("sponsor_description");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
