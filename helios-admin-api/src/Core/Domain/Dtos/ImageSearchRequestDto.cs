using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ImageSearchRequestDto
    {
      
        [Required]
        public required string Base64Image { get; set; }
    }
}
