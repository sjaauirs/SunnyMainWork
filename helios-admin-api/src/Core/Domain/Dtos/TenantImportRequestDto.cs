using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class TenantImportRequestDto
    {
        [Required]
        public string tenantCode { get; set; } = string.Empty;
        public string? CustomerCode { get; set; } = string.Empty;
        public string? SponsorCode { get; set; } = string.Empty;

        [JsonIgnore]
        public IList<string> ImportOptions {  get; set; }
        public string ImportOptionsString { get; set; }
        [Required]
        public IFormFile File { get; set; }

    }
}
