using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class PostRunValidationJson
    {
        public class PostRun
        {
            public List<TenantData> PerTenantData { get; set; }
            public CrossTenantData CrossTenantData { get; set; }

            public PostRun()
            {
                PerTenantData = new List<TenantData>();
                CrossTenantData= new CrossTenantData();
            }
        }

        // Represents data per tenant
        public class TenantData
        {
            public string TenantCode { get; set; }
            public Counts Counts { get; set; }

            public TenantData()
            {
                Counts = new Counts();
            }
        }

        // Represents cross-tenant aggregated data
        public class CrossTenantData
        {
            public Counts Counts { get; set; }

            public CrossTenantData()
            {
                Counts = new Counts();
            }
        }

        // Represents the count data for processed and successful operations
        public class Counts
        {
            public int ProcessedAdd { get; set; }
            public int ProcessedUpdate { get; set; }
            public int ProcessedCancel { get; set; }
            public int ProcessedDelete { get; set; }
            public int SuccessfulAdd { get; set; }
            public int SuccessfulUpdate { get; set; }
            public int SuccessfulCancel { get; set; }
            public int SuccessfulDelete { get; set; }

          
        }

    }
}
