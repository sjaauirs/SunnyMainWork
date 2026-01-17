using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Core.Constants;

namespace Sunny.Benefits.Bff.Api.Filters
{
    public class ValidateLanguageCodeAttribute : IAsyncActionFilter
    {
        private readonly ILogger<ValidateLanguageCodeAttribute> _logger;
        private readonly ICommonHelper _commonHelper;

        public ValidateLanguageCodeAttribute(ILogger<ValidateLanguageCodeAttribute> logger, ICommonHelper commonHelper)
        {
            _logger = logger;
            _commonHelper = commonHelper;
        }
        public async System.Threading.Tasks.Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                _logger.LogInformation("Action {HeliosActionName} is getting valid language code.", context.ActionDescriptor.DisplayName);
                foreach (var param in context.ActionArguments)
                {
                    if (param.Value != null)
                    {
                        var languageCodeProperty = param.Value.GetType().GetProperty("LanguageCode", BindingFlags.Public | BindingFlags.Instance);

                        if (languageCodeProperty != null)
                        {
                            var languageCodeValue = languageCodeProperty.GetValue(param.Value)?.ToString();
                            _logger.LogInformation("Parameter {Key} has consumerCodeValue = {consumerCodeValue}", param.Key, languageCodeValue);
                            if (string.IsNullOrEmpty(languageCodeValue))
                            {
                                var consumerLanguageCode = await _commonHelper.GetLanguageCode();
                                consumerLanguageCode = consumerLanguageCode ?? CommonConstants.DefaultLanguageCode;
                                languageCodeProperty.SetValue(param.Value, consumerLanguageCode);

                                _logger.LogWarning("languageCodeValue not Passed in request hence setting person language code.");

                            }

                        }
                    }
                }
                await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ValidateLanguageCodeAttribute- msg: {Msg}", ex.Message);
                context.Result = new ForbidResult();
            }
        }


    }

}
