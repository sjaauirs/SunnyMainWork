using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class PersonService : IPersonService
    {
        private readonly ILogger<PersonService> _logger;
        private readonly IUserClient _userClient;
        private readonly IMapper _mapper;
        private const string className = nameof(PersonService);
        public PersonService(ILogger<PersonService> logger, IUserClient userClient, IMapper mapper)
        {
            _logger = logger;
            _userClient = userClient;
            _mapper = mapper;
        }

        public async Task<PersonResponseDto> UpdatePersonData(UpdatePersonRequestDto updatePersonRequestDto)
        {
            const string methodName = nameof(UpdatePersonData);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing UpdatePersonData for ConsumerCode : {ConsumerCode}", className, methodName, updatePersonRequestDto.ConsumerCode);
            try
            {
                var response = await _userClient.Put<PersonResponseDto>("person/update-person", updatePersonRequestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while updating person data. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, updatePersonRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return response;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Person data updated successfully for ConsumerCode: {ConsumerCode}", className, methodName, updatePersonRequestDto.ConsumerCode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while updating person data. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new PersonResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }
    }
}
