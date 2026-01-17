using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ETLMemberAttrRequestDto
    {
        /// <summary>
        /// Partner Code assgined by Rewards System for the customer
        /// </summary>
        public string PartnerCode { get; set; } = string.Empty;

        /// <summary>
        /// Array of Member Attributes
        /// </summary>
        public List<ETLMemberAttributeDetailDto> MemberAttributes { get; set; } = new List<ETLMemberAttributeDetailDto>();
    }
}
