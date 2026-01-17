using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ExtendedErrorResponseDto : BaseResponseDto
    {
        public List<string> ExtendedErrors { get; set; } = new List<string>();
    }
}
