using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Health.Core.Domains.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("/api/v1/")]
    [ApiController]
    [Authorize]
    public class ImageSearchController : ControllerBase
    {
        private readonly ILogger<ImageSearchController> _logger;
        private readonly IImageSearchService _imageSearchService;

        private const string className = nameof(ImageSearchController);

        public ImageSearchController( ILogger<ImageSearchController> logger, IImageSearchService imageSearchService)
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
                    ImageSearchResponseDto imageSearchResponseDto = await _imageSearchService.AnalyseImageSearch(imageRequest);
                    if (imageSearchResponseDto.ErrorCode != null)
                    {
                        _logger.LogError("{ClassName}.{MethodName}: Image search failed for request: {request}. ErrorCode: {ErrorCode}, Response Data: {ResponseData}",
                        className, methodName, imageRequest.ToJson(), imageSearchResponseDto.ErrorCode, imageSearchResponseDto.ToJson());
                        return StatusCode((int)imageSearchResponseDto.ErrorCode, imageSearchResponseDto);
                    }

                    _logger.LogInformation("{ClassName}.{MethodName}: Image search completed successfully for request: {request}. Response Data: {ResponseData}",
                        className, methodName, imageRequest.ToJson(), imageSearchResponseDto.ToJson());

                    return Ok(imageSearchResponseDto);
                  
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName}: Image request dto is invalid",
                    className, methodName);
                    return StatusCode(StatusCodes.Status204NoContent, new { Error = "Failed to analyze image" });


                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new ImageSearchResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }
        
    }
}
