using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TriviaQuestionMockModel : TriviaQuestionModel
    {
        public TriviaQuestionMockModel()
        {
            TriviaQuestionId = 1;
            TriviaQuestionCode = "trq-6b50df09e8474fd3a618d432bc2b3ff9";
            TriviaJson = "{\n  \"answerText\": " +
                "[\n    \"Loan terms\",\n    \"Your marriage\",\n " +
                "   \"Golf handicap\"\n  ],\n  \"answerType\":" +
                " \"SINGLE\",\n  \"layoutType\": \"BUTTON\",\n  \"questionText\":" +
                " \"What does your credit score impact?\",\n  \"correctAnswer\": [\n    0\n  ]\n}";
            QuestionExternalCode = "what_does_your_cred_scor_impa";
        }
    }
}
