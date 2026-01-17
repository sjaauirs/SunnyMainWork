using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Services
{
    public class ConsumerFlowProgressServiceTest
    {
        private readonly Mock<ILogger<ConsumerFlowProgressService>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IConsumerFlowProgressRepo> _progressRepoMock = new();
        private readonly Mock<IConsumerOnboardingProgressHistoryRepo> _historyRepoMock = new();
        private readonly Mock<ISession> _sessionMock = new();

        private ConsumerFlowProgressService CreateService() =>
            new ConsumerFlowProgressService(
                _loggerMock.Object,
                _mapperMock.Object,
                _progressRepoMock.Object,
                _historyRepoMock.Object,
                _sessionMock.Object);

        private UpdateFlowStatusRequestDto GetRequestDto(
            string status = "Success",
            long? toFlowStepId = 2,
            long flowId = 1,
            int versionId = 1,
            long fromFlowStepId = 1)
        {
            return new UpdateFlowStatusRequestDto
            {
                ConsumerCode = "C123",
                TenantCode = "T456",
                CohortCode = "CO789",
                FlowId = flowId,
                VersionId = versionId,
                Status = status,
                ToFlowStepId = toFlowStepId,
                FromFlowStepId = fromFlowStepId
            };
        }

        [Fact]
        public async Task UpdateOnboardingStatusFlow_UpdatesExistingProgress_ReturnsMappedDto()
        {
            var request = GetRequestDto();
            var model = new ConsumerFlowProgressModel
            {
                Pk = 10,
                ConsumerCode = request.ConsumerCode,
                TenantCode = request.TenantCode,
                CohortCode = request.CohortCode,
                FlowFk = request.FlowId,
                VersionNbr = request.VersionId,
                FlowStepPk = request.ToFlowStepId ?? 0,
                Status = "Started",
                DeleteNbr = 0
            };
            _progressRepoMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerFlowProgressModel, bool>>>(), false))
                .ReturnsAsync(model);

            _progressRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ConsumerFlowProgressModel>()))
                .ReturnsAsync(model);
            _historyRepoMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerOnboardingProgressHistoryModel, bool>>>(), false))
                .ReturnsAsync((ConsumerOnboardingProgressHistoryModel)null);
            
            var mappedDto = new ConsumerFlowProgressDto { Pk = model.Pk, ConsumerCode = model.ConsumerCode, TenantCode = model.TenantCode };
            _mapperMock.Setup(m => m.Map<ConsumerFlowProgressDto>(It.IsAny<ConsumerFlowProgressModel>()))
                .Returns(mappedDto);

            var service = CreateService();
            var result = await service.UpdateOnboardingStatusFlow(request);

            Assert.NotNull(result.ConsumerFlowProgress);
            Assert.Equal(model.Pk, result.ConsumerFlowProgress.Pk);
            Assert.Equal(model.ConsumerCode, result.ConsumerFlowProgress.ConsumerCode);
        }

        [Fact]
        public async Task UpdateOnboardingStatusFlow_CreatesNewProgress_ReturnsMappedDto()
        {
            var request = GetRequestDto();
            _progressRepoMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerFlowProgressModel, bool>>>(), false))
                .ReturnsAsync((ConsumerFlowProgressModel)null);
            
            _historyRepoMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerOnboardingProgressHistoryModel, bool>>>(), false))
                .ReturnsAsync((ConsumerOnboardingProgressHistoryModel)null);

            var model = new ConsumerFlowProgressModel
            {
                Pk = 20,
                ConsumerCode = request.ConsumerCode,
                TenantCode = request.TenantCode,
                CohortCode = request.CohortCode,
                FlowFk = request.FlowId,
                VersionNbr = request.VersionId,
                FlowStepPk = request.ToFlowStepId ?? 0,
                Status = "Started",
                DeleteNbr = 0
            };
            _mapperMock.Setup(m => m.Map<ConsumerFlowProgressDto>(It.IsAny<ConsumerFlowProgressModel>()))
                .Returns(new ConsumerFlowProgressDto { Pk = model.Pk, ConsumerCode = model.ConsumerCode, TenantCode = model.TenantCode });

            var service = CreateService();
            var result = await service.UpdateOnboardingStatusFlow(request);

            Assert.NotNull(result.ConsumerFlowProgress);
            Assert.Equal(model.ConsumerCode, result.ConsumerFlowProgress.ConsumerCode);
        }

        [Fact]
        public async Task UpdateOnboardingStatusFlow_ExistingProgress_SameStatusAndStep_ReturnsEarly()
        {
            var request = GetRequestDto(status: "Started", toFlowStepId: 2);
            var model = new ConsumerFlowProgressModel
            {
                Pk = 30,
                ConsumerCode = request.ConsumerCode,
                TenantCode = request.TenantCode,
                CohortCode = request.CohortCode,
                FlowFk = request.FlowId,
                VersionNbr = request.VersionId,
                FlowStepPk = request.ToFlowStepId ?? 0,
                Status = request.Status,
                DeleteNbr = 0
            };
            _progressRepoMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerFlowProgressModel, bool>>>(), false))
                .ReturnsAsync(model);
            _mapperMock.Setup(m => m.Map<ConsumerFlowProgressDto>(It.IsAny<ConsumerFlowProgressModel>()))
                .Returns(new ConsumerFlowProgressDto { Pk = model.Pk, ConsumerCode = model.ConsumerCode, TenantCode = model.TenantCode });

            var service = CreateService();
            var result = await service.UpdateOnboardingStatusFlow(request);

            Assert.NotNull(result.ConsumerFlowProgress);
            Assert.Equal(model.Pk, result.ConsumerFlowProgress.Pk);
        }

        [Fact]
        public async Task UpdateOnboardingStatusFlow_ThrowsException_ReturnsErrorResponse()
        {
            var request = GetRequestDto();
            _progressRepoMock.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<ConsumerFlowProgressModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Test error"));

            var service = CreateService();
            var result = await service.UpdateOnboardingStatusFlow(request);

            Assert.NotNull(result);
            Assert.Equal("Test error", result.ErrorMessage);
        }
    }
}