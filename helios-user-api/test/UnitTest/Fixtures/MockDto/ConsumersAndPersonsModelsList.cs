using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public class ConsumersAndPersonsModelMockList
    {
        public static List<ConsumersAndPersonsModels> consumersAndPersonsModelsData()
        {
            var consumerModel1 = new ConsumerModel() { ConsumerId = 1 };
            var personModel1 = new PersonModel() { PersonId = 1 };

            var consumerModel2 = new ConsumerModel() { ConsumerId = 2 };
            var personModel2 = new PersonModel() { PersonId = 2 };
            return new List<ConsumersAndPersonsModels>()
            {
                new ConsumersAndPersonsModels(personModel1 , consumerModel1),
                new ConsumersAndPersonsModels(personModel2 , consumerModel2)
            };
        }
    }
}
