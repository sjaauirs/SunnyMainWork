using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class MemberGroupByKey
    {
        public string MemberId { get; set; }
        public string PartnerCode { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not MemberGroupByKey other)
                return false;

            return string.Equals(MemberId, other.MemberId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(PartnerCode, other.PartnerCode, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                MemberId?.ToLowerInvariant(),
                PartnerCode?.ToLowerInvariant()
            );
        }
    }

}
