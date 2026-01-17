using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{

    public class ConsumerMap : BaseConsumerMap<ConsumerModel>
    {
        public ConsumerMap() : base("consumer") {

            InitializeMapping();
        }

        protected override void MapId()
        {
            Id(x => x.ConsumerId).Column("consumer_id").GeneratedBy.Identity();
        }
    }

    
}