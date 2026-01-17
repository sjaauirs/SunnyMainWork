using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface IAdventureService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="getAdventureRequestDto"></param>
        /// <returns></returns>
        Task<GetAdventureResponseDto> GetAllAdventures(GetAdventureRequestDto getAdventureRequestDto);

        Task<ExportAdventureResponseDto> ExportTenantAdventures(ExportAdventureRequestDto exportAdventureRequestDto);
        Task<BaseResponseDto> ImportTenantAdventures(ImportAdventureRequestDto importAdventureRequestDto);

    }
}
