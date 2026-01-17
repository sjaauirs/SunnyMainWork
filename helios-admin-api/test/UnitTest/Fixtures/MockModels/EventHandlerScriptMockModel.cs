using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockModels
{
    public class EventHandlerScriptMockModel:EventHandlerScriptModel
    {
        public EventHandlerScriptMockModel()
        {
            EventHandlerId = 64;
            EventHandlerCode = "evh-95a9df5973f64e419c88bf4d6f335630";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ScriptId = 1;
            EventType = "TASK_TRIGGER";
            EventSubType = "HEALTH_TASK_PROGRESS";
        }
    }
}
