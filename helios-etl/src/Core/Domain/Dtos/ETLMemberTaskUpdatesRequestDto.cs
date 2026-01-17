using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ETLMemberTaskUpdatesRequestDto
    {
        /// <summary>
        /// Array of Member Task (Action) Updates
        /// </summary>
        public List<ETLMemberTaskUpdateDetailDto> MemberTaskUpdates = new   ();
    }
}
