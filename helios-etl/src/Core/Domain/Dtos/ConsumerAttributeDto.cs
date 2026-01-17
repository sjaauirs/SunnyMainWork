using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ConsumerAttributeDto
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("pldFileData")]
        public string? PldFileData { get; set; }
        [JsonPropertyName("benefitsCardOptions")]
        public BenefitsCardOption? BenefitsCardOptions { get; set; }
    }

    public class BenefitsCardOption
    {
        [JsonPropertyName("cardCreateOptions")]
        public CardCreateOption? CardCreateOptions { get; set; }

    }

    public class CardCreateOption
    {
        [JsonPropertyName("deliveryMethod")]
        public int DeliveryMethod { get; set; }
    }
}
