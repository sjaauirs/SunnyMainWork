namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IRedshiftDataService
    {
        /// <summary>
        /// Fetches data from a Redshift table based on a date column filter.
        /// </summary>
        /// <param name="tableName">Name of the Redshift table.</param>
        /// <param name="dateColumn">Column used to filter by date.</param>
        /// <param name="dateRangeStart">Start date in yyyy-MM-dd format.</param>
        /// <param name="dateRangeEnd">End date in yyyy-MM-dd format.</param>
        /// <param name="delimiter">delimiter eg. comma, tab</param>
        /// <returns>Delimited string of records.</returns>
        Task<string> FetchDataAsync(string tableName, string dateColumn, string dateRangeStart, string dateRangeEnd, string delimiter, string databaseName, string dateFormat, bool shouldAppendTotalRowCount);
    }
}
