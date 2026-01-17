using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;


namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class JobDetailReportController : ControllerBase
    {
        private readonly ILogger<JobDetailReportController> _jobDetailReportLogger;
        private readonly IJobDetailReportService _jobDetailReportService;
        private const string className = nameof(JobDetailReportController);

        public JobDetailReportController(ILogger<JobDetailReportController> jobDetailReportLogger,
            IJobDetailReportService jobDetailReportService)
        {
            _jobDetailReportLogger = jobDetailReportLogger;
            _jobDetailReportService = jobDetailReportService;
        }

        /// <summary>
        /// Retrieves job detail report based on the report code provided.
        /// </summary>
        /// <param name="JobDetailReportRequestDto">Contains JobDetailReport code.</param>
        /// <returns>Paginated list of job detail Report that match the JobDetailReport code.</returns>
        /// <remarks>This method performs return data for job detail report</remarks>
        [HttpPost("get-detail-report")]
        public async Task<IActionResult> GetJobDetailReport([FromBody] JobDetailReportRequestDto jobDetailReportRequestDto)
        {
            const string methodName = nameof(GetJobDetailReport);
            try
            {
                _jobDetailReportLogger.LogInformation("{className}.{methodName}: API - Started with JobDetailReportCode : {JobDetailReportCode}", className, methodName, jobDetailReportRequestDto.JobReportCode);

                var response = await _jobDetailReportService.GetJobDetailReport(jobDetailReportRequestDto);
                
                if (response.ErrorCode != null)
                {
                    _jobDetailReportLogger.LogError("{ClassName}.{MethodName}: Error occurred while fetching Job Detail Report. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, jobDetailReportRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _jobDetailReportLogger.LogInformation("{ClassName}.{MethodName}: Job Detail Report fetched successful, JobDetailReportCode: {JobDetailReportCode}", className, methodName, jobDetailReportRequestDto.JobReportCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _jobDetailReportLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching Job Detail Report. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }

        // <summary>
        /// Save Job Detail Report based on data provided
        /// </summary>
        /// <param name="BatchJobDetailReportRequestDto">Contains list of JobDetailReport</param>
        /// <returns>Saved Records</returns>
        /// <remarks>This method saves data in batch job detail report</remarks>
        [HttpPost("detail-report")]
        public async Task<IActionResult> SaveJobDetailReport([FromBody] BatchJobDetailReportRequestDto batchJobDetailReportRequestDto)
        {
            const string methodName = nameof(SaveJobDetailReport);
            try
            {
                _jobDetailReportLogger.LogInformation("{className}.{methodName}: API - Started with jobDetailReportId : {jobDetailReportId}", className, methodName, batchJobDetailReportRequestDto.BatchJobDetailReportDtos?.FirstOrDefault()?.BatchJobDetailReportId ?? 0);

                var response = await _jobDetailReportService.SaveJobDetailReport(batchJobDetailReportRequestDto);

                if (response.ErrorCode != null)
                {
                    _jobDetailReportLogger.LogError("{ClassName}.{MethodName}: Error occurred while Saving Job Detail Report. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, batchJobDetailReportRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _jobDetailReportLogger.LogInformation("{ClassName}.{MethodName}: Job Detail Report  saved successful, jobDetailReportId: {jobDetailReportId}", className, methodName, batchJobDetailReportRequestDto.BatchJobDetailReportDtos?.FirstOrDefault()?.BatchJobDetailReportId ?? 0);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _jobDetailReportLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching Job Report Details. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }
    }
}
