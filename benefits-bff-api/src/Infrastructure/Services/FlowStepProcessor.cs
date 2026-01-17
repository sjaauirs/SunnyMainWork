using AutoMapper;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Text.Json;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    /// <summary>
    /// Handles processing of flow steps, applying suppression logic based on dynamic filter conditions.
    /// </summary>
    public class FlowStepProcessor : IFlowStepProcessor
    {
        private readonly ILogger<FlowStepProcessor> _logger;
        private readonly IConsumerService _consumerService;
        private readonly IMapper _mapper;
        private readonly IDynamicQueryProcessor _dynamicQueryProcessor;

        public FlowStepProcessor(ILogger<FlowStepProcessor> logger, IDynamicQueryProcessor dynamicQueryProcessor,
            IConsumerService consumerService, IMapper mapper)
        {
            _logger = logger;
            _dynamicQueryProcessor = dynamicQueryProcessor;
            _consumerService = consumerService;
            _mapper = mapper;
        }


        /// <summary>
        /// Processes a list of flow steps, removing suppressed steps and reconnecting the step chain.
        /// </summary>
        public async Task<List<FlowStepDto>> ProcessSteps(List<FlowStepDto>? flowSteps, string consumerCode)
        {
            try
            {
                if (flowSteps == null || flowSteps.Count == 0)
                {
                    _logger.LogWarning("No steps found to process.");
                    return new List<FlowStepDto>();
                }

                if (!HasSuppressionCondition(flowSteps))
                {
                    _logger.LogInformation("No suppression conditions found in any steps. Returning original step list.");
                    return flowSteps;
                }

                _logger.LogInformation("Starting step processing. Total steps: {Count}", flowSteps.Count);

                var consumerResponse = await _consumerService.GetConsumer(new GetConsumerRequestDto { ConsumerCode = consumerCode });

                var consumer = _mapper.Map<ConsumerFilter>(consumerResponse.Consumer);

                var dynamicFilterContext = new DynamicFilterContext
                {
                    Consumer = consumer,
                };

                var stepMap = flowSteps.ToDictionary(s => s.StepId, s => s);

                // Identify the start node (no other step points to it)
                var startStepId = flowSteps
                    .FirstOrDefault(s => !flowSteps.Any(x => x.OnSuccessStepId == s.StepId))
                    ?.StepId;

                _logger.LogInformation("Identified start step: {StartStepId}", startStepId);

                long? currentStepId = startStepId;

                // Traverse and process the step chain
                while (currentStepId != null)
                {
                    if (!stepMap.TryGetValue(currentStepId.Value, out var currentStep))
                    {
                        _logger.LogWarning("StepId {StepId} not found in step map. Terminating chain.", currentStepId);
                        break;
                    }

                    _logger.LogInformation("Evaluating step {StepId} ({StepName})", currentStep.StepId, currentStep.ComponentName);

                    if (ShouldSuppressStep(currentStep, dynamicFilterContext))
                    {
                        _logger.LogInformation("Suppressing step {StepId} ({StepName})", currentStep.StepId, currentStep.ComponentName);

                        var nextStepId = currentStep.OnSuccessStepId;

                        // Reconnect previous step to next
                        var previous = stepMap.Values.FirstOrDefault(s => s.OnSuccessStepId == currentStep.StepId);
                        if (previous != null)
                        {
                            previous.OnSuccessStepId = nextStepId;
                            _logger.LogInformation("Reconnected step {PrevId} -> {NextId}", previous.StepId, nextStepId);
                        }

                        stepMap.Remove(currentStep.StepId);
                        currentStepId = nextStepId; // Continue with next step
                    }
                    else
                    {
                        _logger.LogInformation("Retaining step {StepId} ({StepName})", currentStep.StepId, currentStep.ComponentName);
                        currentStepId = currentStep.OnSuccessStepId;
                    }
                }

                _logger.LogInformation("Completed step processing. Retained steps: {Count}", stepMap.Count);
                return stepMap.Values.OrderBy(s => s.StepIdx).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing steps: {Message}", ex.Message);
                return flowSteps ?? new List<FlowStepDto>();
            }
        }

        /// <summary>
        /// Determines if a given step should be suppressed based on its configuration and filter context.
        /// </summary>
        private bool ShouldSuppressStep(FlowStepDto step, DynamicFilterContext filterContext)
        {
            if (string.IsNullOrWhiteSpace(step.StepConfigJson))
                return false;

            StepConfigDto? config;
            try
            {
                config = JsonSerializer.Deserialize<StepConfigDto>(step.StepConfigJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize StepConfig for StepId: {StepId}", step.StepId);
                return false;
            }

            if (config?.SupressionCondition == null || config.SupressionCondition.Count == 0)
                return false;

            return _dynamicQueryProcessor.EvaluateConditionsForAllContexts(config.SupressionCondition, filterContext);
        }

        /// <summary>
        /// Checks if any flow step has suppression conditions defined.
        /// </summary>
        /// <param name="flowSteps"></param>
        /// <returns></returns>
        private bool HasSuppressionCondition(List<FlowStepDto> flowSteps)
        {
            if (flowSteps == null || flowSteps.Count == 0)
            {
                _logger.LogWarning("No flow steps provided for suppression condition check.");
                return false;
            }

            foreach (var step in flowSteps)
            {
                if (string.IsNullOrWhiteSpace(step.StepConfigJson))
                    continue;

                try
                {
                    var config = JsonSerializer.Deserialize<StepConfigDto>(step.StepConfigJson);
                    if (config?.SupressionCondition == null || config.SupressionCondition.Count == 0)
                        continue;

                    if (config.SupressionCondition.Any(kvp => kvp.Value != null && kvp.Value.Count > 0))
                        return true;
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse StepConfig JSON for StepId: {StepId}", step.StepId);
                    // Invalid JSON, ignore this step
                    continue;
                }
            }

            return false;
        }
    }
}
