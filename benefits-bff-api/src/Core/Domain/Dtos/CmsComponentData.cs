using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class CmsComponentData
    {
        [JsonProperty("data")]
        public CmsComponentDetails? Data { get; set; }
    }

    public class CmsComponentDetails
    {
        [JsonProperty("details")]
        public ComponentDetails? Details { get; set; }
    }

    public class ComponentDetails
    {
        [JsonProperty("headerText")]
        public string? HeaderText { get; set; }

        [JsonProperty("sectionCollectionName")]
        public string? SectionCollectionName { get; set; }

        [JsonProperty("descriptionText")]
        public string? DescriptionText { get; set; }

        [JsonProperty("htmlContentUrl")]
        public string? HtmlContentUrl { get; set; }

    }
}
