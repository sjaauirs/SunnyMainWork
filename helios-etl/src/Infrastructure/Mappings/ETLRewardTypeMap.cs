using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLRewardTypeMap : BaseMapping<ETLRewardTypeModel>
    {
        public ETLRewardTypeMap()
        {
            Schema("task");
            Table("reward_type");
            Id(x => x.RewardTypeId).Column("reward_type_id").GeneratedBy.Identity();
            Map(x => x.RewardTypeCode).Column("reward_type_code");
            Map(x => x.RewardTypeName).Column("reward_type_name");
            Map(x => x.RewardTypeDescription).Column("reward_type_description");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }
}