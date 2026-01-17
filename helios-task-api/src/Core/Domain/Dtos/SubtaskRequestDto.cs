using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class SubtaskRequestDto
    {
        [Required]
        public required string ParentTaskRewardCode { get; set; }
        [Required]
        public required string ChildTaskRewardCode { get; set; }
        [Required]
        public required PostSubTaskDto Subtask { get; set; }
    }
    public class PostSubTaskDto
    {
        public long SubTaskId { get; set; }
        public long ParentTaskRewardId { get; set; }
        public long ChildTaskRewardId { get; set; }
        public string? ConfigJson { get; set; }
        [Required]

        public required string CreateUser { get; set; }
    }
}
