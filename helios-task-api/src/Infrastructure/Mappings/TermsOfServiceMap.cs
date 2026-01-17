using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TermsOfServiceMap : BaseMapping<TermsOfServiceModel>
    {
        public TermsOfServiceMap()
        {
            Table("terms_of_service");
            Schema("task");
            Id(x => x.TermsOfServiceId).Column("terms_of_service_id").GeneratedBy.Identity();
            Map(x => x.TermsOfServiceText).Column("terms_of_service_text");
            Map(x => x.LanguageCode).Column("language_code");
            Map(x => x.TermsOfServiceCode).Column("terms_of_service_code");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }
}