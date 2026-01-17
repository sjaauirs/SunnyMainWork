using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ExportTenantResponseDto : BaseResponseDto
    {
        public MemoryStream? ExportFileData { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
    }
}
