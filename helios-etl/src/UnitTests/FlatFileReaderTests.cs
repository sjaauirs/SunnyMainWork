using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Common.Helpers;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;

namespace SunnyRewards.Helios.ETL.UnitTests
{
    public class FlatFileReaderTests
    {
        [Fact]
        public void ReadFlatFileData()
        {
            IFlatFileReader reader = new FlatFileReader();

            // record from sample file
            string record = "D|1217824|Sunny UAT Test|872817|Sunny UAT Test|404962|USD|840|BANKFIRST|************3143|************3143|0|410040|100.0000|05082024 04:36:44|1|840|USD|1|0|1710|0|Null|Null|05082024|Null|Null|Null|MANUAL BATCH LOADER|Null|Null|Null|100.0000|05082024 08:36:46|12|Null|Null|Null|0|Null|Null|Null|Null|6902031683149|0|Null|Null|Null|Null|Null|Null|Null|Null|Null|Null|0|Null|Null|Null|12100|Completed OK|Value Load|Approval|CC00C59C649D|0|Manual batch loader|?|ValueLoad|UATA2ANations1|Golla|Kiran|05082024 04:36:46|test|543453t555|Null|1710|Value Load|Null|6902031683149|1D1B7AF6-9DBA-41A7-89BB-2D2C5A50D98B|OTC2100|ACTIVE|05082024|01012022|12312030|05082024|Null|117|Value Load - Manual|0|005555|Null|Null|Null|Null|Null|Null|Null|100.0000|100.0000|05082024 04:36:46|05082024 08:36:46|05082024 08:36:46|Null|Null||Null|Null|Null|Null|Null|\r\n";

            var fisMonetoryDetailDtoObj = reader.ReadFlatFileRecord<FISMonetoryDetailDto>(record, '|');

            Assert.NotNull(fisMonetoryDetailDtoObj);
            Assert.Equal('D', fisMonetoryDetailDtoObj.RecordType);
            Assert.Null(fisMonetoryDetailDtoObj.DeviceType);
        }
    }
}