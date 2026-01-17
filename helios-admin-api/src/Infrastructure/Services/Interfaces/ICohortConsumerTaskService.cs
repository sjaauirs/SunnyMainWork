using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ICohortConsumerTaskService
    {
        Task<bool> TaskCompletionPrePostScriptCheck(FindConsumerTasksByIdResponseDto consumerTaskUpdateRequestDto, 
            ConsumerDto consumer, string scriptType, PersonDto? person = null);

    }
}
