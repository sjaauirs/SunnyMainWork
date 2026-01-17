using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.ETL.Common.Helpers;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;

namespace SunnyRewards.Helios.ETL.UnitTests.Helpers
{
    public class DateTimeHelperTests
    {
        private readonly Mock<ILogger<DateTimeHelper>> _mockLogger;
        private readonly IDateTimeHelper _dateTimeHelper;

        public DateTimeHelperTests()
        {
            _mockLogger = new Mock<ILogger<DateTimeHelper>>();
            _dateTimeHelper = new DateTimeHelper(_mockLogger.Object);
        }

        [Fact]
        public void GetUtcMidnight_ValidInput_ReturnsExpectedUtcDateTime()
        {
            // Arrange
            string date = "2024-10-01";
            string timeZone = "Eastern Standard Time"; // EST

            // Act
            DateTime result = _dateTimeHelper.GetUtcDateTime(date, timeZone);

            // Assert
            DateTime expected = new DateTime(2024, 10, 01, 04, 00, 00); // 12:00 AM EST is 4:00 AM UTC
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetUtcDateTime_InvalidDateFormat_ThrowsFormatException()
        {
            // Arrange
            string invalidDate = "08-10-2024"; // Wrong format, should be yyyy-MM-dd
            string timeZone = "Eastern Standard Time";

            // Act & Assert
            Assert.Throws<FormatException>(() => _dateTimeHelper.GetUtcDateTime(invalidDate, timeZone));
        }

        [Fact]
        public void GetUtcDateTime_InvalidTimeZone_ThrowsTimeZoneNotFoundException()
        {
            // Arrange
            string date = "2024-10-08";
            string invalidTimeZone = "Invalid Time Zone"; // A different kind of invalid zone

            // Act & Assert
            Assert.Throws<TimeZoneNotFoundException>(() => _dateTimeHelper.GetUtcDateTime(date, invalidTimeZone));
        }
    }
}

