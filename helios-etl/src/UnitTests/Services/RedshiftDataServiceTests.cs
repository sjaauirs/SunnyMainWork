using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using System.Reflection;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class RedshiftDataServiceTests
    {
        private readonly Mock<ISecretHelper> _mockSecretHelper;
        private readonly Mock<ILogger<RedshiftDataService>> _mockLogger;
        private readonly RedshiftDataService _service;

        public RedshiftDataServiceTests()
        {
            _mockSecretHelper = new Mock<ISecretHelper>();
            _mockLogger = new Mock<ILogger<RedshiftDataService>>();
            _mockSecretHelper.Setup(s => s.GetRedshiftConnectionString())
                             .ReturnsAsync("Host=localhost;Username=test;Password=test;Database=test");

            _service = new RedshiftDataService(_mockLogger.Object, _mockSecretHelper.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async System.Threading.Tasks.Task FetchDataAsync_InvalidTableName_ThrowsException(string tableName)
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.FetchDataAsync(tableName, "created_at", "2024-01-01", "2024-01-02", "Comma","etl_staging", "yyyy-MM-dd", true));

            Assert.Equal("Table name cannot be null or empty. (Parameter 'tableName')", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async System.Threading.Tasks.Task FetchDataAsync_InvalidDateColumn_ThrowsException(string dateColumn)
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.FetchDataAsync("my_table", dateColumn, "2024-01-01", "2024-01-02", "Comma", "etl_staging", "yyyy-MM-dd", true));

            Assert.Equal("Date column cannot be null or empty. (Parameter 'dateColumn')", ex.Message);
        }

        [Theory]
        [InlineData("invalid-date")]
        [InlineData("2024/31/12")]
        public async System.Threading.Tasks.Task FetchDataAsync_InvalidStartDateFormat_ThrowsException(string startDate)
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.FetchDataAsync("my_table", "created_at", startDate, "2024-01-01", "Comma", "etl_staging", "yyyy-MM-dd", true));

            Assert.Contains("Start date must be", ex.Message);
        }

        [Theory]
        [InlineData("notadate")]
        [InlineData("2024-13-01")]
        public async System.Threading.Tasks.Task FetchDataAsync_InvalidEndDateFormat_ThrowsException(string endDate)
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.FetchDataAsync("my_table", "created_at", "2024-01-01", endDate, "Comma", "etl_staging", "yyyy-MM-dd", true));

            Assert.Contains("End date must be", ex.Message);
        }

        [Fact]
        public void GetDelimiterCharacter_Invalid_ThrowsException()
        {
            var method = typeof(RedshiftDataService)
                .GetMethod("GetDelimiterCharacter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var ex = Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(_service, new object[] { "InvalidDelimiter" }));

            Assert.IsType<ArgumentException>(ex.InnerException);
        }

    }
}
