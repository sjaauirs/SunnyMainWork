using AutoMapper;
using log4net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class ScriptService : IScriptService
    {
        private readonly ILogger<IScriptService> _scriptServiceLogger;
        private readonly IScriptRepo _scriptRepo;
        private readonly IMapper _mapper;

        const string className = nameof(ScriptService);

        public ScriptService(ILogger<IScriptService> scriptServiceLogger, IScriptRepo scriptRepo, IMapper mapper)
        {
            _scriptServiceLogger = scriptServiceLogger;
            _scriptRepo = scriptRepo;
            _mapper = mapper;
        }
        public async Task<ScriptResponseDto> GetScript()
        {
            const string methodName = nameof(GetScript);
            try
            {
                var script = await _scriptRepo.FindAllAsync();
                if (script.Count <= 0)
                {
                    _scriptServiceLogger.LogError("{className}.{methodName}: No script found", className, methodName);
                    return new ScriptResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "No script found" };
                }
                var response = _mapper.Map<List<ScriptDto>>(script);
                var scriptResponseDto = new ScriptResponseDto() { scripts = response };

                _scriptServiceLogger.LogInformation("{className}.{methodName}: Retrieved script Details Successfully", className, methodName);


                return scriptResponseDto;
            }
            catch (Exception ex)
            {
                _scriptServiceLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }
    }
}
