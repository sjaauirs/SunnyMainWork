using AutoMapper;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class ConsumerActivityService : IConsumerActivityService
    {
        private readonly IConsumerActivityRepo _consumerActivityRepo;
        private readonly ILogger<ConsumerActivityService> _logger;
        private readonly IMapper _mapper;
        private const string className = nameof(ConsumerActivityService);

        public ConsumerActivityService(ILogger<ConsumerActivityService> logger, IConsumerActivityRepo consumerActivityRepo, IMapper mapper)
        {
            _consumerActivityRepo = consumerActivityRepo;
            _mapper = mapper;
            _logger = logger;
        }
        /// <summary>
        /// Handles the creation of a consumer activity.
        /// </summary>
        /// <param name="consumerActivityRequestDto">
        /// The DTO containing details of the consumer activity, such as TenantCode, ConsumerCode, ActivitySource, ActivityType, and ActivityJson.
        /// </param>
        /// <returns>
        /// A <see cref="ConsumerActivityResponseDto"/> object indicating the success of the operation.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when an error occurs during the creation of the consumer activity.
        /// </exception>

        public async Task<ConsumerActivityResponseDto> CreateConsumerActivityAsync(ConsumerActivityRequestDto consumerActivityRequestDto)
        {
            const string methodName = nameof(CreateConsumerActivityAsync);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing create consumer activity for TenantCode:{Code},ConsumerCode:{Consumer}",
                    className, methodName, consumerActivityRequestDto.TenantCode, consumerActivityRequestDto.ConsumerCode);

                var consumerActivityModel = new ConsumerActivityModel()
                {
                    TenantCode = consumerActivityRequestDto.TenantCode,
                    ConsumerCode = consumerActivityRequestDto.ConsumerCode,
                    ConsumerActivityCode = "cat-" + GetUniqueCode(),
                    ActivitySource = consumerActivityRequestDto.ActivitySource.ToUpper(),
                    ActivityType = consumerActivityRequestDto.ActivityType.ToUpper(),
                    ActivityDetailJson = consumerActivityRequestDto.ActivityJson,
                    CreateTs = DateTime.UtcNow,
                    CreateUser = Constants.CreateUser
                };

                consumerActivityModel = await _consumerActivityRepo.CreateAsync(consumerActivityModel);

                _logger.LogInformation("{ClassName}.{MethodName}: Successfully created consumer activity for TenantCode:{Code},ConsumerCode:{Consumer}",
                    className, methodName, consumerActivityRequestDto.TenantCode, consumerActivityRequestDto.ConsumerCode);

                return new ConsumerActivityResponseDto()
                {
                    ConsumerActivityDto = _mapper.Map<ConsumerActivityDto>(consumerActivityModel)
                };

            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GetUniqueCode()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
