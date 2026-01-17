using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Mappings
{
    public class CustomerMap : BaseMapping<CustomerModel>
    {
        public CustomerMap()
        {
            Schema("tenant");
            Table("customer");
            Id(x => x.CustomerId).Column("customer_id").GeneratedBy.Identity();
            Map(x => x.CustomerCode).Column("customer_code");
            Map(x => x.CustomerName).Column("customer_name");
            Map(x => x.CustomerDescription).Column("customer_description");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
        
    }
}
