using SunnyRewards.Helios.ETL.Common.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTaskCategoryModel : BaseModel
    {
        public virtual long TaskCategoryId { get; set; }  // task_category_id

        public virtual string TaskCategoryCode { get; set; } = string.Empty;  // task_category_code

        public virtual string? TaskCategoryDescription { get; set; }  

        public virtual string TaskCategoryName { get; set; }
    }
}
