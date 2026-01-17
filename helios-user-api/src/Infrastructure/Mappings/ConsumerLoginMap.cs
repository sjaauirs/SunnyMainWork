using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    public class ConsumerLoginMap : BaseMapping<ConsumerLoginModel>
    {
        public ConsumerLoginMap()
        {
            Schema("huser");
            Table("consumer_login");

            Id(x => x.ConsumerLoginId).Column("consumer_login_id").GeneratedBy.Identity();
            Map(x => x.ConsumerId).Column("consumer_id");
            Map(x => x.LoginTs).Column("login_ts");
            Map(x => x.RefreshTokenTs).Column("refresh_token_ts");
            Map(x => x.LogoutTs).Column("logout_ts");
            Map(x => x.AccessToken).Column("access_token");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UserAgent).Column("user_agent");
            Map(x => x.TokenApp).Column("token_app");
        }
    }
}