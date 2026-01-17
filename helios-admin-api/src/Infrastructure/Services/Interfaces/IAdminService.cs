using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IAdminService
    {
        public ExportAdminResponseDto GetAdminScripts(string tenantCode);
        public Task<BaseResponseDto> CreateAdminScripts(ImportAdminRequestDto importAdminRequest,Dictionary<string,string> taskrewards=null);
    }
}
