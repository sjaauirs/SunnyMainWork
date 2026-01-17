using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SunnyRewards.Helios.ETL.Core.Domain.Dtos.PostRunValidationJson;
using static SunnyRewards.Helios.ETL.Core.Domain.Dtos.PreRunValidationJson;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class BatchJobReportValidationJson
    {
        public PreRun? PreRun { get; set; }
        public PostRun? PostRun { get; set; }



        public string SetValidationJsonDetails(BatchJobReportValidationJson details)
        {

            return JsonSerializer.Serialize(details);

        }
    }
}

