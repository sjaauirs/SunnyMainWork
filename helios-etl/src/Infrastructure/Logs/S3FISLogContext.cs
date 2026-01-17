using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Logs
{
    public class S3FISLogContext: S3LogContext
    {
        public string? LogFileName { get; set; }
        public bool throwEtlError = true;
    }
}
