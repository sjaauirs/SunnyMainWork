using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLPersonRoleModel : BaseModel
    {
        public virtual long RoleId { get; set; }

        public virtual long PersonId { get; set; }
    }

}
