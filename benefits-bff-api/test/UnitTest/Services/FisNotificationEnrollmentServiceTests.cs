using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Fis.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Services
{
    public class FisNotificationEnrollmentServiceTests
    {
        private readonly Mock<ILogger<CardOperationService>> _loggerMock = new();
        private readonly Mock<IFisClient> _fisClientMock = new();
        private readonly Mock<IConsumerAccountService> _consumerAccountServiceMock = new();
        private readonly FisNotificationEnrollmentService _service;

        public FisNotificationEnrollmentServiceTests()
        {
            _service = new FisNotificationEnrollmentService(
                _loggerMock.Object,
                _fisClientMock.Object,
                _consumerAccountServiceMock.Object
            );
        }

        [Fact]
        public async Task GetNotificationsEnrollmentAsync_ReturnsMappedResponse_WhenNoError()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var fisApiResponse = new GetNotificationsEnrollmentResponseDto
            {
                FisResponse = new SunnyBenefits.Fis.Core.Domain.Fis.Dtos.ServicePayloadCardholderEnrollment
                {
                    CardholderEnrollmentData = new SunnyBenefits.Fis.Core.Domain.Fis.Dtos.CardholderEnrollmentData
                    {
                        EnrollmentUid = "UID123",
                        MessageData = new SunnyBenefits.Fis.Core.Domain.Fis.Dtos.MessageData
                        {
                            Messages = new List<SunnyBenefits.Fis.Core.Domain.Fis.Dtos.Message>
                            {
                                new SunnyBenefits.Fis.Core.Domain.Fis.Dtos.Message { MsgId = "1", MsgType = "TypeA", MsgDescription = "DescA" }
                            }
                        }
                    }
                }
            };
            _fisClientMock.Setup(f => f.Post<GetNotificationsEnrollmentResponseDto>(
                FisNotificationConstants.FisGetNotificationsEnrollmentApiUrl, request))
                .ReturnsAsync(fisApiResponse);

            // Act
            var result = await _service.GetNotificationsEnrollmentAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UID123", result.EnrollmentUid);
            Assert.Single(result.EnrolledNotifications);
            Assert.Equal("1", result.EnrolledNotifications[0].MessageId);
        }

        [Fact]
        public async Task GetNotificationsEnrollmentAsync_ReturnsError_WhenFisApiResponseHasError()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var fisApiResponse = new GetNotificationsEnrollmentResponseDto { ErrorCode = 123 };
            _fisClientMock.Setup(f => f.Post<GetNotificationsEnrollmentResponseDto>(
                FisNotificationConstants.FisGetNotificationsEnrollmentApiUrl, request))
                .ReturnsAsync(fisApiResponse);

            // Act
            var result = await _service.GetNotificationsEnrollmentAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fisApiResponse.ErrorCode, result.ErrorCode);
        }

        [Fact]
        public async Task GetNotificationsEnrollmentAsync_ThrowsInvalidOperationException_OnException()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            _fisClientMock.Setup(f => f.Post<GetNotificationsEnrollmentResponseDto>(
                FisNotificationConstants.FisGetNotificationsEnrollmentApiUrl, request))
                .ThrowsAsync(new Exception("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetNotificationsEnrollmentAsync(request));
        }

        [Fact]
        public async Task SetNotificationsEnrollmentAsync_CallsPut_WhenEnrollmentUidExists()
        {
            // Arrange
            var request = new FisSetEnrollNotificationsRequestDto
            {
                TenantCode = "T1",
                ConsumerCode = "C1",
                EnrolledNotifications = "EN"
            };
            var consumerAccountResponse = new GetConsumerAccountResponseDto
            {
                ConsumerAccount = new ConsumerAccountDto { NotificationsEnrollmentUid = "UID" }
            };
            _consumerAccountServiceMock.Setup(c => c.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(consumerAccountResponse);

            var enrollResponse = new EnrollNotificationsResponseDto();
            _fisClientMock.Setup(f => f.Put<EnrollNotificationsResponseDto>(
                FisNotificationConstants.FisNotificationsEnrollmentApiUrl, It.IsAny<UpdateNotificationsRequestDto>()))
                .ReturnsAsync(enrollResponse);

            // Act
            var result = await _service.SetNotificationsEnrollmentAsync(request);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task SetNotificationsEnrollmentAsync_CallsPost_WhenEnrollmentUidMissing()
        {
            // Arrange
            var request = new FisSetEnrollNotificationsRequestDto
            {
                TenantCode = "T1",
                ConsumerCode = "C1",
                EnrolledNotifications = "EN"
            };
            var consumerAccountResponse = new GetConsumerAccountResponseDto
            {
                ConsumerAccount = new ConsumerAccountDto { NotificationsEnrollmentUid = null }
            };
            _consumerAccountServiceMock.Setup(c => c.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(consumerAccountResponse);

            var enrollResponse = new EnrollNotificationsResponseDto();
            _fisClientMock.Setup(f => f.Post<EnrollNotificationsResponseDto>(
                FisNotificationConstants.FisNotificationsEnrollmentApiUrl, request))
                .ReturnsAsync(enrollResponse);

            // Act
            var result = await _service.SetNotificationsEnrollmentAsync(request);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task SetNotificationsEnrollmentAsync_ReturnsError_WhenErrorCodePresent()
        {
            // Arrange
            var request = new FisSetEnrollNotificationsRequestDto
            {
                TenantCode = "T1",
                ConsumerCode = "C1",
                EnrolledNotifications = "EN"
            };
            var consumerAccountResponse = new GetConsumerAccountResponseDto
            {
                ConsumerAccount = new ConsumerAccountDto { NotificationsEnrollmentUid = null }
            };
            _consumerAccountServiceMock.Setup(c => c.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(consumerAccountResponse);

            var enrollResponse = new EnrollNotificationsResponseDto { ErrorCode = 123, ErrorMessage = "fail" };
            _fisClientMock.Setup(f => f.Post<EnrollNotificationsResponseDto>(
                FisNotificationConstants.FisNotificationsEnrollmentApiUrl, request))
                .ReturnsAsync(enrollResponse);

            // Act
            var result = await _service.SetNotificationsEnrollmentAsync(request);

            // Assert
            Assert.Equal(123, result.ErrorCode);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task SetNotificationsEnrollmentAsync_ThrowsInvalidOperationException_OnException()
        {
            // Arrange
            var request = new FisSetEnrollNotificationsRequestDto
            {
                TenantCode = "T1",
                ConsumerCode = "C1",
                EnrolledNotifications = "EN"
            };
            _consumerAccountServiceMock.Setup(c => c.GetConsumerAccount(It.IsAny<GetConsumerAccountRequestDto>()))
                .ThrowsAsync(new Exception("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SetNotificationsEnrollmentAsync(request));
        }

        [Fact]
        public async Task GetClientConfigAsync_ReturnsResponse_WhenNoError()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new FisGetClientConfigResponseDto();
            _fisClientMock.Setup(f => f.Post<FisGetClientConfigResponseDto>(
                FisNotificationConstants.FisGetClientConfigApiUrl, request))
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetClientConfigAsync(request);

            // Assert
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task GetClientConfigAsync_ReturnsError_WhenErrorCodePresent()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            var response = new FisGetClientConfigResponseDto { ErrorCode = 123 };
            _fisClientMock.Setup(f => f.Post<FisGetClientConfigResponseDto>(
                FisNotificationConstants.FisGetClientConfigApiUrl, request))
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetClientConfigAsync(request);

            // Assert
            Assert.Equal(123, result.ErrorCode);
        }

        [Fact]
        public async Task GetClientConfigAsync_ThrowsInvalidOperationException_OnException()
        {
            // Arrange
            var request = new FisGetNotificationsEnrollmentRequestDto { TenantCode = "T1", ConsumerCode = "C1" };
            _fisClientMock.Setup(f => f.Post<FisGetClientConfigResponseDto>(
                FisNotificationConstants.FisGetClientConfigApiUrl, request))
                .ThrowsAsync(new Exception("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetClientConfigAsync(request));
        }
    }
}
