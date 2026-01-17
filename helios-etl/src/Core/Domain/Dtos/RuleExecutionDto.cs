using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public class CohortRuleExecutionDto
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CohortRuleSuccessResult { get; set; }
    }
}
