using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Services
{
    public class DynamicQueryProcessorTests
    {
        private readonly Mock<ILogger<DynamicQueryProcessor>> _loggerMock;
        private readonly DynamicQueryProcessor _processor;

        public DynamicQueryProcessorTests()
        {
            _loggerMock = new Mock<ILogger<DynamicQueryProcessor>>();
            _processor = new DynamicQueryProcessor(_loggerMock.Object);
        }

        [Fact]
        public void GetFilterObject_ReturnsPropertyValue_IgnoringCase()
        {
            var context = new DynamicFilterContext
            {
                Consumer = new ConsumerFilter { ConsumerCode = "C123" }
            };

            var result = _processor.GetFilterObject(context, "consumer");
            Assert.NotNull(result);
            Assert.IsType<ConsumerFilter>(result);

            var result2 = _processor.GetFilterObject(context, "CONSUMER");
            Assert.NotNull(result2);
            Assert.IsType<ConsumerFilter>(result2);
        }

        [Fact]
        public void GetFilterObject_ReturnsNull_WhenKeyNotFound()
        {
            var context = new DynamicFilterContext
            {
                Consumer = new ConsumerFilter { ConsumerCode = "C123" }
            };

            var result = _processor.GetFilterObject(context, "nonexistent");
            Assert.Null(result);
        }

        [Fact]
        public void GetFilterObject_ReturnsNull_WhenContextIsNullOrKeyIsEmpty()
        {
            Assert.Null(_processor.GetFilterObject(null, "consumer"));
            Assert.Null(_processor.GetFilterObject(new DynamicFilterContext(), ""));
        }

        [Fact]
        public void EvaluateConditionsForContext_ReturnsTrue_ForMatchingConditions_AND()
        {
            var consumer = new ConsumerFilter { ConsumerCode = "C123", OnBoardingState = "verified" };
            var conditions = new List<Condition>
            {
                new Condition
                {
                    AttributeName = "ConsumerCode",
                    Operator = "=",
                    DataType = "string",
                    AttributeValue = "C123",
                    Criteria = "AND"
                },
                new Condition
                {
                    AttributeName = "OnBoardingState",
                    Operator = "=",
                    DataType = "string",
                    AttributeValue = "verified"
                }
            };

            var result = _processor.EvaluateConditionsForContext(consumer, conditions);
            Assert.True(result);
        }

        [Fact]
        public void EvaluateConditionsForContext_ReturnsFalse_ForNonMatchingConditions_AND()
        {
            var consumer = new ConsumerFilter { ConsumerCode = "C123", OnBoardingState = "verified" };
            var conditions = new List<Condition>
            {
                new Condition
                {
                    AttributeName = "ConsumerCode",
                    Operator = "=",
                    DataType = "string",
                    AttributeValue = "C123",
                    Criteria = "AND"
                },
                new Condition
                {
                    AttributeName = "OnBoardingState",
                    Operator = "=",
                    DataType = "string",
                    AttributeValue = "Jane"
                }
            };

            var result = _processor.EvaluateConditionsForContext(consumer, conditions);
            Assert.False(result);
        }

        [Fact]
        public void EvaluateConditionsForContext_ReturnsTrue_ForMatchingConditions_OR()
        {
            var consumer = new ConsumerFilter { ConsumerCode = "C123", OnBoardingState = "verified" };
            var conditions = new List<Condition>
            {
                new Condition
                {
                    AttributeName = "ConsumerCode",
                    Operator = "=",
                    DataType = "string",
                    AttributeValue = "C999",
                    Criteria = "OR"
                },
                new Condition
                {
                    AttributeName = "OnBoardingState",
                    Operator = "=",
                    DataType = "string",
                    AttributeValue = "verified"
                }
            };

            var result = _processor.EvaluateConditionsForContext(consumer, conditions);
            Assert.True(result);
        }

        [Fact]
        public void EvaluateConditionsForContext_ReturnsFalse_WhenConditionsEmpty()
        {
            var consumer = new ConsumerFilter { ConsumerCode = "C123", OnBoardingState = "verified" };
            var conditions = new List<Condition>();
            var result = _processor.EvaluateConditionsForContext(consumer, conditions);
            Assert.False(result);
        }

        [Fact]
        public void EvaluateConditionsForAllContexts_ReturnsTrue_IfAnyContextMatches()
        {
            var filterContext = new DynamicFilterContext
            {
                Consumer = new ConsumerFilter { ConsumerCode = "C123", OnBoardingState = "John" }
            };

            var contexts = new Dictionary<string, List<Condition>>
            {
                { "consumer", new List<Condition>
                    {
                        new Condition
                        {
                            AttributeName = "ConsumerCode",
                            Operator = "=",
                            DataType = "string",
                            AttributeValue = "C123"
                        }
                    }
                }
            };

            var result = _processor.EvaluateConditionsForAllContexts(contexts, filterContext);
            Assert.True(result);
        }

        [Fact]
        public void EvaluateConditionsForAllContexts_ReturnsFalse_IfNoContextMatches()
        {
            var filterContext = new DynamicFilterContext
            {
                Consumer = new ConsumerFilter { ConsumerCode = "C123", OnBoardingState = "John" }
            };

            var contexts = new Dictionary<string, List<Condition>>
            {
                { "consumer", new List<Condition>
                    {
                        new Condition
                        {
                            AttributeName = "ConsumerCode",
                            Operator = "=",
                            DataType = "string",
                            AttributeValue = "C999"
                        }
                    }
                }
            };

            var result = _processor.EvaluateConditionsForAllContexts(contexts, filterContext);
            Assert.False(result);
        }

        [Fact]
        public void EvaluateConditionsForAllContexts_ReturnsFalse_WhenContextsEmpty()
        {
            var filterContext = new DynamicFilterContext
            {
                Consumer = new ConsumerFilter { ConsumerCode = "C123", OnBoardingState = "John" }
            };

            var contexts = new Dictionary<string, List<Condition>>();
            var result = _processor.EvaluateConditionsForAllContexts(contexts, filterContext);
            Assert.False(result);
        }
    }
}
