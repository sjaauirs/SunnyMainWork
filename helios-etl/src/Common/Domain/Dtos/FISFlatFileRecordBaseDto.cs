using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Common.Domain.Dtos
{
    public class FISFlatFileRecordBaseDto
    {
        public int RecordType { get; private set; } = 0;

        public FISFlatFileRecordBaseDto(int recordType)
        {
            RecordType = recordType;
        }
    }
}
