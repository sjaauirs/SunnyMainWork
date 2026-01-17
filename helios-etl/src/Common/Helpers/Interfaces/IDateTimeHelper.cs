namespace SunnyRewards.Helios.ETL.Common.Helpers.Interfaces
{
    public interface IDateTimeHelper
    {
        /// <summary>
        /// GetUtcDateTime
        /// </summary>
        /// <param name="date"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        DateTime GetUtcDateTime(string date, string timeZone);

        /// <summary>
        /// Get Datetime of specific zone based on UtcOffset
        /// </summary>
        /// <param name="utcOffset"></param>
        /// <returns></returns>
        DateTime GetUtcOffsetDateTime(string utcOffset);
    }
}
