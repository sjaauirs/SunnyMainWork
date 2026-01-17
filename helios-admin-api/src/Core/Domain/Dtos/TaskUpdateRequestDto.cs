using System.ComponentModel;
using Microsoft.AspNetCore.Http;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class TaskUpdateRequestDto : BaseRequestDto
    {   
        public long TaskId { get; set; }
        public string? TaskStatus { get; set; }
        public string? TaskCode { get; set; }
        public string? PartnerCode { get; set; }
        public string? MemberId { get; set; }
        public DateTime? TaskCompletedTs { get; set; }
        public string? TaskName { get; set; }
        public IFormFile? TaskCompletionEvidenceDocument {  get; set; }
        public bool? SupportLiveTransferToRewardsPurse { get; set; } = false;

        [DefaultValue(false)]
        public bool IsAutoEnrollEnabled { get; set; } = false;
        public string? ImageType { get; set; }
        public string? ImageName { get; set; }
        public IFormFile? Image { get; set; }  // Currently we are supporting only images

        [DefaultValue(false)]
        public bool SkipValidation { get; set; } = false;
    }
}