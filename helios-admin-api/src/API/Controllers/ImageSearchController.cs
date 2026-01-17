using Grpc.Net.Client.Configuration;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class ImageSearchController : ControllerBase
    {
        private readonly ILogger<ImageSearchController> _logger;
        private readonly IImageSearchService _imageSearchService;

        private const string className = nameof(ImageSearchController);

        public ImageSearchController(ILogger<ImageSearchController> logger, IImageSearchService imageSearchService)
        {
            _logger = logger;
            _imageSearchService = imageSearchService;
        }

        [HttpPost("image-analysis")]
        public async Task<ActionResult<ImageSearchResponseDto>> AnalyzeImage([FromBody] ImageSearchRequestDto imageRequest)
        {
            const string methodName = nameof(AnalyzeImage);

            try
            {
                _logger.LogInformation("{className}.{methodName}: API started with request:{imageRequest}", className, methodName, imageRequest.ToJson());

                if (imageRequest != null)
                {
                    // Call the service to analyze the image
                    var response = await _imageSearchService.AnalyzeImage(imageRequest);
                    if (response.ErrorCode != null)
                    {
                        _logger.LogError("{ClassName}.{MethodName}: Image search failed for request: {request}. ErrorCode: {ErrorCode}, Response Data: {ResponseData}",
                        className, methodName, imageRequest.ToJson(), response.ErrorCode, response.ToJson());
                        return new ImageSearchResponseDto() { ErrorCode = response.ErrorCode, ErrorMessage = response.ErrorMessage, ErrorDescription = response.ErrorDescription};
                    }

                    _logger.LogInformation("{ClassName}.{MethodName}: Image search completed successfully for request: {request}. Response Data: {ResponseData}",
                        className, methodName, imageRequest.ToJson(), response.ToJson());

                    return Ok(response);
                    
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName}: Image request dto is invalid:{request}",
                    className, methodName, imageRequest?.ToJson());
                    return new ImageSearchResponseDto() { ErrorCode = StatusCodes.Status204NoContent, ErrorMessage = "Failed to analyze image" };

                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new ImageSearchResponseDto() { ErrorCode=StatusCodes.Status500InternalServerError,ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }
    }
}
