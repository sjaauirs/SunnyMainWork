using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SunnyRewards.Helios.ETL.Core.Domain.Dtos.PreRunValidationJson;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class PerTenantConsumerCount
    {
        public List<PerTenantPreConsumerCountData> TenantPreConsumerPreRunData { get; set; }
        public List<PerTenantPostConsumerCountData> TenantPreConsumerPostRunData { get; set; }
        public PerTenantConsumerCount()
        {
            TenantPreConsumerPreRunData = new List<PerTenantPreConsumerCountData>();
            TenantPreConsumerPostRunData = new List<PerTenantPostConsumerCountData>();          
        }
    }

    public class PerTenantPreConsumerCountData
    {
        public PerTenantPreConsumerData PerTenantConsumerCount { get; set; }

        public string TenantCode { get; set; }=string.Empty;
        public PerTenantPreConsumerCountData()
        {
            PerTenantConsumerCount = new PerTenantPreConsumerData();
        }
    }
    public class PerTenantPreConsumerData
    {
        public int TotalConsumerCount { get; set; }
        public int RemovedConsumerCount { get; set; }
    }
    public class PerTenantPostConsumerCountData
    {
        public PerTenantPostConsumerData PerTenantConsumerCount { get; set; }

        public string TenantCode { get; set; }=string.Empty;
        public PerTenantPostConsumerCountData()
        {
            PerTenantConsumerCount = new PerTenantPostConsumerData();
        }
    }
    public class PerTenantPostConsumerData
    {
        public int TotalConsumerCount { get; set; }
        public int RemovedConsumerCount { get; set; }
        public int UpdateConsumerCount { get; set; }
    }
}
