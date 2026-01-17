using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TriviaMockModel : TriviaModel
    {
        public TriviaMockModel()
        {
            TriviaId = 1;
            TriviaCode = "trq-6b50df09e8474fd3a618d432bc2b3ff9";
            TaskRewardId = 1;
            CtaTaskExternalCode = "trw-1a949151b44f463cbc048d732305c483";
            ConfigJson = "{\n  \"answerText\": [\n    \"Loan terms\",\n    \"Your marriage\",\n    \"Golf handicap\"\n  ],\n  \"answerType\": \"SINGLE\",\n  \"layoutType\": \"BUTTON\",\n  \"questionText\": \"What does your credit score impact?\",\n  \"correctAnswer\": [\n    0\n  ]\n}";

        }
    }
}
