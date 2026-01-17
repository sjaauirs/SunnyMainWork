using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    public class ServerLoginMap : BaseMapping<ServerLoginModel>
    {
        public ServerLoginMap()
        {
            Schema("huser");
            Table("server_login");

            Id(x => x.ServerLoginId).Column("server_login_id").GeneratedBy.Identity();
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.LoginTs).Column("login_ts");
            Map(x => x.RefreshTokenTs).Column("refresh_token_ts");
            Map(x => x.LogoutTs).Column("logout_ts");
            Map(x => x.ApiToken).Column("api_token");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
