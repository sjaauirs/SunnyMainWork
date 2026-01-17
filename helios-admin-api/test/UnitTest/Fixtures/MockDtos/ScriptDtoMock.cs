using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class ScriptDtoMock : ScriptDto
    {
        public ScriptDtoMock()
        {
            new List<ScriptDto>
            { new ScriptDto{
                ScriptId = 1,
                ScriptCode = "SCR001",
                ScriptName = "Script One",
                ScriptDescription = "This is a basic script to demonstrate the process.",
                ScriptJson = "{ \"operation\": \"add\", \"operands\": [1, 2] }",
                ScriptSource = "https://example.com/scripts/scriptone"
            }
            };

        }
    }
}
