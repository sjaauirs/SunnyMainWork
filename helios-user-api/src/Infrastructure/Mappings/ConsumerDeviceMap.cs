using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    public class ConsumerDeviceMap : BaseMapping<ConsumerDeviceModel>
    {
        public ConsumerDeviceMap()
        {
            Schema("huser");
            Table("consumer_device");
            Id(x => x.ConsumerDeviceId).Column("consumer_device_id").GeneratedBy.Identity();
            Map(x => x.ConsumerDeviceCode).Column("consumer_device_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.DeviceIdHash).Column("device_id_hash");
            Map(x => x.DeviceIdEnc).Column("device_id_enc");
            Map(x => x.DeviceType).Column("device_type");
            Map(x => x.DeviceAttrJson).Column("device_attr_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
