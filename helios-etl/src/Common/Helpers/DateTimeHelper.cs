using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;

namespace SunnyRewards.Helios.ETL.Common.Helpers
{
    public class DateTimeHelper : IDateTimeHelper
    {
        private const string className = nameof(DateTimeHelper);
        private readonly ILogger<DateTimeHelper> _logger;

        public DateTimeHelper(ILogger<DateTimeHelper> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// GetUtcDateTime
        /// </summary>
        /// <param name="date"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        /// 
        public DateTime GetUtcDateTime(string date, string timeZone)
        {
            try
            {
                // Parse the cutoff date as a DateTime object
                DateTime localMidnight = DateTime.ParseExact(date, "yyyy-MM-dd", null).Date;

                // Find the specified time zone (e.g., "Eastern Standard Time")
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

                // Convert the local midnight time to a DateTimeOffset in the given time zone
                DateTimeOffset localMidnightWithOffset = new(localMidnight, TimeSpan.Zero);
                DateTimeOffset tzMidnight = TimeZoneInfo.ConvertTimeToUtc(localMidnightWithOffset.DateTime, timeZoneInfo);

                // Convert to UTC
                DateTime utcMidnight = tzMidnight.UtcDateTime;

                return utcMidnight;
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Invalid date format: {Date}. Expected format is yyyy-MM-dd.", date);
                throw;
            }
            catch (TimeZoneNotFoundException ex)
            {
                _logger.LogError(ex, "Time zone not found: {TimeZone}.", timeZone);
                throw;
            }
            catch (InvalidTimeZoneException ex)
            {
                _logger.LogError(ex, "Invalid time zone: {TimeZone}.", timeZone);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while converting {Date} to UTC for time zone {TimeZone}.", date, timeZone);
                throw;
            }
        }


        /// <summary>
        /// Get Datetime of specific zone based on UtcOffset
        /// </summary>
        /// <param name="utcOffset"></param>
        /// <returns></returns>
        public DateTime GetUtcOffsetDateTime(string utcOffset)
        {
            const string methodName = nameof(GetUtcOffsetDateTime);
            try
            {
                // A simple mapping of UTC offsets to TimeZoneInfo IDs (this can be extended)
                string timeZoneId = GetTimeZoneId(utcOffset);
                // Get the TimeZoneInfo object for the corresponding timezone ID
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                Console.WriteLine($"TimeZoneInfo for the UTCOffset: {utcOffset} is: {timeZone.ToJson()}");

                // Get the current UTC time
                DateTime utcNow = DateTime.UtcNow;

                // Convert the UTC time to the specified timezone
                DateTime specificTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);

                _logger.LogInformation($"{className}:{methodName}: Current date and time in {timeZone.DisplayName}: {specificTime}");
                return specificTime;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{className}:{methodName}: Error fetching datetime for offset {utcOffset}. \n Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get TimeZoneId based on UTCOffset
        /// </summary>
        /// <param name="utcOffset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private string GetTimeZoneId(string utcOffset)
        {
            string timeZoneId = utcOffset switch
            {
                "UTC-12:00" => "Dateline Standard Time",
                "UTC-11:00" => "UTC-11",
                "UTC-10:00" => "Hawaiian Standard Time",
                "UTC-09:30" => "Marquesas Time",
                "UTC-09:00" => "Alaskan Standard Time",
                "UTC-08:30" => "No common time zone", // No common time zone
                "UTC-08:00" => "Pacific Standard Time",
                "UTC-07:30" => "No common time zone", // No common time zone
                "UTC-07:00" => "Mountain Standard Time",
                "UTC-06:30" => "No common time zone", // No common time zone
                "UTC-06:00" => "Central Standard Time",
                "UTC-05:30" => "India Standard Time",
                "UTC-05:00" => "Eastern Standard Time",
                "UTC-04:30" => "Venezuela Time",
                "UTC-04:00" => "Atlantic Standard Time",
                "UTC-03:30" => "Newfoundland Standard Time",
                "UTC-03:00" => "Argentina Standard Time",
                "UTC-02:30" => "No common time zone", // No common time zone
                "UTC-02:00" => "Azores Standard Time",
                "UTC-01:00" => "Cape Verde Standard Time",
                "UTC+00:00" => "GMT Standard Time",
                "UTC+01:00" => "W. Europe Standard Time",
                "UTC+02:00" => "E. Europe Standard Time",
                "UTC+03:00" => "Russian Standard Time",
                "UTC+03:30" => "Iran Standard Time",
                "UTC+04:00" => "Azerbaijan Standard Time",
                "UTC+04:30" => "Afghanistan Time",
                "UTC+05:00" => "Pakistan Standard Time",
                "UTC+05:30" => "Asia/Kolkata",
                "UTC+05:45" => "Nepal Time",
                "UTC+06:00" => "Central Asia Standard Time",
                "UTC+06:30" => "Cocos Islands Time",
                "UTC+07:00" => "SE Asia Standard Time",
                "UTC+08:00" => "China Standard Time",
                "UTC+08:45" => "Australia Western Standard Time",
                "UTC+09:00" => "Tokyo Standard Time",
                "UTC+09:30" => "Australian Central Standard Time",
                "UTC+10:00" => "AUS Eastern Standard Time",
                "UTC+10:30" => "Lord Howe Standard Time",
                "UTC+11:00" => "Solomon Islands Time",
                "UTC+11:30" => "Norfolk Island Time",
                "UTC+12:00" => "Fiji Standard Time",
                "UTC+12:45" => "Chatham Islands Time",
                "UTC+13:00" => "Phoenix Island Time",
                "UTC+14:00" => "Line Islands Time",
                _ => throw new ArgumentException("Unsupported UTC offset")
            };
            return timeZoneId;
        }
    }
}
