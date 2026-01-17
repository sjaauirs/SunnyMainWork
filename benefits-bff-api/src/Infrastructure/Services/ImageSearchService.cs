using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Azure;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class ImageSearchService : IImageSearchService
    {
        public readonly ILogger<ImageSearchService> _logger;
        public readonly IAdminClient _adminClient;
        public const string className = nameof(ImageSearchService);

        public ImageSearchService(ILogger<ImageSearchService> logger, IAdminClient adminClient)
        {
            _logger = logger;
            _adminClient = adminClient;
        }

        public async Task<ImageSearchResponseDto> AnalyseImageSearch(ImageSearchRequestDto requestDto)
        {
            const string methodName = nameof(AnalyseImageSearch);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Image search process started for request: {request}", className, methodName, requestDto.ToJson());

                var imageResponse = await _adminClient.Post<ImageSearchResponseDto>(AdminConstants.AnalyzeImage, requestDto);
                if(imageResponse==null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while Anlysing the image for the request: {request}", className, methodName, requestDto.ToJson());
                    return new ImageSearchResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "No data found"
                    };
                }
                if (imageResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while Anlysing the image for the request: {request}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), imageResponse.ErrorCode);
                    return new ImageSearchResponseDto { 
                    ErrorCode = imageResponse.ErrorCode,
                    ErrorMessage = imageResponse.ErrorMessage
                    };
                }
               
                _logger.LogInformation("{ClassName}.{MethodName}: image Analysis  was successfully, request: {request}", className, methodName, requestDto.ToJson());
                return imageResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}:  Error occurred while Anlysing the image for the request: {request}, ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, requestDto.ToJson(), ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}