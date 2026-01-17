using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class PreRunValidationJson
    {
        public class PreRun
        {
            public List<TenantData> PerTenantData { get; set; }
            public CrossTenantData CrossTenantData { get; set; }
            public InvalidRecords InvalidRecords { get; set; }

            public PreRun()
            {
                PerTenantData = new List<TenantData> ();
                InvalidRecords = new InvalidRecords();
                CrossTenantData = new CrossTenantData();
            }
        }
        public class InvalidRecords
        {
            public List<int> InvalidAddRecords { get; set; } = new List<int>();
            public List<int> InvalidUpdateRecords { get; set; } = new List<int>();
            public List<int> InvalidCancelRecords { get; set; } = new List<int>();
            public List<int> InvalidDeleteRecords { get; set; }= new List<int>();
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

        // Represents the count data for total, valid, and invalid records
        public class Counts
        {
            public int TotalRecords { get; set; }
            public int TotalAdd { get; set; }
            public int TotalUpdate { get; set; }
            public int TotalCancel { get; set; }
            public int TotalDelete { get; set; }
            public int ValidAdd { get; set; }
            public int ValidUpdate { get; set; }
            public int ValidCancel { get; set; }
            public int ValidDelete { get; set; }

        
        }
    }
}
