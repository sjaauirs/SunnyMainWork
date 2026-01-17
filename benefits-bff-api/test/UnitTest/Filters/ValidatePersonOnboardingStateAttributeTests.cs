using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Filters;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Filters
{
    public class ValidatePersonOnboardingStateAttributeTests
    {
        private readonly Mock<ILogger<ValidatePersonOnboardingStateAttribute>> _mockLogger;
        private readonly Mock<IPersonHelper> _mockPersonHelper;
        private readonly ValidatePersonOnboardingStateAttribute _attribute;

        public ValidatePersonOnboardingStateAttributeTests()
        {
            _mockLogger = new Mock<ILogger<ValidatePersonOnboardingStateAttribute>>();
            _mockPersonHelper = new Mock<IPersonHelper>();
            _attribute = new ValidatePersonOnboardingStateAttribute(_mockLogger.Object, _mockPersonHelper.Object);
        }

        private ActionExecutingContext CreateActionExecutingContext(Dictionary<string, object> actionArguments)
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
            return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, new Mock<Controller>().Object);
        }

        [Fact]
        public async Task OnActionExecutionAsync_ConsumerCodeIsNull_ForbidResult()
        {
            // Arrange
            var actionArguments = new Dictionary<string, object>
            {
                { "param", new FindConsumerWalletRequestDto(){ ConsumerCode = null } }
            };
            var context = CreateActionExecutingContext(actionArguments);
            var actionExecutionDelegate = new Mock<ActionExecutionDelegate>();

            // Act
            await _attribute.OnActionExecutionAsync(context, actionExecutionDelegate.Object);

            // Assert
            Assert.IsType<ForbidResult>(context.Result);
        }

        [Fact]
        public async Task OnActionExecutionAsync_PersonIsNotVerified_ForbidResult()
        {
            // Arrange
            var actionArguments = new Dictionary<string, object>
                {
                    { "param", new { ConsumerCode = "12345" } }
                };
            var context = CreateActionExecutingContext(actionArguments);
            var actionExecutionDelegate = new Mock<ActionExecutionDelegate>();

            _mockPersonHelper.Setup(ph => ph.ValidatePersonIsVerified(It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(false);

            // Act
            await _attribute.OnActionExecutionAsync(context, actionExecutionDelegate.Object);

            // Assert
            Assert.IsType<ForbidResult>(context.Result);
        }

        [Fact]
        public async Task OnActionExecutionAsync_PersonIsVerified_ProceedToNext()
        {
            // Arrange
            var actionArguments = new Dictionary<string, object>
        {
            { "param", new { ConsumerCode = "12345" } }
        };
            var context = CreateActionExecutingContext(actionArguments);
            var actionExecutionDelegate = new Mock<ActionExecutionDelegate>();
            actionExecutionDelegate.Setup(d => d()).Returns(Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new Mock<Controller>().Object)));

            _mockPersonHelper.Setup(ph => ph.ValidatePersonIsVerified(It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(true);

            // Act
            await _attribute.OnActionExecutionAsync(context, actionExecutionDelegate.Object);

            // Assert
            Assert.Null(context.Result);
            actionExecutionDelegate.Verify(d => d(), Times.Once);
        }
    }
}
