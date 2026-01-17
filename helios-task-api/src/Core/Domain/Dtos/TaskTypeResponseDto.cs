using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskTypeResponseDto :BaseResponseDto
    {
        public long TaskTypeId { get; set; }
        public string? TaskTypeCode { get; set; }
        public string? TaskTypeName { get; set; }
        public string? TaskTypeDescription { get; set; }
    }
}
