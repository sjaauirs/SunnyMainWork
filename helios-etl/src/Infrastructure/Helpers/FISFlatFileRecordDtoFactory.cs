using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Constants.FIS;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Helpers
{
    public class FISFlatFileRecordDtoFactory : IFISFlatFileRecordDtoFactory
    {
        public FISFlatFileRecordBaseDto CreateFisRecordInstance(string record)
        {
            int recordType = int.Parse(record.Substring(0, 2));
            switch (recordType)
            {
                case FISBatchConstants.RECORD_TYPE_CARD_HOLDER_DATA:
                    return new FISCardHolderDataDto();

                case FISBatchConstants.RECORD_TYPE_FILE_HEADER:
                    return new FISFileHeaderDto();

                case FISBatchConstants.RECORD_TYPE_BATCH_HEADER:
                    return new FISBatchHeaderDto();

                case FISBatchConstants.RECORD_TYPE_BATCH_TRAILER:
                    return new FISBatchTrailerDto();

                case FISBatchConstants.RECORD_TYPE_FILE_TRAILER:
                    return new FISFileTrailerDto();

                case FISBatchConstants.RECORD_TYPE_CARD_LOAD_DATA:
                    return new FISCardDisbursementRecordDto();

                case FISBatchConstants.RECORD_TYPE_CARD_LOAD_ADDITIONAL_DATA:
                    return new FISCardAdditionalDisbursementRecordDto();

                default:
                    return new FISFlatFileRecordBaseDto(0);

            }
        }
    }
}
