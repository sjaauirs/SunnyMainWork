using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class JobReportService : IJobReportService
    {
        public readonly ILogger<JobReportService> _logger;
        private readonly IBatchJobReportRepo _batchJobReportRepo;
        private readonly IMapper _mapper;
        public const string className = nameof(JobReportService);


        public JobReportService(ILogger<JobReportService> logger,IBatchJobReportRepo batchJobReportRepo ,
            IMapper mapper )
        {
            _logger = logger;
            _mapper = mapper;
            _batchJobReportRepo = batchJobReportRepo; 
        }

        public async Task<GetBatchJobReportResponseDto> GetJobReports(JobReportRequestDto jobReportRequestDto)
        {
            const string methodName = nameof(GetJobReports);
            try
            {
                IList<BatchJobReportModel>? batchJobReportModels = null;
                int recordCount = 0;
                _logger.LogInformation("{ClassName}.{MethodName}: Get Job Report started for JobReportCode : {JobReportCode}, Search : {Search}", className, methodName, jobReportRequestDto.JobReportCode, jobReportRequestDto.SearchByJobName);

                if (String.IsNullOrWhiteSpace(jobReportRequestDto.SearchByJobName) && String.IsNullOrWhiteSpace(jobReportRequestDto.JobReportCode))
                {
                    //Resturn all data when no search and JobReportCode provided
                    var paginatedResult = await PaginatedSearchByJobType(jobReportRequestDto);

                    return new GetBatchJobReportResponseDto()
                    {
                        jobReports = _mapper.Map<List<BatchJobReportDto>>(paginatedResult?.JobReports),
                        RecordCount = paginatedResult!.TotalRecords
                    };
                }

                if (!string.IsNullOrWhiteSpace(jobReportRequestDto.JobReportCode))
                {
                    batchJobReportModels = await _batchJobReportRepo.FindAsync(x => x.BatchJobReportCode == jobReportRequestDto.JobReportCode.Trim()
                                                                                    && x.DeleteNbr == 0);
                    recordCount = batchJobReportModels.Count;
                }

                // If no specific job report was found by code or if no code was provided, perform a paginated search
                if (batchJobReportModels == null || batchJobReportModels.Count == 0)
                {
                    var paginatedResult = await PaginatedSearchByJobType(jobReportRequestDto);
                    if (paginatedResult != null)
                    {
                        batchJobReportModels = paginatedResult.JobReports;
                        recordCount = paginatedResult.TotalRecords;
                    }
                }

                if (batchJobReportModels == null || batchJobReportModels.Count == 0)
                {
                    _logger.LogError("{className}.{methodName}: ERROR - Job Report Records not Found for  JobReportCode : {JobReportCode}, Search : {Search} , Error Code:{errorCode}", className, methodName, jobReportRequestDto.JobReportCode , jobReportRequestDto.SearchByJobName, StatusCodes.Status404NotFound);
                    return new GetBatchJobReportResponseDto() { ErrorCode = StatusCodes.Status404NotFound };
                }

                return new GetBatchJobReportResponseDto() {
                    jobReports = _mapper.Map<List<BatchJobReportDto>>(batchJobReportModels),
                     RecordCount = recordCount
                };                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: Failed to get data - ERROR Msg:{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        private async Task<PaginatedBatchJobReport> PaginatedSearchByJobType(JobReportRequestDto jobReportRequestDto)
        {

                var skip = (jobReportRequestDto.PageNumber - 1) * jobReportRequestDto.PageSize;
                return await _batchJobReportRepo.GetPaginatedJobReport(jobReportRequestDto.SearchByJobName??"", skip, jobReportRequestDto.PageSize);

        }
        public async Task<BatchJobReportResponseDto> SaveJobReport(BatchJobReportDto batchJobReportDto)
        {
            var response = new BatchJobReportResponseDto();
            const string methodName = nameof(SaveJobReport);
            try
            {
                var jobReportModel = _mapper.Map<BatchJobReportModel>(batchJobReportDto);
                var result = await _batchJobReportRepo.CreateAsync(jobReportModel);
                if (result!= null && result.BatchJobReportId > 0)
                {
                    _logger.LogInformation("{className}.{methodName}: successfully saved data batch Job Report, BatchJobReportCode:  {BatchJobReportCode}", className, methodName, result.BatchJobReportCode);
                    response.jobReport = _mapper.Map<BatchJobReportDto>(result);
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName} - Failed to saved data batch Job Report, BatchJobReportCode:  {BatchJobReportCode}", className, methodName, batchJobReportDto.BatchJobReportCode);
                    return new BatchJobReportResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = $"Failed to saved data batch Job Report, BatchJobReportCode:  {batchJobReportDto.BatchJobReportCode}" };
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
