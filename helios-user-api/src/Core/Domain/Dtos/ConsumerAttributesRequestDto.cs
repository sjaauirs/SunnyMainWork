using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerAttributesRequestDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string TenantCode { get; set; } = string.Empty;

        /// <summary>
        /// Array of Member Attributes
        /// </summary>
        /// 
        public ConsumerAttributeDetailDto[] ConsumerAttributes { get; set; } = new ConsumerAttributeDetailDto[0];
    }
}
