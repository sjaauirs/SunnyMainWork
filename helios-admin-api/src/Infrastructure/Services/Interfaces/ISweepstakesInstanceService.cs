using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ISweepstakesInstanceService
    {
        Task<SweepstakesInstanceResponseDto> CreateSweepstakesInstance(SweepstakesInstanceRequestDto requestDto);
        Task<SweepstakesInstanceDto> GetSweepstakesInstance(string sweepstakesInstanceCode);


    }
}
