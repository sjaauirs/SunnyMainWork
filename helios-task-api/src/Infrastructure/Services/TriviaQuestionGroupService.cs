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
    public class TriviaQuestionGroupService : ITriviaQuestionGroupService
    {
        private readonly ITriviaQuestionGroupRepo _triviaQuestionGroupRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<TriviaQuestionGroupService> _logger;
        private const string className = nameof(TriviaQuestionGroupService);

        public TriviaQuestionGroupService(
            ITriviaQuestionGroupRepo triviaQuestionGroupRepo,
            IMapper mapper,
            ILogger<TriviaQuestionGroupService> logger)
        {
            _triviaQuestionGroupRepo = triviaQuestionGroupRepo;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Gets the trivia question groups by trivia identifier.
        /// </summary>
        /// <param name="triviaId">The trivia identifier.</param>
        /// <returns></returns>
        public async Task<TriviaQuestionGroupResponseDto> GetTriviaQuestionGroupsByTriviaId(long triviaId)
        {
            const string methodName = nameof(GetTriviaQuestionGroupsByTriviaId);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Fetching trivia question groups for TriviaId: {TriviaId}.", className, methodName, triviaId);

                var triviaQuestionGroups = await _triviaQuestionGroupRepo.FindAsync(x => x.TriviaId == triviaId && x.DeleteNbr == 0);
                return new TriviaQuestionGroupResponseDto
                {
                    TriviaQuestionGroupList = _mapper.Map<List<TriviaQuestionGroupDto>>(triviaQuestionGroups)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: An error occurred while fetching trivia question groups. TriviaId: {TriviaId}, Error: {ErrorMessage}.",
                    className, methodName, triviaId, ex.Message);

                return new TriviaQuestionGroupResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while retrieving trivia question groups."
                };
            }
        }

        /// <summary>
        /// Updates the trivia question group.
        /// </summary>
        /// <param name="triviaQuestionGroupId">The trivia question group identifier.</param>
        /// <param name="updateRequest">The update request.</param>
        /// <returns></returns>
        public async Task<TriviaQuestionGroupUpdateResponseDto> UpdateTriviaQuestionGroup(long triviaQuestionGroupId, TriviaQuestionGroupDto updateRequest)
        {
            const string methodName = nameof(UpdateTriviaQuestionGroup);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Updating trivia question group. Id: {TriviaQuestionGroupId}.", className, methodName, triviaQuestionGroupId);

                if (triviaQuestionGroupId != updateRequest.TriviaQuestionGroupId)
                {
                    _logger.LogError("{className}.{methodName}: TriviaQuestionGroupId mismatch between path and body. Path: {PathId}, Body: {BodyId}.",
                        className, methodName, triviaQuestionGroupId, updateRequest.TriviaQuestionGroupId);
                    return new TriviaQuestionGroupUpdateResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "TriviaQuestionGroupId in the path and body must match.",
                        IsSuccess = false
                    };
                }

                var triviaQuestionGroup = await _triviaQuestionGroupRepo.FindOneAsync(x => x.TriviaQuestionGroupId == triviaQuestionGroupId && x.DeleteNbr == 0);
                if (triviaQuestionGroup == null)
                {
                    _logger.LogError("{className}.{methodName}: Trivia question group not found. Id: {TriviaQuestionGroupId}.", className, methodName, triviaQuestionGroupId);
                    return new TriviaQuestionGroupUpdateResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Trivia question group not found.",
                        IsSuccess = false
                    };
                }

                // Update fields
                triviaQuestionGroup.TriviaId = updateRequest.TriviaId;
                triviaQuestionGroup.TriviaQuestionId = updateRequest.TriviaQuestionId;
                triviaQuestionGroup.SequenceNbr = updateRequest.SequenceNbr;
                triviaQuestionGroup.ValidStartTs = updateRequest.ValidStartTs;
                triviaQuestionGroup.ValidEndTs = updateRequest.ValidEndTs;
                triviaQuestionGroup.UpdateUser = updateRequest.UpdateUser ?? Constant.SystemUser;
                triviaQuestionGroup.UpdateTs = DateTime.UtcNow;

                await _triviaQuestionGroupRepo.UpdateAsync(triviaQuestionGroup);

                _logger.LogInformation("{className}.{methodName}: Successfully updated trivia question group. Id: {TriviaQuestionGroupId}.", className, methodName, triviaQuestionGroupId);

                return new TriviaQuestionGroupUpdateResponseDto { IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: An error occurred while updating trivia question group. Id: {TriviaQuestionGroupId}, Error: {ErrorMessage}.",
                    className, methodName, triviaQuestionGroupId, ex.Message);

                return new TriviaQuestionGroupUpdateResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while updating the trivia question group.",
                    IsSuccess = false
                };
            }
        }

        /// <summary>
        /// Deletes the trivia question group.
        /// </summary>
        /// <param name="triviaQuestionGroupId">The trivia question group identifier.</param>
        /// <returns></returns>
        public async Task<BaseResponseDto> DeleteTriviaQuestionGroup(long triviaQuestionGroupId)
        {
            const string methodName = nameof(DeleteTriviaQuestionGroup);

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Deleting trivia question group. Id: {TriviaQuestionGroupId}.", className, methodName, triviaQuestionGroupId);

                var triviaQuestionGroup = await _triviaQuestionGroupRepo.FindOneAsync(x => x.TriviaQuestionGroupId == triviaQuestionGroupId && x.DeleteNbr == 0);
                if (triviaQuestionGroup == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Trivia question group not found. Id: {TriviaQuestionGroupId}.", className, methodName, triviaQuestionGroupId);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Trivia question group not found."
                    };
                }
                triviaQuestionGroup.DeleteNbr = triviaQuestionGroupId;
                triviaQuestionGroup.UpdateTs = DateTime.UtcNow;
                triviaQuestionGroup.UpdateUser = Constant.SystemUser;

                await _triviaQuestionGroupRepo.UpdateAsync(triviaQuestionGroup);

                _logger.LogInformation("{ClassName}.{MethodName}: Successfully deleted trivia question group. Id: {TriviaQuestionGroupId}.", className, methodName, triviaQuestionGroupId);

                return new BaseResponseDto();
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while deleting trivia question group. Id: {TriviaQuestionGroupId}, Error: {ErrorMessage}.",
                    className, methodName, triviaQuestionGroupId, ex.Message);

                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while deleting the trivia question group."
                };
            }
        }

    }

}
