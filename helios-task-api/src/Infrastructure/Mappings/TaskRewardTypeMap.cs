using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TaskRewardTypeMap : BaseMapping<TaskRewardTypeModel>
    {
        public TaskRewardTypeMap() 
        {
            Table("reward_type");
            Schema("task");
            Id(x => x.RewardTypeId).Column("reward_type_id").GeneratedBy.Identity();
            Map(x => x.RewardTypeName).Column("reward_type_name");
            Map(x => x.RewardTypeDescription).Column("reward_type_description");
            Map(x => x.RewardTypeCode).Column("reward_type_code");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
           
        }
    }
}
