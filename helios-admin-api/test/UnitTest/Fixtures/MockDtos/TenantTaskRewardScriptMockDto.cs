using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class TenantTaskRewardScriptMockDto : TenantTaskRewardScriptRequestDto
    {
        public TenantTaskRewardScriptMockDto()
        {

            TenantCode = "Tenant1";
            TaskRewardCode = "TR001";
            ScriptType = "Type1";
            ScriptCode = "SCR001";

        }
    }
}
