using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class PostTaskProgressUpdateRequestDto
    {
        public string? ConsumerCode { get; set; }
        public long TaskId { get; set; }
        public string? ProgressDetail { get; set; }
    }
}
