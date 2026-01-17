using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TriviaQuestionGroupService : ITriviaQuestionGroupService
    {
        public readonly ILogger<TriviaQuestionGroupService> _logger;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TriviaQuestionGroupService);

        public TriviaQuestionGroupService(ILogger<TriviaQuestionGroupService> logger, ITaskClient taskClient)
        {
            _logger = logger;
            _taskClient = taskClient;
        }
        public async Task<BaseResponseDto> CreateTriviaQuestionGroup(TriviaQuestionGroupRequestDto requestDto)
        {
            const string methodName = nameof(CreateTriviaQuestionGroup);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create CreateTriviaQuestionGroup process started for requestDto.TriviaQuestionCode: {CreateTriviaQuestionGroupCode}", className, methodName, requestDto.TriviaQuestionCode);

                var taskResponse = await _taskClient.Post<BaseResponseDto>(Constant.CreateTriviaQuestionGroupRequest, requestDto);
                if (taskResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating triviaQuestion, requestDto.TriviaQuestionCode: {Trivia}, ErrorCode: {ErrorCode}", className, methodName, requestDto.TriviaQuestionCode, taskResponse.ErrorCode);
                    return taskResponse;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: triviaQuestion created successfully, requestDto.TriviaQuestionCode: {TriviaQuestionCode}", className, methodName, requestDto.TriviaQuestionCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating triviaQuestion. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace},", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Gets the trivia question groups by trivia identifier.
        /// </summary>
        /// <param name="triviaId">The trivia identifier.</param>
        /// <returns></returns>
        public async Task<TriviaQuestionGroupResponseDto> GetTriviaQuestionGroupsByTriviaId(long triviaId)
        {
            return await _taskClient.GetById<TriviaQuestionGroupResponseDto>(Constant.TriviaQuestionGroupsAPIUrl + "/", triviaId);
        }

        /// <summary>
        /// Updates the trivia question group.
        /// </summary>
        /// <param name="triviaQuestionGroupId">The trivia question group identifier.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        public async Task<TriviaQuestionGroupUpdateResponseDto> UpdateTriviaQuestionGroup(long triviaQuestionGroupId, TriviaQuestionGroupDto updateRequest)
        {
            return await _taskClient.Put<TriviaQuestionGroupUpdateResponseDto>($"{Constant.TriviaQuestionGroupsAPIUrl}/{triviaQuestionGroupId.ToString()}", updateRequest);
        }

        /// <summary>
        /// Deletes the trivia question group.
        /// </summary>
        /// <param name="triviaQuestionGroupId">The trivia question group identifier.</param>
        /// <returns></returns>
        public async Task<BaseResponseDto> DeleteTriviaQuestionGroup(long triviaQuestionGroupId)
        {
            return await _taskClient.Delete<BaseResponseDto>($"{Constant.TriviaQuestionGroupsAPIUrl}/{triviaQuestionGroupId.ToString()}");
        }

    }
}
