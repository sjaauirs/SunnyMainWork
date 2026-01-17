using AutoMapper;
using FirebaseAdmin;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class ConsumerHistoryService : BaseService, IConsumerHistoryService
    {
        private readonly ILogger<ConsumerHistoryService> _logger;
        private readonly IMapper _mapper;
        private readonly IConsumerHistoryRepo _consumerHistoryRepo;
        private const string className = nameof(ConsumerHistoryService);

        public ConsumerHistoryService(
            ILogger<ConsumerHistoryService> consumerHistoryLogger,
            IMapper mapper,
            IConsumerHistoryRepo consumerHistoryRepo
            )
        {
            _logger = consumerHistoryLogger;
            _mapper = mapper;
            _consumerHistoryRepo = consumerHistoryRepo;
        }

        /// <summary>
        /// CreateConsumers
        /// </summary>
        /// <param name="consumersCreateRequestDto"></param>
        /// <returns></returns>

        public async Task<BaseResponseDto> InsertConsumerHistory(IList<ConsumerDto> consumers)
        {
            const string methodName = nameof(InsertConsumerHistory);
            var response = new BaseResponseDto();

            if (consumers == null || consumers.Count == 0)
            {
                _logger.LogWarning("{className}{Method} called with no consumers to process.",className, methodName);
                response.ErrorMessage = "No consumers to insert into history.";
                return response;
            }

            _logger.LogInformation("{className}{Method} started. Processing {Count} consumers.", className, methodName, consumers.Count);

            try
            {
                foreach (var consumer in consumers)
                {
                    var consumerHistoryModel = _mapper.Map<ConsumerHistoryModel>(consumer);
                    consumerHistoryModel.CreateUser = consumerHistoryModel.UpdateUser ?? consumerHistoryModel.CreateUser;
                    consumerHistoryModel.CreateTs = DateTime.UtcNow;

                    _logger.LogDebug("{Method} - Inserting history for ConsumerCode: {ConsumerCode}", methodName, consumer.ConsumerCode);
                     await _consumerHistoryRepo.CreateAsync(consumerHistoryModel);

                }
                _logger.LogInformation("{Method} completed successfully.", methodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method} failed with error: {ErrorMessage}", methodName, ex.Message);
                response.ErrorCode = StatusCodes.Status500InternalServerError;
                response.ErrorMessage = ex.Message;
            }

            return response;
        }

    }
}

