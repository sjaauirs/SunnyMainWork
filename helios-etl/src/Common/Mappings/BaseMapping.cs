using SunnyRewards.Helios.ETL.Common.Domain.Models;
using FluentNHibernate.Mapping;

namespace SunnyRewards.Helios.ETL.Common.Mappings
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseMapping<T> : ClassMap<T> where T : BaseModel, new()
    {
        /// <summary>
        /// 
        /// </summary>
        public void AddBaseColumnMap()
        {
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
