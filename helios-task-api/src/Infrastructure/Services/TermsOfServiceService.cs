using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class TermsOfServiceService : ITermsOfServiceService
    {
        private readonly ITermsOfServiceRepo _termsOfServiceRepo;
        private readonly ILogger<TermsOfServiceService> _termsOfServiceLogger;
        private readonly IMapper _mapper;
        public const string className = nameof(TermsOfServiceService);

        public TermsOfServiceService(ITermsOfServiceRepo termsOfServiceRepo, ILogger<TermsOfServiceService> termsOfServiceLogger, IMapper mapper)
        {
            _termsOfServiceRepo = termsOfServiceRepo;
            _termsOfServiceLogger = termsOfServiceLogger;
            _mapper = mapper;
        }

        public async Task<BaseResponseDto> CreateTermsOfService(CreateTermsOfServiceRequestDto createTermsOfServiceRequestDto)
        {
            const string methodName = nameof(CreateTermsOfService);
            try
            {
                _termsOfServiceLogger.LogInformation("{ClassName}.{MethodName}: Fetching TermsOfService for Terms Of Service Id:{Id}", className, methodName, createTermsOfServiceRequestDto.TermsOfServiceId);
                var termsOfService = await _termsOfServiceRepo.FindOneAsync(x => x.TermsOfServiceId == createTermsOfServiceRequestDto.TermsOfServiceId && x.DeleteNbr == 0);
                if (termsOfService != null)
                {
                    return new BaseResponseDto() { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = $"TermsOfService are already Existed with Id: {createTermsOfServiceRequestDto.TermsOfServiceId}" };
                }
                var termsOfServiceModel = _mapper.Map<TermsOfServiceModel>(createTermsOfServiceRequestDto);
                termsOfServiceModel.TermsOfServiceId = 0;
                termsOfServiceModel.DeleteNbr = 0;
                termsOfServiceModel.CreateTs = DateTime.UtcNow;
                await _termsOfServiceRepo.CreateAsync(termsOfServiceModel);
                _termsOfServiceLogger.LogInformation("{ClassName}.{MethodName}: TermsOfService created Successfully. TermsOfServiceId:{Id}", className, methodName, createTermsOfServiceRequestDto.TermsOfServiceId);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _termsOfServiceLogger.LogError(ex, "{ClassName}:{MethodName}: Error Creating TermsOfService, for Terms Of Service Id:{Id}", className, methodName, createTermsOfServiceRequestDto.TermsOfServiceId);
                throw;
            }
        }
    }
}
