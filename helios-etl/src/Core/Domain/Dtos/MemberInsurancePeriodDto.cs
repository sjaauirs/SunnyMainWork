using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class MemberInsurancePeriodDto : MemberGroupByKey
    {
        public string? ConsumerCode { get; set; }

        public DateTime? EligibleEndTs { get; set; }
        public  DateTime? EligibleStartTs { get; set; }
    }
}
