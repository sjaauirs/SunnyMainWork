using Microsoft.Extensions.Logging;
using Moq;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class ConsumerPurseCohortAssignmentServiceTests
    {
        private readonly Mock<ILogger<ConsumerPurseCohortAssignmentService>> _loggerMock;
        private readonly Mock<IFisClient> _fisClientMock;
        private readonly Mock<IConsumerCohortHelper> _consumerCohortHelperMock;
        private readonly Mock<IConsumerPurseAssignmentService> _consumerPurseAssignmentServiceMock;
        private readonly ConsumerPurseCohortAssignmentService _service;

        public ConsumerPurseCohortAssignmentServiceTests()
        {
            _loggerMock = new Mock<ILogger<ConsumerPurseCohortAssignmentService>>();
            _fisClientMock = new Mock<IFisClient>();
            _consumerCohortHelperMock = new Mock<IConsumerCohortHelper>();


            _service = new ConsumerPurseCohortAssignmentService(
                _loggerMock.Object,
                _fisClientMock.Object,
                _consumerCohortHelperMock.Object,
                _consumerPurseAssignmentServiceMock.Object
            );
        }

        [Fact]
        public void ConsumerPurseCohortAssignmentAsync_InvalidInput_ReturnsFalse()
        {
            // Arrange
            var consumer = new ConsumerDto
            {
                TenantCode = "",
                ConsumerCode = "C123",
                PlanId = "P1"
            };

            // Act
            var result = _service.ConsumerPurseCohortAssignment(consumer);

            // Assert
            Assert.False(result);
            _loggerMock.VerifyLog(LogLevel.Error, Times.Once());
        }

        [Fact]
        public void ConsumerPurseCohortAssignmentAsync_NoMappings_ReturnsFalse()
        {
            // Arrange
            var consumer = new ConsumerDto { TenantCode = "T1", ConsumerCode = "C1", PlanId = "P1" };
            _fisClientMock
                .Setup(x => x.Get<GetPlanCohortPurseMappingResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetPlanCohortPurseMappingResponseDto
                {
                    PlanCohortPurseMappings = new List<PlanCohortPurseMappingDto>() // empty
                });

            // Act
            var result = _service.ConsumerPurseCohortAssignment(consumer);

            // Assert
            Assert.False(result);
            _loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
        }

        [Fact]
        public void ConsumerPurseCohortAssignmentAsync_NoCohortChanges_ReturnsFalse()
        {
            // Arrange
            var consumer = new ConsumerDto { TenantCode = "T1", ConsumerCode = "C1", PlanId = "P1" };
            var mapping = new PlanCohortPurseMappingDto { CohortName = "Cohort1", Ssbci = true };
            var cohortResp = new CohortsResponseDto
            {
                Cohorts = new List<CohortDto> { new CohortDto { CohortName = "Cohort1" } }
            };

            _fisClientMock
                .Setup(x => x.Get<GetPlanCohortPurseMappingResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetPlanCohortPurseMappingResponseDto
                {
                    PlanCohortPurseMappings = new List<PlanCohortPurseMappingDto> { mapping }
                });

            _consumerCohortHelperMock
                .Setup(x => x.GetConsumerCohorts(It.IsAny<ConsumerCohortsRequestDto>()))
                .ReturnsAsync(cohortResp);

            // Act
            var result = _service.ConsumerPurseCohortAssignment(consumer);

            // Assert
            Assert.False(result);
            _loggerMock.VerifyLog(LogLevel.Information, Times.AtLeastOnce());
        }

        [Fact]
        public void ConsumerPurseCohortAssignmentAsync_AddsCohort_ReturnsTrue()
        {
            // Arrange
            var consumer = new ConsumerDto { TenantCode = "T1", ConsumerCode = "C1", PlanId = "P1" };
            var mapping = new PlanCohortPurseMappingDto { CohortName = "NewCohort", Ssbci = true };
            var cohortResp = new CohortsResponseDto
            {
                Cohorts = new List<CohortDto>() // empty
            };

            _fisClientMock
                .Setup(x => x.Get<GetPlanCohortPurseMappingResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(new GetPlanCohortPurseMappingResponseDto
                {
                    PlanCohortPurseMappings = new List<PlanCohortPurseMappingDto> { mapping }
                });

            _consumerCohortHelperMock
                .Setup(x => x.GetConsumerCohorts(It.IsAny<ConsumerCohortsRequestDto>()))
                .ReturnsAsync(cohortResp);

            _consumerCohortHelperMock
                .Setup(x => x.AddConsumerCohort(It.IsAny<CohortConsumerRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = _service.ConsumerPurseCohortAssignment(consumer);

            // Assert
            Assert.True(result);
            _consumerCohortHelperMock.Verify(x => x.AddConsumerCohort(It.IsAny<CohortConsumerRequestDto>()), Times.Once);
        }

        [Fact]
        public void ConsumerPurseCohortAssignmentAsync_Exception_ReturnsFalse()
        {
            // Arrange
            var consumer = new ConsumerDto { TenantCode = "T1", ConsumerCode = "C1", PlanId = "P1" };

            _fisClientMock
                .Setup(x => x.Get<GetPlanCohortPurseMappingResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
                .ThrowsAsync(new Exception("Service failed"));

            // Act
            var result = _service.ConsumerPurseCohortAssignment(consumer);

            // Assert
            Assert.False(result);
            _loggerMock.VerifyLog(LogLevel.Error, Times.Once());
        }
    }

    // --- Helper to verify logs ---
    public static class LoggerExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times)
        {
            logger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                times);
        }
    }
}
