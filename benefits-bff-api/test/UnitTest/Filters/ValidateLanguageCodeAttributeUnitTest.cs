using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Filters;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Filters
{
    public class ValidateLanguageCodeAttributeUnitTest
    {
        private readonly Mock<ILogger<ValidateLanguageCodeAttribute>> _mockLogger;
        private readonly Mock<ICommonHelper> _mockCommonHelper;
        private readonly ValidateLanguageCodeAttribute _attribute;

        public ValidateLanguageCodeAttributeUnitTest()
        {
            _mockLogger = new Mock<ILogger<ValidateLanguageCodeAttribute>>();
            _mockCommonHelper = new Mock<ICommonHelper>();
            _attribute = new ValidateLanguageCodeAttribute(_mockLogger.Object, _mockCommonHelper.Object);
        }

        private ActionExecutingContext CreateActionExecutingContext(Dictionary<string, object> actionArguments)
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
            return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, new Mock<Controller>().Object);
        }

        [Fact]
        public async System.Threading.Tasks.Task OnActionExecutionAsync_languageCodeIsNull_ForbidResult()
        {
            // Arrange
            var actionArguments = new Dictionary<string, object>
            {
                { "param",  new { LanguageCode = string.Empty } }
            };
            var context = CreateActionExecutingContext(actionArguments);
            var actionExecutionDelegate = new Mock<ActionExecutionDelegate>();

            // Act
            await _attribute.OnActionExecutionAsync(context, actionExecutionDelegate.Object);

            // Assert
            Assert.NotNull(context.Result);
        }

        [Fact]
        public async System.Threading.Tasks.Task OnActionExecutionAsync_languageIsNotVerified_ForbidResult()
        {
            // Arrange
            var actionArguments = new Dictionary<string, object>
                {
                    { "param", new { LanguageCode = "en-US" } }
                };
            var context = CreateActionExecutingContext(actionArguments);
            var actionExecutionDelegate = new Mock<ActionExecutionDelegate>();

            _mockCommonHelper.Setup(ph => ph.GetLanguageCode())
                .ReturnsAsync("en=US");

            // Act
            await _attribute.OnActionExecutionAsync(context, actionExecutionDelegate.Object);

            // Assert
            Assert.Null(context.Result);
        }

        [Fact]
        public async System.Threading.Tasks.Task OnActionExecutionAsync_languageIsVerified_ProceedToNext()
        {
            // Arrange
            var actionArguments = new Dictionary<string, object>
        {
                    { "param", new { LanguageCode = "en-US" } }
        };
            var context = CreateActionExecutingContext(actionArguments);
            var actionExecutionDelegate = new Mock<ActionExecutionDelegate>();
            actionExecutionDelegate.Setup(d => d()).Returns(System.Threading.Tasks.Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new Mock<Controller>().Object)));

            _mockCommonHelper.Setup(ph => ph.GetLanguageCode())
               .ReturnsAsync("en-US");

            // Act
            await _attribute.OnActionExecutionAsync(context, actionExecutionDelegate.Object);

            // Assert
            Assert.Null(context.Result);
            actionExecutionDelegate.Verify(d => d(), Times.Once);
        }
    }
}
