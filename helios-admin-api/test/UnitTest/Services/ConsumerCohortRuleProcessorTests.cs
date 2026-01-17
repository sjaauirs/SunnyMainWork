using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;
using TaskA = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class ConsumerCohortRuleProcessorTests
    {
        private readonly Mock<ILogger<IConsumerCohortRuleProcessor>> _logger;
        private readonly Mock<IConsumerPurseCohortAssignmentService> _consumerPurseCohortAssignmentService = new();

        public ConsumerCohortRuleProcessorTests()
        {
            _logger = new Mock<ILogger<IConsumerCohortRuleProcessor>>();
        }


        [Fact]
        public async TaskA EvaluateRule_NoRules_ReturnsFalse_AndLogsWarning()
        {
            var svc = new ConsumerCohortRuleProcessor(_logger.Object, _consumerPurseCohortAssignmentService.Object);
            var cohortRule = new CohortRuleArrayJson
            {
                RuleExpr = new List<string>(), // no rules
                SuccessExpr = "true"
            };

            var input = new CohortRuleInput
            {
                Person = new PersonDto { Gender = "MALE",  City = "Delhi" },
                Consumer = new ConsumerDto { PlanId = "2"}
            };

            var result = await svc.EvaluateRule("CohortA", cohortRule, input);

            Assert.False(result.RulesExecutionResult);
            Assert.Equal("CohortA", result.CohortName);
            _logger.VerifyLog(LogLevel.Warning, times: Times.Once());
        }

        [Fact]
        public async TaskA EvaluateRule_SinglePassingRule_ReturnsTrue()
        {
            var svc = new ConsumerCohortRuleProcessor(_logger.Object, _consumerPurseCohortAssignmentService.Object);
            var cohortRule = new CohortRuleArrayJson
            {
                RuleExpr = new List<string>
            {
                "Person.Gender == \"MALE\"" , "Consumer.PlanId = \"2\""
            },
                SuccessExpr = "true"
            };

            var input = new CohortRuleInput
            {
                Person = new PersonDto { Gender = "MALE", City = "Delhi" },
                Consumer = new ConsumerDto { PlanId = "2" }
            };

            var result = await svc.EvaluateRule("CohortA", cohortRule, input);

            Assert.True(result.RulesExecutionResult);
            Assert.Equal("CohortA", result.CohortName);
            _logger.VerifyLog(LogLevel.Information, times: Times.AtLeastOnce());
        }

        [Fact]
        public async TaskA EvaluateRule_MixOfPassAndFail_ReturnsTrue_WhenAnyPass()
        {
            var svc = new ConsumerCohortRuleProcessor(_logger.Object, _consumerPurseCohortAssignmentService.Object);
            var cohortRule = new CohortRuleArrayJson
            {
                RuleExpr = new List<string>
            {
                "Consumer.PlanId = \"2\" ", 
                "Person.Gender == \"FEMALE\""               // will fail
            },
                SuccessExpr = "true"
            };

            var input = new CohortRuleInput
            {
                Person = new PersonDto { Gender = "MALE", City = "Delhi" },
                Consumer = new ConsumerDto { PlanId = "2" }
            };

            var result = await svc.EvaluateRule("CohortB", cohortRule, input);

            Assert.True(result.RulesExecutionResult); // because at least one rule passed
            _logger.VerifyLog(LogLevel.Information, times: Times.AtLeastOnce());
        }

        [Fact]
        public async TaskA EvaluateRule_AllFail_ReturnsFalse()
        {
            var svc = new ConsumerCohortRuleProcessor(_logger.Object, _consumerPurseCohortAssignmentService.Object);
            var cohortRule = new CohortRuleArrayJson
            {
                RuleExpr = new List<string>
            {
                "Person.Age > 18",
                "Consumer.IsSmoker == true"
            },
                SuccessExpr = "true"
            };

            var input = new CohortRuleInput
            {
                Person = new PersonDto { Gender = "MALE", City = "Delhi" },
                Consumer = new ConsumerDto { PlanId = "2" }
            };

            var result = await svc.EvaluateRule("CohortC", cohortRule, input);

            Assert.False(result.RulesExecutionResult);
            _logger.VerifyLog(LogLevel.Information, times: Times.AtLeastOnce());
        }

        [Fact]
        public async TaskA EvaluateRule_BadExpression_DoesNotThrow_ReturnsFalse()
        {
            var svc = new ConsumerCohortRuleProcessor(_logger.Object, _consumerPurseCohortAssignmentService.Object);
            var cohortRule = new CohortRuleArrayJson
            {
                RuleExpr = new List<string>
            {
                // intentionally broken expression (missing quote)
                "Person.Gender == \"MALE"
            },
                SuccessExpr = "true"
            };

            var input = new CohortRuleInput
            {
                Person = new PersonDto { Gender = "MALE", City = "Delhi" },
                Consumer = new ConsumerDto { PlanId = "2" }
            };

            // Should not throw due to settings.IgnoreException = true; should log and return false.
            var result = await svc.EvaluateRule("CohortD", cohortRule, input);

            Assert.False(result.RulesExecutionResult);
            _logger.VerifyLog(LogLevel.Information, times: Times.AtLeastOnce());
        }

        [Fact]
        public void NormalizeIdentifiers_ReplacesOnlyRootIdentifiers()
        {
            var expr = "Person.Age > 25 && Consumer.PlanId == 3 && person.Gender == \"MALE\"";
            var normalized = ConsumerCohortRuleProcessor.NormalizeIdentifiers(expr);

            Assert.Contains("person.Age > 25", normalized);
            Assert.Contains("consumer.PlanId == 3", normalized);
            // Should not meddle with property names or string literals:
            Assert.Contains("person.Gender == \"MALE\"", normalized);
        }
    }

    /// <summary>
    /// Quick logger verification helper for Moq.
    /// </summary>
    public static class LoggerMoqExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times? times = null)
        {
            times ??= Times.Once();
            logger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v != null), // we don't assert exact message text here
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times.Value);
        }
    }

}


