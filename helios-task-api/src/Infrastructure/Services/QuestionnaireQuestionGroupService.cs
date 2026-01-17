using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class QuestionnaireQuestionGroupService : IQuestionnaireQuestionGroupService
    {
        private readonly IQuestionnaireQuestionGroupRepo _questionnaireQuestionGroupRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestionnaireQuestionGroupService> _logger;
        private const string className = nameof(QuestionnaireQuestionGroupService);

        public QuestionnaireQuestionGroupService(IQuestionnaireQuestionGroupRepo questionnaireQuestionGroupRepo,
            IMapper mapper, ILogger<QuestionnaireQuestionGroupService> logger)
        {
            _questionnaireQuestionGroupRepo = questionnaireQuestionGroupRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BaseResponseDto> DeleteQuestionnaireQuestionGroup(long questionnaireQuestionGroupId)
        {
            const string methodName = nameof(DeleteQuestionnaireQuestionGroup);

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Deleting Questionnaire question group. Id: {QuestionnaireQuestionGroupId}.", className, methodName, questionnaireQuestionGroupId);

                var questionnaireQuestionGroup = await _questionnaireQuestionGroupRepo.FindOneAsync(x => x.QuestionnaireQuestionGroupId == questionnaireQuestionGroupId && x.DeleteNbr == 0);
                if (questionnaireQuestionGroup == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Questionnaire question group not found. Id: {QuestionnaireQuestionGroupId}.", className, methodName, questionnaireQuestionGroupId);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Questionnaire question group not found."
                    };
                }
                questionnaireQuestionGroup.DeleteNbr = questionnaireQuestionGroupId;
                questionnaireQuestionGroup.UpdateTs = DateTime.UtcNow;
                questionnaireQuestionGroup.UpdateUser = Constant.SystemUser;

                await _questionnaireQuestionGroupRepo.UpdateAsync(questionnaireQuestionGroup);

                _logger.LogInformation("{ClassName}.{MethodName}: Successfully deleted Questionnaire question group. Id: {QuestionnaireQuestionGroupId}.", className, methodName, questionnaireQuestionGroupId);

                return new BaseResponseDto();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while deleting Questionnaire question group. Id: {QuestionnaireQuestionGroupId}, Error: {ErrorMessage}.",
                    className, methodName, questionnaireQuestionGroupId, ex.Message);

                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while deleting the Questionnaire question group."
                };
            }
        }

        public async Task<QuestionnaireQuestionGroupResponseDto> GetQuestionnaireQuestionGroupsByQuestionnaireId(long questionnaireId)
        {
            const string methodName = nameof(GetQuestionnaireQuestionGroupsByQuestionnaireId);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Fetching Questionnaire question groups for QuestionnaireId: {QuestionnaireId}.", className, methodName, questionnaireId);

                var questionnaireQuestionGroups = await _questionnaireQuestionGroupRepo.FindAsync(x => x.QuestionnaireId == questionnaireId && x.DeleteNbr == 0);
                return new QuestionnaireQuestionGroupResponseDto
                {
                    QuestionnaireQuestionGroupList = _mapper.Map<List<QuestionnaireQuestionGroupDto>>(questionnaireQuestionGroups)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: An error occurred while fetching Questionnaire question groups. QuestionnaireId: {QuestionnaireId}, Error: {ErrorMessage}.",
                    className, methodName, questionnaireId, ex.Message);

                return new QuestionnaireQuestionGroupResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while retrieving Questionnaire question groups."
                };
            }
        }

        /// <summary>
        /// Update Questionnaire Question Group
        /// </summary>
        /// <param name="questionnaireQuestionGroupId"></param>
        /// <param name="updateRequest"></param>
        /// <returns></returns>
        public async Task<QuestionnaireQuestionGroupUpdateResponseDto> UpdateQuestionnaireQuestionGroup(long questionnaireQuestionGroupId, QuestionnaireQuestionGroupDto updateRequest)
        {
            const string methodName = nameof(UpdateQuestionnaireQuestionGroup);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Updating Questionnaire question group. Id: {QuestionnaireQuestionGroupId}.", className, methodName, questionnaireQuestionGroupId);

                if (questionnaireQuestionGroupId != updateRequest.QuestionnaireQuestionGroupId)
                {
                    _logger.LogError("{className}.{methodName}: QuestionnaireQuestionGroupId mismatch between path and body. Path: {PathId}, Body: {BodyId}.",
                        className, methodName, questionnaireQuestionGroupId, updateRequest.QuestionnaireQuestionGroupId);
                    return new QuestionnaireQuestionGroupUpdateResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "QuestionnaireQuestionGroupId in the path and body must match.",
                        IsSuccess = false
                    };
                }

                var questionnaireQuestionGroup = await _questionnaireQuestionGroupRepo.FindOneAsync(x => x.QuestionnaireQuestionGroupId == questionnaireQuestionGroupId && x.DeleteNbr == 0);
                if (questionnaireQuestionGroup == null)
                {
                    _logger.LogError("{className}.{methodName}: Questionnaire question group not found. Id: {QuestionnaireQuestionGroupId}.", className, methodName, questionnaireQuestionGroupId);
                    return new QuestionnaireQuestionGroupUpdateResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Questionnaire question group not found.",
                        IsSuccess = false
                    };
                }

                // Update fields
                questionnaireQuestionGroup.QuestionnaireId = updateRequest.QuestionnaireId;
                questionnaireQuestionGroup.QuestionnaireQuestionId = updateRequest.QuestionnaireQuestionId;
                questionnaireQuestionGroup.SequenceNbr = updateRequest.SequenceNbr;
                questionnaireQuestionGroup.ValidStartTs = updateRequest.ValidStartTs;
                questionnaireQuestionGroup.ValidEndTs = updateRequest.ValidEndTs;
                questionnaireQuestionGroup.UpdateUser = updateRequest.UpdateUser ?? Constant.SystemUser;
                questionnaireQuestionGroup.UpdateTs = DateTime.UtcNow;

                await _questionnaireQuestionGroupRepo.UpdateAsync(questionnaireQuestionGroup);

                _logger.LogInformation("{className}.{methodName}: Successfully updated questionnaire question group. Id: {QuestionnaireQuestionGroupId}.", className, methodName, questionnaireQuestionGroupId);

                return new QuestionnaireQuestionGroupUpdateResponseDto { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: An error occurred while updating questionnaire question group. Id: {QuestionnaireQuestionGroupId}, Error: {ErrorMessage}.",
                    className, methodName, questionnaireQuestionGroupId, ex.Message);

                return new QuestionnaireQuestionGroupUpdateResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while updating the questionnaire question group.",
                    IsSuccess = false
                };
            }
        }
    }
}
