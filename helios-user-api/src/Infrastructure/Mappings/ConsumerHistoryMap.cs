using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{

    public class ConsumerHistoryMap : BaseConsumerMap<ConsumerHistoryModel>
    {
        public ConsumerHistoryMap() : base("consumer_history")
        {
            InitializeMapping();
        }

        protected override void MapId()
        {
            Id(x => x.ConsumerHistoryId).Column("consumer_history_id").GeneratedBy.Identity();
            Map(x => x.ConsumerId).Column("consumer_id");
        }
    }
}