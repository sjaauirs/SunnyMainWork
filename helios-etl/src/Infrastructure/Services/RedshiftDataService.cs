using Microsoft.Extensions.Logging;
using Npgsql;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Enums;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{

    public class RedshiftDataService : IRedshiftDataService
    {
        private readonly string _redshiftConnectionString;
        private readonly ILogger<RedshiftDataService> _logger;
        public RedshiftDataService(ILogger<RedshiftDataService> logger, ISecretHelper secretHelper)
        {
            _logger = logger;
            _redshiftConnectionString =  secretHelper.GetRedshiftConnectionString().Result;

        }
        public async Task<string> FetchDataAsync(string tableName, string dateColumn, string dateRangeStart, string dateRangeEnd, string delimiter, string databaseName, string dateFormat, bool shouldAppendTotalRowCount)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));

            if (string.IsNullOrWhiteSpace(dateColumn))
                throw new ArgumentException("Date column cannot be null or empty.", nameof(dateColumn));

            string delimiterChar = GetDelimiterCharacter(delimiter);

            const string fullDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            const string dateOnlyFormat = "yyyy-MM-dd";

            if (!DateTime.TryParseExact(dateRangeStart, new[] { fullDateTimeFormat, dateOnlyFormat }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                throw new ArgumentException($"Start date must be in '{fullDateTimeFormat}' or '{dateOnlyFormat}' format.", nameof(dateRangeStart));

            if (DateTime.TryParseExact(dateRangeEnd, fullDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate)) { }
            else if (DateTime.TryParseExact(dateRangeEnd, dateOnlyFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
            {
                endDate = endDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            else
            {
                throw new ArgumentException($"End date must be in '{fullDateTimeFormat}' or '{dateOnlyFormat}' format.", nameof(dateRangeEnd));
            }

            string query = $"SELECT * FROM {tableName} WHERE {dateColumn} BETWEEN @DateStart AND @DateEnd";
            _logger.LogInformation("Executing Redshift query: {Query}", query);

            try
            {
               var redshiftConnectionStr = OverrideDatabaseInConnectionString(_redshiftConnectionString, databaseName);

                await using var connection = new NpgsqlConnection(redshiftConnectionStr);
                await connection.OpenAsync();
                _logger.LogInformation("Redshift connection opened successfully.");

                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@DateStart", startDate.ToString(dateOnlyFormat));
                command.Parameters.AddWithValue("@DateEnd", endDate.ToString(fullDateTimeFormat));

                await using var reader = await command.ExecuteReaderAsync();
                var resultBuilder = new StringBuilder();
                int rowCount = 0;

                // Write column headers in uppercase
                var columnNames = Enumerable.Range(0, reader.FieldCount)
                                            .Select(i => reader.GetName(i).ToUpperInvariant());
                resultBuilder.AppendLine(string.Join(delimiterChar, columnNames));

                // Write data rows
                while (await reader.ReadAsync())
                {
                    var row = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (string.Equals(reader.GetName(i), dateColumn, StringComparison.OrdinalIgnoreCase) &&
                            reader[i] != DBNull.Value)
                        {
                            // Format date column as MM/DD/YYYY
                            if (DateTime.TryParse(reader[i].ToString(), out var parsedDate))
                            {
                                row[i] = parsedDate.ToString(dateFormat, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                row[i] = reader[i].ToString() ?? string.Empty;
                            }
                        }
                        else
                        {
                            row[i] = reader[i]?.ToString() ?? string.Empty;
                        }
                    }
                    resultBuilder.AppendLine(string.Join(delimiterChar, row));
                    rowCount++;
                }

                if (shouldAppendTotalRowCount)
                {
                    resultBuilder.AppendLine($"Total Rows{delimiterChar}{rowCount}");
                }

                _logger.LogInformation("Fetched {RowCount} rows from Redshift table {TableName}.", rowCount, tableName);
                return resultBuilder.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from Redshift for table {TableName}", tableName);
                throw;
            }
        }

        private string GetDelimiterCharacter(string delimiter)
        {
            if (!Enum.TryParse<DelimiterType>(delimiter, true, out var parsed))
                throw new ArgumentException("Invalid delimiter. Allowed values: Comma, Tab, Pipe, Semicolon.");

            return parsed switch
            {
                DelimiterType.Comma => ",",
                DelimiterType.Tab => "\t",
                DelimiterType.Pipe => "|",
                DelimiterType.Semicolon => ";",
                _ => ","
            };
        }
        public static string OverrideDatabaseInConnectionString(string originalConnectionString, string newDatabaseName)
        {
            if (string.IsNullOrWhiteSpace(newDatabaseName))
                return originalConnectionString;

            var builder = new Npgsql.NpgsqlConnectionStringBuilder(originalConnectionString)
            {
                Database = newDatabaseName
            };

            return builder.ToString();
        }

    }
}
