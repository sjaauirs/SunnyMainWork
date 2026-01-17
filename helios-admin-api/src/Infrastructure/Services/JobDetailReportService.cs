using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class JobDetailReportService : IJobDetailReportService
    {
        public readonly ILogger<JobDetailReportService> _logger;
        private readonly IBatchJobDetailReportRepo _batchJobDetailReportRepo;
        private readonly IMapper _mapper;
        public const string className = nameof(JobDetailReportService);


        public JobDetailReportService(ILogger<JobDetailReportService> logger,IBatchJobDetailReportRepo batchJobDetailReportRepo ,
            IMapper mapper )
        {
            _logger = logger;
            _mapper = mapper;
            _batchJobDetailReportRepo = batchJobDetailReportRepo;
        }

        public async Task<BatchJobDetailReportResponseDto> GetJobDetailReport(JobDetailReportRequestDto jobDetailReportRequestDto)
        {
            const string methodName = nameof(GetJobDetailReport);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Get Job Detail Report started for JobReportCode : {JobReportCode}", className, methodName, jobDetailReportRequestDto.JobReportCode);

                var response = new BatchJobDetailReportResponseDto();
                if (String.IsNullOrWhiteSpace(jobDetailReportRequestDto.JobReportCode))
                {
                    _logger.LogError("Invalid request, JobReportCode is Blank");
                    return new BatchJobDetailReportResponseDto() { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Invalid request, JobDetailReportCode and Search is Blank" };
                }
                var skip = (jobDetailReportRequestDto.PageNumber - 1) * jobDetailReportRequestDto.PageSize;
                var Paginatedresult = await _batchJobDetailReportRepo.GetBatchJobDetailsByReportCode(jobDetailReportRequestDto.JobReportCode, skip, jobDetailReportRequestDto.PageSize);
                if (Paginatedresult == null || Paginatedresult.JobDetailReports.Count == 0)
                {
                    _logger.LogError("{className}.{methodName}: ERROR - Job Detail Report Records not Found for  JobDetailReportCode : {JobDetailReportCode} , Error Code:{errorCode}", className, methodName, jobDetailReportRequestDto.JobReportCode, StatusCodes.Status404NotFound);
                    return new BatchJobDetailReportResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                }
                response.BatchJobDetails = _mapper.Map<List<BatchJobDetailReportDto>>(Paginatedresult.JobDetailReports);
                response.RecordCount = Paginatedresult.TotalRecords;
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Failed to get data - ERROR Msg:{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        public async Task<BatchJobDetailReportResponseDto> SaveJobDetailReport(BatchJobDetailReportRequestDto batchJobDetailReportRequestDto)
        {
            var response = new BatchJobDetailReportResponseDto();
            response.BatchJobDetails = new List<BatchJobDetailReportDto>();
            const string methodName = nameof(SaveJobDetailReport);
            try
            {
                if(batchJobDetailReportRequestDto.BatchJobDetailReportDtos == null || batchJobDetailReportRequestDto.BatchJobDetailReportDtos.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName} - No Data to save, batch Job detail Report",className, methodName);
                    return new BatchJobDetailReportResponseDto() { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "No Data to save, batch Job detail Report" };
                }

                foreach (var jobDetail in batchJobDetailReportRequestDto.BatchJobDetailReportDtos)
                {
                    var jobDetailRecordModel = _mapper.Map<BatchJobDetailReportModel>(jobDetail);
                    var result = await _batchJobDetailReportRepo.CreateAsync(jobDetailRecordModel);

                    if (result != null && result.BatchJobDetailReportId > 0)
                    {
                        _logger.LogInformation("{className}.{methodName}: successfully Saved data batch Job Detail Record, BatchJobDetailReportId:  {BatchJobDetailReportId}", className, methodName, result.BatchJobDetailReportId);
                        response.BatchJobDetails.Add(_mapper.Map<BatchJobDetailReportDto>(result));
                    }
                    else
                    {
                        _logger.LogError("{ClassName}.{MethodName} - Failed to saved data batch Job Detail Report, BatchJobDetailReportId:  {BatchJobDetailReportId}", className, methodName, batchJobDetailReportRequestDto.BatchJobDetailReportDtos?.FirstOrDefault()?.BatchJobDetailReportId ?? 0);
                        return new BatchJobDetailReportResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = $"Failed to saved data batch Job Detail Report, BatchJobDetailReportId:  {jobDetail.BatchJobDetailReportId}" };
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Save Failed - ERROR Msg:{msg}", className, methodName, ex.Message);
                throw;
            }
        }
    }
}
