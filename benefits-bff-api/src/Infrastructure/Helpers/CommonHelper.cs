using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sunny.Benefits.Bff.Infrastructure.Helpers
{
    public class CommonHelper : ICommonHelper
    {
        private readonly IUserClient _userClient;
        private readonly ILogger<CommonHelper> _commonHelperLogger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        const string className = nameof(CommonHelper);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userClient"></param>
        /// <param name="commonHelperLogger"></param>
        public CommonHelper(IUserClient userClient, ILogger<CommonHelper> commonHelperLogger, IHttpContextAccessor httpContextAccessor)
        {
            _userClient = userClient;
            _commonHelperLogger = commonHelperLogger;
            _httpContextAccessor = httpContextAccessor;

        }
        public string? GetUserConsumerCodeFromHttpContext()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Items.TryGetValue(HttpContextKeys.JwtConsumerCode, out var consumerCode) == true ? consumerCode as string : null;
        }
        public async Task<string> GetLanguageCode()
        {
            const string methodName = nameof(GetLanguageCode);
            try
            {
                try
                {
                    var ConsumerCode = GetUserConsumerCodeFromHttpContext();
                    if (string.IsNullOrEmpty(ConsumerCode))
                    {
                        return CommonConstants.DefaultLanguageCode;
                    }
                    var personAndConsumer = await _userClient.Post<GetPersonAndConsumerResponseDto>(CommonConstants.GetPersonAndConsumerAPIUrl, new GetConsumerRequestDto() { ConsumerCode = ConsumerCode });
                    if (personAndConsumer != null && personAndConsumer.Person?.LanguageCode != null)
                    {
                        return personAndConsumer.Person.LanguageCode;
                    }
                    return CommonConstants.DefaultLanguageCode;
                }
                catch (Exception ex)
                {
                    _commonHelperLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}", className, methodName, ex.Message);
                    return CommonConstants.DefaultLanguageCode;
                }
            }
            catch (Exception ex)
            {
                _commonHelperLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}", className, methodName, ex.Message);
                return CommonConstants.DefaultLanguageCode;
            }
        }
    }
}
