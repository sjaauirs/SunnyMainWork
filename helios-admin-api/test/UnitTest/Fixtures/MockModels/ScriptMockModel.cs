using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockModels
{
    public class ScriptMockModel:ScriptModel
    {
        public ScriptMockModel()
        {
            ScriptId = 1;
            ScriptCode = "src-d051dabee30b4ca4a547c5dbba706510";
            ScriptName = "TASK_TRIGGER";
            ScriptJson = "Sample json";
            ScriptSource = "SomeSource";
        }
    }
}
