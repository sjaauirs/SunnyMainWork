using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;
using System.Text.Json;

namespace Sunny.Benefits.Bff.UnitTest.Services
{
    public class FlowStepProcessorTests
    {
        private readonly Mock<ILogger<FlowStepProcessor>> _logger = new();
        private readonly Mock<IDynamicQueryProcessor> _dynamicQueryProcessor = new();
        private readonly Mock<IConsumerService> _consumerService = new();
        private readonly Mock<IMapper> _mapper = new();

        private FlowStepProcessor CreateProcessor() =>
            new(_logger.Object, _dynamicQueryProcessor.Object, _consumerService.Object, _mapper.Object);

        private FlowStepDto CreateStep(long id, long? nextId, string? configJson = null, int idx = 0)
        {
            return new FlowStepDto
            {
                StepId = id,
                OnSuccessStepId = nextId,
                StepConfigJson = configJson,
                StepIdx = idx,
                ComponentName = $"Step{id}"
            };
        }

        [Fact]
        public async Task ProcessSteps_ReturnsEmpty_WhenInputIsNullOrEmpty()
        {
            var processor = CreateProcessor();
            var result1 = await processor.ProcessSteps(null, "C123");
            Assert.Empty(result1);

            var result2 = await processor.ProcessSteps(new List<FlowStepDto>(), "C123");
            Assert.Empty(result2);
        }

        [Fact]
        public async Task ProcessSteps_ReturnsOriginalSteps_WhenNoSuppressionCondition()
        {
            var processor = CreateProcessor();
            var steps = new List<FlowStepDto>
        {
            CreateStep(1, 2, null, 0),
            CreateStep(2, null, null, 1)
        };

            // No suppression condition
            _consumerService.Setup(x => x.GetConsumer(It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseDto { Consumer = new ConsumerDto() });
            _mapper.Setup(x => x.Map<ConsumerFilter>(It.IsAny<ConsumerDto>()))
                .Returns(new ConsumerFilter());
            _dynamicQueryProcessor.Setup(x => x.EvaluateConditionsForAllContexts(It.IsAny<Dictionary<string, List<Condition>>>(), It.IsAny<DynamicFilterContext>()))
                .Returns(false);

            var result = await processor.ProcessSteps(steps, "C123");
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].StepId);
            Assert.Equal(2, result[1].StepId);
        }

        [Fact]
        public async Task ProcessSteps_SuppressesStep_WhenConditionMet()
        {
            var processor = CreateProcessor();

            // Step 1 points to Step 2, Step 2 is suppressible
            var config = new StepConfigDto
            {
                SupressionCondition = new Dictionary<string, List<Condition>>
            {
                { "consumer", new List<Condition> { new Condition { AttributeName = "ConsumerCode", Operator = "=", DataType = "string", AttributeValue = "C123" } } }
            }
            };
            var configJson = JsonSerializer.Serialize(config);

            var steps = new List<FlowStepDto>
        {
            CreateStep(1, 2, null, 0),
            CreateStep(2, null, configJson, 1)
        };

            _consumerService.Setup(x => x.GetConsumer(It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseDto { Consumer = new ConsumerDto { ConsumerCode = "C123" } });
            _mapper.Setup(x => x.Map<ConsumerFilter>(It.IsAny<ConsumerDto>()))
                .Returns(new ConsumerFilter { ConsumerCode = "C123" });
            _dynamicQueryProcessor.Setup(x => x.EvaluateConditionsForAllContexts(It.IsAny<Dictionary<string, List<Condition>>>(), It.IsAny<DynamicFilterContext>()))
                .Returns(true);

            var result = await processor.ProcessSteps(steps, "C123");
            // Step 2 should be suppressed, only step 1 remains
            Assert.Single(result);
            Assert.Equal(1, result[0].StepId);
            Assert.Null(result[0].OnSuccessStepId);
        }

        [Fact]
        public async Task ProcessSteps_RetainsStep_WhenSuppressionConditionNotMet()
        {
            var processor = CreateProcessor();

            var config = new StepConfigDto
            {
                SupressionCondition = new Dictionary<string, List<Condition>>
            {
                { "consumer", new List<Condition> { new Condition { AttributeName = "ConsumerCode", Operator = "=", DataType = "string", AttributeValue = "C999" } } }
            }
            };
            var configJson = JsonSerializer.Serialize(config);

            var steps = new List<FlowStepDto>
        {
            CreateStep(1, 2, null, 0),
            CreateStep(2, null, configJson, 1)
        };

            _consumerService.Setup(x => x.GetConsumer(It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseDto { Consumer = new ConsumerDto { ConsumerCode = "C123" } });
            _mapper.Setup(x => x.Map<ConsumerFilter>(It.IsAny<ConsumerDto>()))
                .Returns(new ConsumerFilter { ConsumerCode = "C123" });
            _dynamicQueryProcessor.Setup(x => x.EvaluateConditionsForAllContexts(It.IsAny<Dictionary<string, List<Condition>>>(), It.IsAny<DynamicFilterContext>()))
                .Returns(false);

            var result = await processor.ProcessSteps(steps, "C123");
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].StepId);
            Assert.Equal(2, result[1].StepId);
        }

        [Fact]
        public async Task ProcessSteps_ReturnsOriginalSteps_OnException()
        {
            var processor = CreateProcessor();
            var steps = new List<FlowStepDto>
        {
            CreateStep(1, null, null, 0)
        };

            _consumerService.Setup(x => x.GetConsumer(It.IsAny<GetConsumerRequestDto>()))
                .ThrowsAsync(new System.Exception("Test exception"));

            var result = await processor.ProcessSteps(steps, "C123");
            Assert.Single(result);
            Assert.Equal(1, result[0].StepId);
        }
    }
}
