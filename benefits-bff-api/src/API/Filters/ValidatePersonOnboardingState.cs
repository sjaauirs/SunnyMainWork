using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Reflection;

namespace Sunny.Benefits.Bff.Api.Filters
{
    public class ValidatePersonOnboardingStateAttribute : IAsyncActionFilter
    {
        private readonly ILogger<ValidatePersonOnboardingStateAttribute> _logger;
        private readonly IPersonHelper _personHelper;

        public ValidatePersonOnboardingStateAttribute(ILogger<ValidatePersonOnboardingStateAttribute> logger, IPersonHelper personHelper)
        {
            _logger = logger;
            _personHelper = personHelper;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                _logger.LogInformation("Action {HeliosActionName} is validating person OnBoarding State.", context.ActionDescriptor.DisplayName);
                foreach (var param in context.ActionArguments)
                {
                    if (param.Value != null)
                    {
                        var consumerCodeProperty = param.Value.GetType().GetProperty("ConsumerCode", BindingFlags.Public | BindingFlags.Instance);

                        if (consumerCodeProperty != null)
                        {
                            var consumerCodeValue = consumerCodeProperty.GetValue(param.Value)?.ToString();
                            _logger.LogInformation("Parameter {Key} has ConsumerCode = {ConsumerCode}", param.Key, consumerCodeValue);
                            if (string.IsNullOrEmpty(consumerCodeValue))
                            {
                                _logger.LogWarning("ConsumerCode not detected - Forbidden.");
                                context.Result = new ForbidResult();
                                return;
                            }
                            else
                            {
                                if (!await _personHelper.ValidatePersonIsVerified(new GetConsumerRequestDto() { ConsumerCode = consumerCodeValue }))
                                {
                                    _logger.LogWarning("Person is not verified - Forbidden.");
                                    context.Result = new ForbidResult();
                                    return;
                                }
                            }
                        }
                    }
                }
                await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ValidatePersonOnboardingStateFilter- msg: {Msg}", ex.Message);
                context.Result = new ForbidResult();
            }
        }
    }
}
