using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers
{
    public class QuestionnaireHelper : IQuestionnaireHelper
    {
        private readonly ITaskRewardService _taskRewardService;
        private readonly ILogger<QuestionnaireHelper> _logger;
        private const string className = nameof(QuestionnaireHelper);
        public QuestionnaireHelper(ITaskRewardService taskRewardService, ILogger<QuestionnaireHelper> logger)
        {
            _taskRewardService = taskRewardService;
            _logger = logger;
        }
        public string? FilterQuestionnaireJsonByLanguage(string? questionnaireJson, string? language)
        {
            if (string.IsNullOrWhiteSpace(questionnaireJson))
            {
                return questionnaireJson;
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                language = Constant.LanguageCode.ToLower();
            }

            try
            {
                var jObject = JObject.Parse(questionnaireJson);

                var dict = jObject.Properties()
                          .ToDictionary(p => p.Name.ToLower(), p => p.Value);

                if (dict.TryGetValue(language.ToLower(), out var localizedContent))
                {
                    return localizedContent.ToString(Formatting.None);
                }
                // if not return , return english
                if (dict.TryGetValue(Constant.LanguageCode.ToLower(), out var fallbackContent))
                {
                    return fallbackContent.ToString(Formatting.None);
                }

                return questionnaireJson;
            }
            catch
            {
                return questionnaireJson;
            }
        }

        public async System.Threading.Tasks.Task GetTaskRewardDetails(GetQuestionnaireResponseDto response, TaskRewardModel? ctaTaskreward, string? languageCode)
        {
            var taskRewardRequestDto = new GetTaskRewardByCodeRequestDto();
            taskRewardRequestDto.TaskRewardCode = ctaTaskreward?.TaskRewardCode ?? string.Empty;
            taskRewardRequestDto.LanguageCode = languageCode;

            var taskRewardResponseDto = new GetTaskRewardByCodeResponseDto();
            if (ctaTaskreward?.TaskExternalCode != null)
            {
                _logger.LogInformation("{className}.GetTaskRewarddetails: successfully retrieved data from  GetTaskRewardByCode API for CtaTaskExternalCode: {CtaTaskExternalCode}", className, ctaTaskreward?.TaskExternalCode);
                taskRewardResponseDto = await _taskRewardService.GetTaskRewardByCode(taskRewardRequestDto);
            }
            response.Questionnaire.taskRewardDetail = taskRewardResponseDto.TaskRewardDetail;
        }

        public string? NormalizeJsonInput(object? input)
        {
            if (input == null) return null;

            try
            {
                if (input is string str && !string.IsNullOrWhiteSpace(str))
                {
                    return NormalizeJsonString(str);
                }

                // Already an object (JObject, etc.)
                return SerializeToJson(input);
            }
            catch
            {
                // fallback: store raw string
                return input.ToString();
            }
        }

        private static string NormalizeJsonString(string jsonString)
        {
            object? deserializedObject = jsonString;

            while (deserializedObject is string innerJson)
            {
                try
                {
                    var deserializedTemp = JsonConvert.DeserializeObject(innerJson);

                    // stop if nothing changes (prevents infinite loop)
                    if (deserializedTemp is string tempStr && tempStr == innerJson)
                        break;

                    deserializedObject = deserializedTemp;
                }
                catch
                {
                    // Not valid JSON -> return the raw string
                    return innerJson;
                }
            }

            return SerializeToJson(deserializedObject);
        }

        private static string SerializeToJson(object input)
        {
            return JsonConvert.SerializeObject(input, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.None
            });
        }
    }
}
