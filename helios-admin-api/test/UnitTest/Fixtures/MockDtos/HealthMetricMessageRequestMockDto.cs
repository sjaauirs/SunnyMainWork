using SunnyBenefits.Health.Core.Domains.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class HealthMetricMessageRequestMockDto : HealthMetricMessageRequestDto
    {
        public HealthMetricMessageRequestMockDto()
        {
            HealthMetricMessages = new List<HealthMetricMessageDataDto>
            {
                new HealthMetricMessageDataDto
                {
                    HealthMetricTypeCode = "hmt-ecada21e57154928a51f877bae38f22e",
                    ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    DataJson = "{\r\n  \"healthMetric\": {\r\n    \"steps\": \"1000\",\r\n    \"distance\": \"22\",\r\n    \"heartRate\": \"72\"\r\n  },\r\n  \"rawHealthMetric\": {\r\n    \"bmi\": \"23.46\",\r\n    \"steps\": \"1000\",\r\n    \"height\": \"5.91 feets\",\r\n    \"weight\": \"76.00 kg\",\r\n    \"distance\": \"2.19824000000814\",\r\n    \"workouts\": \"NOT_AVAILABLE\",\r\n    \"heartRate\": \"NOT_AVAILABLE\",\r\n    \"walkingSpeed\": \"1.1655297981650474\",\r\n    \"bloodPressure\": \"NOT_AVAILABLE\",\r\n    \"floorsClimbed\": \"2.00\",\r\n    \"restingEnergy\": \"1373.098\",\r\n    \"sleepDuration\": \"5.824145375556416\",\r\n    \"cyclingDistance\": \"0.00\",\r\n    \"activeEnergyBurned\": \"81.45700000000001\"\r\n  }\r\n}",
                    CaptureTs = DateTime.UtcNow,
                    OsMetricTs = DateTime.UtcNow
                }
            };
        }
    }
}
