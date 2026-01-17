namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class ConsumersAndPersonsModels
    {
        public ConsumersAndPersonsModels(PersonModel PersonModel, ConsumerModel ConsumerModel)
        {
            this.PersonModel = PersonModel;
            this.ConsumerModel = ConsumerModel;
        }
        public PersonModel PersonModel { get; set; }
        public ConsumerModel ConsumerModel { get; set; }
    }
}