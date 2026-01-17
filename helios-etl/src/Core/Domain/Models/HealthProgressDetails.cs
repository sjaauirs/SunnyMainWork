using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class HealthProgressDetails
    {
        public virtual string? DetailType { get; set; }
        public virtual RollupDataDto? HealthProgress { get; set; }
    }
}
