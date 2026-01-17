using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTermsOfServiceMap : BaseMapping<ETLTermsOfServiceModel>
    {
        public ETLTermsOfServiceMap()
        {
            Schema("task");
            Table("terms_of_service");

            Id(x => x.TermsOfServiceId).Column("terms_of_service_id").GeneratedBy.Identity();
            Map(x => x.TermsOfServiceText).Column("terms_of_service_text").Not.Nullable();
            Map(x => x.LanguageCode).Column("language_code").Not.Nullable();
            Map(x => x.TermsOfServiceCode).Column("terms_of_service_code").Not.Nullable();

            Map(x => x.CreateTs).Column("create_ts").Not.Nullable();
            Map(x => x.UpdateTs).Column("update_ts").Nullable();
            Map(x => x.CreateUser).Column("create_user").Not.Nullable();
            Map(x => x.UpdateUser).Column("update_user").Nullable();
            Map(x => x.DeleteNbr).Column("delete_nbr").Not.Nullable();
        }
    }
}
