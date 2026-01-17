using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;


namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class JobReportController : ControllerBase
    {
        private readonly ILogger<JobReportController> _jobReportLogger;
        private readonly IJobReportService _jobReportService;
        private const string className = nameof(JobReportController);

        public JobReportController(ILogger<JobReportController> jobReportLogger,
            IJobReportService jobReportService)
        {
            _jobReportLogger = jobReportLogger;
            _jobReportService = jobReportService;
        }

        /// <summary>
        /// Retrieves job reports based on the job name and job report code provided.
        /// </summary>
        /// <param name="JobReportRequestDto">Contains JobReport code, search term, and pagination parameters.</param>
        /// <returns>A paginated list of job Report that match the search criteria.</returns>
        /// <remarks>This method performs optional search, and pagination.</remarks>
        [HttpPost("get-job-report")]
        public async Task<IActionResult> GetJobReports([FromBody] JobReportRequestDto jobReportRequestDto)
        {
            const string methodName = nameof(GetJobReports);
            try
            {
                _jobReportLogger.LogInformation("{className}.{methodName}: API - Started with JobReportCode : {JobReportCode}, Search : {Search}", className, methodName, jobReportRequestDto.JobReportCode, jobReportRequestDto.SearchByJobName);

                var response = await _jobReportService.GetJobReports(jobReportRequestDto);

                if (response.ErrorCode != null)
                {
                    _jobReportLogger.LogError("{ClassName}.{MethodName}: Error occurred while fetching JobReport. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, jobReportRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _jobReportLogger.LogInformation("{ClassName}.{MethodName}: JobReport fetched successful, JobReportCode: {JobReportCode}", className, methodName, jobReportRequestDto.JobReportCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _jobReportLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching JobReport. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Save job reports based on data provided.
        /// </summary>
        /// <param name="BatchJobReportDto">Contains records for table BatchJobReportDto.</param>
        /// <returns>BatchJobReportResponseDto</returns>
        /// <remarks>Method to insert data in Batch Job Report Table</remarks>
        [HttpPost("job-report")]
        public async Task<IActionResult> SaveJobReports([FromBody] BatchJobReportDto batchJobReportDto)
        {
            const string methodName = nameof(SaveJobReports);
            try
            {
                _jobReportLogger.LogInformation("{className}.{methodName}: API - Started with JobReportCode : {JobReportCode}", className, methodName, batchJobReportDto.BatchJobReportCode);

                var response = await _jobReportService.SaveJobReport(batchJobReportDto);

                if (response.ErrorCode != null)
                {
                    _jobReportLogger.LogError("{ClassName}.{MethodName}: Error occurred while Saving JobReport. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, batchJobReportDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _jobReportLogger.LogInformation("{ClassName}.{MethodName}: JobReport saved successfully, JobReportCode: {JobReportCode}", className, methodName, batchJobReportDto.BatchJobReportCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _jobReportLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while saving JobReport. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError , ErrorMessage = ex.Message });
            }
        }
    }
}
