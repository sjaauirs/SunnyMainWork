using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    [Serializable]
    public class ConsumerTaskEventDto: ISerializable
    {
        public  required string TaskRewardCode { get; set; }

        public string? ProgressDetail { get; set; }

        public required string Status { get; set; }

        public ConsumerTaskEventDto() { }

      

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize the properties to the SerializationInfo object
            info.AddValue(nameof(TaskRewardCode), TaskRewardCode);
            info.AddValue(nameof(ProgressDetail), ProgressDetail);
            info.AddValue(nameof(Status), Status);
        }
    }
}
