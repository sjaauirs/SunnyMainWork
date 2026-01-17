using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class SpinWheelProgressDto
    {
        public class ItemDefinition
        {
            public string? itemText { get; set; }
            public string? lowProbability { get; set; }
            public string? highProbability { get; set; }
        }

        public class SpinWheelProgressDtos
        {
            public SpinwheelProgress? spinwheelProgress { get; set; }
        }

        public class SpinwheelConfig
        {
            public int probability { get; set; }
            public List<ItemDefinition>? itemDefinition { get; set; }
            public string? itemTextSuffix { get; set; }
        }

        public class SpinwheelProgress
        {
            public int finalSlotIndex { get; set; }
            public SpinwheelConfig? spinwheelConfig { get; set; }
        }
    }
}
