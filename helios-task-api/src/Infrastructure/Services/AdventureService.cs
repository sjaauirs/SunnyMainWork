using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.ClearScript.JavaScript;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class AdventureService : IAdventureService
    {
        private readonly IAdventureRepo _adventureRepo;
        private readonly ITenantAdventureRepo _tenantAdventureRepo;
        private readonly ILogger<AdventureService> _logger;
        private readonly IMapper _mapper;
        private const string className = nameof(AdventureService);

        public AdventureService(ILogger<AdventureService> logger,IAdventureRepo adventureRepo,IMapper mapper,
            ITenantAdventureRepo tenantAdventureRepo)
        {
            _adventureRepo = adventureRepo;
            _logger = logger;
            _mapper = mapper;
            _tenantAdventureRepo = tenantAdventureRepo;
        }
        /// <summary>
        /// Retrieves all adventures associated with a given tenant.
        /// </summary>
        /// <param name="getAdventureRequestDto">
        /// The request DTO containing the TenantCode to fetch adventures for.
        /// </param>
        /// <returns>
        /// A <see cref="GetAdventureResponseDto"/> containing a list of adventures if found.
        /// Returns a response with an error code if no adventures are available or if an exception occurs.
        /// </returns>
        public async Task<GetAdventureResponseDto> GetAllAdventures(GetAdventureRequestDto getAdventureRequestDto)
        {
            const string methodName = nameof(GetAllAdventures);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing with TenantCode:{TenantCode}",
                    className, methodName,getAdventureRequestDto.TenantCode);

                var adventuresModels = await _adventureRepo.GetAllAdventures(getAdventureRequestDto.TenantCode);

                if (adventuresModels == null || !adventuresModels.Any())
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Adventures not found with TenantCode:{TenantCode}",
                    className, methodName, getAdventureRequestDto.TenantCode);

                    return new GetAdventureResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound
                    };
                }

                var adventures = _mapper.Map<IList<AdventureDto>>(adventuresModels);

                _logger.LogInformation("{ClassName}.{MethodName}: Sucessfully processed with TenantCode:{TenantCode}",
                    className, methodName, getAdventureRequestDto.TenantCode);

                return new GetAdventureResponseDto()
                {
                    Adventures = adventures
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occured with TenantCode:{TenantCode},ErrorCode:{ErrorCode},ERROR:{Error}",
                    className, methodName, getAdventureRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export adventures associated with a given tenant.
        /// </summary>
        /// <param name="exportAdventureRequestDto"></param>
        /// <returns></returns>
        public async Task<ExportAdventureResponseDto> ExportTenantAdventures(ExportAdventureRequestDto exportAdventureRequestDto)
        {
            const string methodName = nameof(ExportTenantAdventures);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing with TenantCode:{TenantCode}",
                    className, methodName, exportAdventureRequestDto.TenantCode);

                var tenantAdventures = await _adventureRepo.GetTenantAdventures(exportAdventureRequestDto.TenantCode);

                return tenantAdventures;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occured with TenantCode:{TenantCode},ErrorCode:{ErrorCode},ERROR:{Error}",
                    className, methodName, exportAdventureRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        public async Task<BaseResponseDto> ImportTenantAdventures(ImportAdventureRequestDto importAdventureRequestDto)
        {
            const string methodName = nameof(ImportTenantAdventures);
            int count = 0;

            // Create a dictionary of AdventureId to AdventureDto for lookup
            var adventureIdToDtoMap = importAdventureRequestDto.Adventures
                .ToDictionary(a => a.AdventureId, a => a);

            foreach (var tenantAdv in importAdventureRequestDto.TenantAdventures)
            {
                try
                {
                    if (!adventureIdToDtoMap.TryGetValue(tenantAdv.AdventureId, out var adventureDto))
                    {
                        _logger.LogError("{className}.{methodName}: Adventure not found for AdventureId:{id}",
                            className, methodName, tenantAdv.AdventureId);
                        continue;
                    }
                    await ImportAdventureAndTenantAdventureAsync(tenantAdv, adventureDto, importAdventureRequestDto.TenantCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{className}.{methodName}: Error importing TenantAdventureCode:{code}",
                        className, methodName, tenantAdv.TenantAdventureCode);
                    ++count;
                }
            }

            return new BaseResponseDto
            {
                ErrorCode = count > 0 ? StatusCodes.Status206PartialContent : null,
                ErrorMessage = count > 0 ? "Some Records encountered Error" : null
            };
        }

        private async System.Threading.Tasks.Task ImportAdventureAndTenantAdventureAsync(TenantAdventureDto tenantAdv, 
            AdventureDto adventureDto, string requestTenantCode)
        {
            const string methodName = nameof(ImportAdventureAndTenantAdventureAsync);

            AdventureModel adventure = await ValidateAdventure(adventureDto);

            // Set the correct (existing or newly created) AdventureId in TenantAdventure
            tenantAdv.AdventureId = adventure.AdventureId;

            // Check if TenantAdventure exists
            var tenantAdvList = await _tenantAdventureRepo.FindAsync(x => x.TenantCode == requestTenantCode
                    && x.AdventureId == adventure.AdventureId && x.DeleteNbr == 0);
            var tenantAdventure = tenantAdvList.OrderByDescending(x => x.TenantAdventureId).FirstOrDefault();

            if (tenantAdventure != null)
            {
                tenantAdv.TenantAdventureId = tenantAdventure.TenantAdventureId;
                _mapper.Map(tenantAdv, tenantAdventure);
                tenantAdventure.TenantCode = requestTenantCode;
                tenantAdventure.UpdateTs = DateTime.UtcNow;
                tenantAdventure.UpdateUser = Constant.ImportUser;
                await _tenantAdventureRepo.UpdateAsync(tenantAdventure);
            }
            else
            {
                var newTenantAdventure = _mapper.Map<TenantAdventureModel>(tenantAdv);
                var guid = Guid.NewGuid();
                var newTenantAdventureCode = $"tad-{guid:N}";
                newTenantAdventure.TenantAdventureCode = newTenantAdventureCode;
                newTenantAdventure.TenantCode = requestTenantCode;
                newTenantAdventure.CreateUser = Constant.ImportUser;
                newTenantAdventure.CreateTs = DateTime.UtcNow;
                newTenantAdventure.DeleteNbr = 0;
                await _tenantAdventureRepo.CreateAsync(newTenantAdventure);
            }

            _logger.LogInformation("{className}.{methodName}: Successfully imported TenantAdventureCode:{code} with AdventureCode:{advCode}",
                className, methodName, tenantAdv.TenantAdventureCode, adventure.AdventureCode);
        }

        private async Task<AdventureModel> ValidateAdventure(AdventureDto advDto)
        {
            // Find existing Adventure by AdventureCode
            var adventureList = await _adventureRepo.FindAsync(x => x.CmsComponentCode == advDto.CmsComponentCode && x.DeleteNbr == 0);
            var adventure = adventureList.OrderByDescending(x => x.AdventureId).FirstOrDefault();
            if (adventure != null)
            {
                advDto.AdventureId = adventure.AdventureId;
                _mapper.Map(advDto, adventure);
                adventure.UpdateTs = DateTime.UtcNow;
                adventure.UpdateUser = Constant.ImportUser;
                await _adventureRepo.UpdateAsync(adventure);
            }
            else
            {
                adventure = _mapper.Map<AdventureModel>(advDto);
                var guid = Guid.NewGuid();
                var newAdventureCode = $"adv-{guid:N}";
                adventure.AdventureCode = newAdventureCode;
                adventure.CreateUser = Constant.ImportUser;
                adventure.CreateTs = DateTime.UtcNow;
                adventure.DeleteNbr = 0;
                await _adventureRepo.CreateAsync(adventure);
            }

            return adventure;
        }
    }
}
