using SunnyRewards.Helios.Admin.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockModels
{
    public class TenantTaskRewardScriptModelMock : TenantTaskRewardScriptModel
    {
        public TenantTaskRewardScriptModelMock() {

            TenantTaskRewardScriptId = 1;
            TenantTaskRewardScriptCode = "trs-b46f38e9a6d34f699ffe0edd01b8cb28";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            TaskRewardCode = "trw-01108c0573634b608f5cc7c040dce95f";
            ScriptType = "TASK_COMPLETE_POST";
            ScriptId = 3;

        }
    }
}
