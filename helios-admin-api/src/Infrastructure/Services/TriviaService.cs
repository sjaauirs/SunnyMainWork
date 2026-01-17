using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TriviaService : ITriviaService
    {
        public readonly ILogger<TriviaService> _logger;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TriviaService);

        public TriviaService(ILogger<TriviaService> logger, ITaskClient taskClient)
        {
            _logger = logger;
            _taskClient = taskClient;
        }
        public async Task<BaseResponseDto> CreateTrivia(TriviaRequestDto triviaDto)
        {
            const string methodName = nameof(CreateTrivia);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create Trivia process started for code: {TaskCode}", className, methodName, triviaDto.trivia.TriviaCode);

                var taskResponse = await _taskClient.Post<BaseResponseDto>(Constant.CreateTriviaRequest, triviaDto);
                if (taskResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating Trivia, Trivia: {Trivia}, ErrorCode: {ErrorCode}", className, methodName, triviaDto.trivia.TriviaCode, taskResponse.ErrorCode);
                    return taskResponse;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Trivia created successfully, TriviaCode: {TaskCode}", className, methodName, triviaDto.trivia.TriviaCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating Trivia. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace},", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Gets all trivia.
        /// </summary>
        /// <returns></returns>
        public async Task<TriviaResponseDto> GetAllTrivia()
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _taskClient.Get<TriviaResponseDto>(Constant.GetAllTriviaAPIUrl, parameters);
        }

    }
}
