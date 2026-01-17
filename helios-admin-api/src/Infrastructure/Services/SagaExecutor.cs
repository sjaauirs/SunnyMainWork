using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class SagaExecutor
    {
        private readonly ILogger<SagaExecutor> _logger;
        private readonly List<ISagaStep> _steps = new();

        public SagaExecutor(ILogger<SagaExecutor> logger)
        {
            _logger = logger;
        }

        public void AddStep(ISagaStep step) => _steps.Add(step);

        public async Task<BaseResponseDto> ExecuteAsync()
        {
            var executedSteps = new Stack<ISagaStep>();

            try
            {
                _logger.LogInformation("Starting Saga execution with {StepCount} steps", _steps.Count);

                foreach (var step in _steps)
                {
                    _logger.LogInformation("Executing step: {Step}", step.GetType().Name);

                    var response = await step.ExecuteAsync();

                    if (response == null || response.ErrorCode != null)
                    {
                        _logger.LogError("Step failed: {Step} with ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}", step.GetType().Name, response?.ErrorCode, response?.ErrorMessage);
                        throw new Exception($"Step failed: {step.GetType().Name}");
                    }

                    // if no Error , push to executed stack
                    _logger.LogInformation("Step executed successfully: {Step}", step.GetType().Name);
                    executedSteps.Push(step);

                }

                _logger.LogInformation("Saga completed successfully");
                return new BaseResponseDto { ErrorCode = 200, ErrorMessage = "Saga completed successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga failed. Starting compensation...");
                while (executedSteps.Count > 0)
                {
                    var step = executedSteps.Pop();
                    try
                    {
                        await step.CompensateAsync();
                        _logger.LogWarning("Compensation successful for {Step}", step.GetType().Name);
                    }
                    catch (Exception compEx)
                    {
                        _logger.LogError(compEx, "Compensation failed for {Step}", step.GetType().Name);
                    }
                }

                return new BaseResponseDto
                {
                    ErrorCode = 500,
                    ErrorMessage = $"Saga failed. Compensation executed. Details: {ex.Message}"
                };
            }
        }
    }



}
