using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ExportDto
    {
        public required string ExportVersion { get; set; }
        public DateTime ExportTs { get; set; }
        public required string ExportType { get; set; }
        public required object Data { get; set; }
    }

}
