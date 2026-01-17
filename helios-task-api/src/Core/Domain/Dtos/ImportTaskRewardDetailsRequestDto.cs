using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SunnyRewards.Helios.Task.Core.Domain.Dtos.TaskRewardDto;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ImportTaskRewardDetailsRequestDto : BaseResponseDto
    {
        /// <summary>
        /// 
        /// </summary>
        public ImportTaskRewardDetailsRequestDto()
        {
            TaskRewardDetails = new List<ImportTaskRewardDetailDto>();
            TenantCode = string.Empty;
            SubTasks = new List<SubTaskDto>();
            TenantTaskCategory = new List<ExportTenantTaskCategoryDto>();
            TaskExternalMappings = new List<TaskExternalMappingDto>();
            TermsOfServices = new List<TermsOfServiceDto>();
        }
        public string TenantCode { get; set; }
        public List<SubTaskDto>? SubTasks { get; set; }
        public List<ExportTenantTaskCategoryDto>? TenantTaskCategory { get; set; }
        public List<TaskExternalMappingDto>? TaskExternalMappings { get; set; }
        public List<ImportTaskRewardDetailDto> TaskRewardDetails { get; set; }
        public List<TermsOfServiceDto> TermsOfServices { get; set; }
    }
    public class ImportTaskRewardDetailDto
    {
        public ImportTaskRewardDetailDto()
        {
            Task = new ExportTaskDto();
        }
        public ExportTaskDto Task { get; set; }
        public ExportTaskRewardDto? TaskReward { get; set; }
        public List<TaskDetailDto>? TaskDetail { get; set; }
    }
}
