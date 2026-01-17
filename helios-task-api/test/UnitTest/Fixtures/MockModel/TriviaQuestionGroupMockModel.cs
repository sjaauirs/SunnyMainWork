using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TriviaQuestionGroupMockModel : TriviaQuestionGroupModel
    {
        public TriviaQuestionGroupMockModel()
        {
            TriviaQuestionGroupId = 2;
            TriviaId = 1;
            TriviaQuestionId = 2;
            SequenceNbr = 3;
        }
    }
}
