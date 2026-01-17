using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    public class AddressTypeMap : BaseMapping<AddressTypeModel>
    {
        public AddressTypeMap() 
        {
            Schema("huser");
            Table("address_type");
            Id(x => x.AddressTypeId).Column("address_type_id").GeneratedBy.Identity();
            Map(x => x.AddressTypeCode).Column("address_type_code");
            Map(x => x.AddressTypeName).Column("address_type_name");
            Map(x => x.Description).Column("description");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
