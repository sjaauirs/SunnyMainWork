using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class ConsumerControllerTest
    {
        private readonly Mock<ILogger<ConsumerService>> _consumerServiceLogger;
        private readonly Mock<ILogger<ConsumerController>> _consumerControllerLogger;
        private readonly IConsumerService _consumerService;
        private readonly ConsumerController _consumerController;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IUserClient> _userClient;
        public ConsumerControllerTest()
        {
            _userClient = new Mock<IUserClient>();
            _consumerControllerLogger = new Mock<ILogger<ConsumerController>>();
            _consumerServiceLogger = new Mock<ILogger<ConsumerService>>();
            _mapper = new Mock<IMapper>();
            _consumerService = new ConsumerService(_consumerServiceLogger.Object, _userClient.Object, _mapper.Object);
            _consumerController = new ConsumerController(_consumerControllerLogger.Object, _consumerService);
        }

        [Fact]
        public async Task Update_Consumer_Should_Return_Ok_Response()
        {
            // Arrange 
            var requestDto = GetConsumerRequestDto();
            var consumerDto = new GetConsumerResponseMockDto().Consumer;
            var consumerId = 124567;
            _userClient.Setup(x => x.Put<ConsumerResponseDto>(It.IsAny<string>(), It.IsAny<ConsumerRequestDto>())).ReturnsAsync(new ConsumerResponseDto() { Consumer = consumerDto });

            // Act 
            var response = await _consumerController.UpdateConsumerAsync(consumerId, requestDto);

            // Assert
            Assert.NotNull(response);
            var okObjectResponse = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okObjectResponse.StatusCode);
        }
        [Fact]
        public async Task Update_Consumer_Should_Return_Not_Found_Response_When_Consumer_Is_Null()
        {
            // Arrange 
            var requestDto = GetConsumerRequestDto();
            var consumerId = 124567;
            _userClient.Setup(x => x.Put<ConsumerResponseDto>(It.IsAny<string>(), It.IsAny<ConsumerRequestDto>())).ReturnsAsync(new ConsumerResponseDto()
            {
                ErrorCode = StatusCodes.Status404NotFound,
            });

            // Act 
            var response = await _consumerController.UpdateConsumerAsync(consumerId, requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResponse = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResponse.StatusCode);
        }
        [Fact]
        public async Task Update_Consumer_Should_Return_Internal_Server_Error_Response_When_Exception_Occurs()
        {
            // Arrange 
            var requestDto = GetConsumerRequestDto();
            var consumerId = 124567;
            _userClient.Setup(x => x.Put<ConsumerResponseDto>(It.IsAny<string>(), It.IsAny<ConsumerRequestDto>())).Throws(new Exception("testing"));

            // Act 
            var response = await _consumerController.UpdateConsumerAsync(consumerId, requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResponse = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResponse.StatusCode);
        }
        [Fact]
        public async Task Update_Consumer_Should_Return_Internal_Server_Error_Response_When_Exception_Occurs_In_Controller()
        {
            // Arrange 
            var requestDto = GetConsumerRequestDto();
            var consumerId = 124567;
            var consumerDto = new GetConsumerResponseMockDto().Consumer;
            var _service = new Mock<IConsumerService>();
            var _controller = new ConsumerController(_consumerControllerLogger.Object, _service.Object);
            _userClient.Setup(x => x.Put<ConsumerResponseDto>(It.IsAny<string>(), It.IsAny<ConsumerRequestDto>())).ReturnsAsync(new ConsumerResponseDto() { Consumer = consumerDto });
            _service.Setup(x => x.UpdateConsumerAsync(It.IsAny<long>(), It.IsAny<ConsumerRequestDto>())).Throws<Exception>();

            // Act 
            var response = await _controller.UpdateConsumerAsync(consumerId, requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResponse = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResponse.StatusCode);
        }
        [Fact]
        public async Task DeactivateConsumer_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var request = new DeactivateConsumerRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "Consumer1"
            };

            var expectedResponse = new ConsumerResponseDto
            {
                ErrorCode = null,
                ErrorMessage = null,
                Consumer = new ConsumerDto
                {
                    TenantCode = "Tenant1",
                    ConsumerCode = "Consumer1",
                    EnrollmentStatus = EnrollmentStatus.DEACTIVATED.ToString()
                }
            };

            _userClient
                .Setup(client => client.Put<ConsumerResponseDto>(
                    It.Is<string>(url => url == CommonConstants.UpdateEnrollmentStatusAPIUrl),
                    It.IsAny<UpdateEnrollmentStatusRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _consumerController.DeactivateConsumer(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ConsumerResponseDto>(okResult.Value);

            Assert.Equal("Consumer1", response.Consumer.ConsumerCode);
            Assert.Equal(EnrollmentStatus.DEACTIVATED.ToString(), response.Consumer.EnrollmentStatus);
            Assert.Null(response.ErrorCode);
        }

        [Fact]
        public async Task DeactivateConsumer_ReturnsError_WhenUserClientFails()
        {
            // Arrange
            var request = new DeactivateConsumerRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "InvalidConsumer"
            };

            var errorResponse = new ConsumerResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Consumer not found"
            };

            _userClient
                .Setup(client => client.Put<ConsumerResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<UpdateEnrollmentStatusRequestDto>()))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _consumerController.DeactivateConsumer(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);

            var response = Assert.IsType<ConsumerResponseDto>(statusResult.Value);
            Assert.Equal("Consumer not found", response.ErrorMessage);
        }

        [Fact]
        public async Task DeactivateConsumer_Returns500_OnException()
        {
            // Arrange
            var request = new DeactivateConsumerRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "ErrorTrigger"
            };

            _userClient
                .Setup(client => client.Put<ConsumerResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<UpdateEnrollmentStatusRequestDto>()))
                .ThrowsAsync(new System.Exception("Internal failure"));

            // Act
            var result = await _consumerController.DeactivateConsumer(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);

            var response = Assert.IsType<ConsumerResponseDto>(statusResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("Internal failure", response.ErrorMessage);
        }

        [Fact]
        public async Task ReactivateConsumer_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var request = new ReactivateConsumerRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "Consumer1"
            };

            var expectedResponse = new ConsumerResponseDto
            {
                ErrorCode = null,
                ErrorMessage = null,
                Consumer = new ConsumerDto
                {
                    TenantCode = "Tenant1",
                    ConsumerCode = "Consumer1",
                    EnrollmentStatus = EnrollmentStatus.ENROLLED.ToString()
                }
            };

            _userClient
                .Setup(client => client.Put<ConsumerResponseDto>(
                    It.Is<string>(url => url == CommonConstants.UpdateEnrollmentStatusAPIUrl),
                    It.IsAny<UpdateEnrollmentStatusRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _consumerController.ReactivateConsumer(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ConsumerResponseDto>(okResult.Value);

            Assert.Equal("Consumer1", response.Consumer.ConsumerCode);
            Assert.Equal(EnrollmentStatus.ENROLLED.ToString(), response.Consumer.EnrollmentStatus);
            Assert.Null(response.ErrorCode);
        }

        [Fact]
        public async Task ReactivateConsumer_ReturnsError_WhenUserClientFails()
        {
            // Arrange
            var request = new ReactivateConsumerRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "InvalidConsumer"
            };

            var errorResponse = new ConsumerResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Consumer not found"
            };

            _userClient
                .Setup(client => client.Put<ConsumerResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<UpdateEnrollmentStatusRequestDto>()))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _consumerController.ReactivateConsumer(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);

            var response = Assert.IsType<ConsumerResponseDto>(statusResult.Value);
            Assert.Equal("Consumer not found", response.ErrorMessage);
        }

        [Fact]
        public async Task ReactivateConsumer_Returns500_OnException()
        {
            // Arrange
            var request = new ReactivateConsumerRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerCode = "ErrorTrigger"
            };

            _userClient
                .Setup(client => client.Put<ConsumerResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<UpdateEnrollmentStatusRequestDto>()))
                .ThrowsAsync(new Exception("Internal failure"));

            // Act
            var result = await _consumerController.ReactivateConsumer(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);

            var response = Assert.IsType<ConsumerResponseDto>(statusResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }


        private ConsumerRequestDto GetConsumerRequestDto()
        {
            return new ConsumerRequestDto()
            {
                ConsumerCode = "cmr-f67b1adbed33411dbe797eb300a83b0c",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerAttribute = "{ \"benefitsCardOptions\": { \"cardCreateOptions\": { \"deliveryMethod\": 11 } }, \"profileSettings\": { \"healthMetricsEnabled\": true } }"
            };
        }


        [Fact]
        public async Task ConsumerAttrinutes_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var request = new ConsumerAttributesRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerAttributes = new ConsumerAttributeDetailDto[]
                 {
                    new ConsumerAttributeDetailDto
                    {
                        ConsumerCode = "C001",
                        GroupName = "GroupA",
                        AttributeName = "Attribute1",
                        AttributeValue = "Value1"
                    },
                    new ConsumerAttributeDetailDto
                    {
                        ConsumerCode = "C001",
                        GroupName = "GroupA",
                        AttributeName = "Attribute2",
                        AttributeValue = "Value2"
                    }
                 }
            };


            var expectedResponse = new ConsumerAttributesResponseDto
            {
                ErrorCode = null,
                ErrorMessage = null,
                Consumers = new List<ConsumerDto>
                {

                    new ConsumerDto
                    {
                        TenantCode = "Tenant1",
                        ConsumerCode = "C001",
                        EnrollmentStatus = EnrollmentStatus.ENROLLED.ToString(),
                        ConsumerAttribute = "{ \"GroupA\": { \"Attribute1\": \"Value1\", \"Attribute2\": \"Value2\" } }"
                    }
                }
            };

            _userClient
                .Setup(client => client.Post<ConsumerAttributesResponseDto>(
                    It.Is<string>(url => url == CommonConstants.ConsumerAttributersUrl),
                    It.IsAny<ConsumerAttributesRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _consumerController.ConsumerAttributes(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ConsumerAttributesResponseDto>(okResult.Value);

            Assert.Equal("C001", response.Consumers[0].ConsumerCode);
            Assert.Equal(EnrollmentStatus.ENROLLED.ToString(), response.Consumers[0].EnrollmentStatus);
            Assert.Null(response.ErrorCode);
        }

        [Fact]
        public async Task ConsumerAttrinutes_ReturnsError_WhenUserClientFails()
        {
            // Arrange
            var request = new ConsumerAttributesRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerAttributes = new ConsumerAttributeDetailDto[]
                {
                    new ConsumerAttributeDetailDto
                    {
                        ConsumerCode = "C001",
                        GroupName = "GroupA",
                        AttributeName = "Attribute1",
                        AttributeValue = "Value1"
                    },
                    new ConsumerAttributeDetailDto
                    {
                        ConsumerCode = "C001",
                        GroupName = "GroupA",
                        AttributeName = "Attribute2",
                        AttributeValue = "Value2"
                    }
                }
            };

            var errorResponse = new ConsumerAttributesResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Consumer not found"
            };

            _userClient
                .Setup(client => client.Post<ConsumerAttributesResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<ConsumerAttributesRequestDto>()))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _consumerController.ConsumerAttributes(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);

            var response = Assert.IsType<ConsumerAttributesResponseDto>(statusResult.Value);
            Assert.Equal("Consumer not found", response.ErrorMessage);
        }

        [Fact]
        public async Task ConsumerAttrinutes_Returns500_OnException()
        {
            // Arrange
            var request = new ConsumerAttributesRequestDto
            {
                TenantCode = "Tenant1",
                ConsumerAttributes = new ConsumerAttributeDetailDto[]
                {
                    new ConsumerAttributeDetailDto
                    {
                        ConsumerCode = "C001",
                        GroupName = "GroupA",
                        AttributeName = "Attribute1",
                        AttributeValue = "Value1"
                    },
                    new ConsumerAttributeDetailDto
                    {
                        ConsumerCode = "C001",
                        GroupName = "GroupA",
                        AttributeName = "Attribute2",
                        AttributeValue = "Value2"
                    }
                }
            };

            _userClient
                .Setup(client => client.Post<ConsumerAttributesResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<ConsumerAttributesRequestDto>()))
                .ThrowsAsync(new Exception("Internal failure"));

            // Act
            var result = await _consumerController.ConsumerAttributes(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);

            var response = Assert.IsType<ConsumerAttributesResponseDto>(statusResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public async Task UpdateConsumerSubscription_status_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = "HAP-TENANT-CODE",
                ConsumerCode = "C001",
                ConsumerSubscriptionStatuses = new ConsumerSubscriptionStatusDetailDto[]
                {
                    new ConsumerSubscriptionStatusDetailDto { Feature = "PickAPurse", Status = "subscribed"}
                }
            };

            var expectedResponse = new BaseResponseDto();

            _userClient
                .Setup(client => client.Post<BaseResponseDto>(
                    It.Is<string>(url => url == CommonConstants.ConsumerSubscriptionStatusUrl),
                    It.IsAny<ConsumerSubscriptionStatusRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
        }

        [Fact]
        public async Task UpdateConsumerSubscription_status_ReturnsError_WhenUserClientFails()
        {
            // Arrange
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = "HAP-TENANT-CODE",
                ConsumerCode = "C001",
                ConsumerSubscriptionStatuses = new ConsumerSubscriptionStatusDetailDto[]
                {
                    new ConsumerSubscriptionStatusDetailDto { Feature = "PickAPurse", Status = "subscribed"}
                }
            };

            var errorResponse = new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Consumer not found"
            };

            _userClient
                .Setup(client => client.Post<BaseResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<ConsumerSubscriptionStatusRequestDto>()))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);

            var response = Assert.IsType<BaseResponseDto>(statusResult.Value);
            Assert.Equal("Consumer not found", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerSubscription_status_Returns500_OnException()
        {
            // Arrange
            var request = new ConsumerSubscriptionStatusRequestDto
            {
                TenantCode = "HAP-TENANT-CODE",
                ConsumerCode = "C001",
                ConsumerSubscriptionStatuses = new ConsumerSubscriptionStatusDetailDto[]
                {
                    new ConsumerSubscriptionStatusDetailDto { Feature = "PickAPurse", Status = "subscribed"}
                }
            };

            _userClient
                .Setup(client => client.Post<BaseResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<ConsumerSubscriptionStatusRequestDto>()))
                .ThrowsAsync(new Exception("Internal failure"));

            // Act
            var result = await _consumerController.UpdateConsumerSubscriptionStatus(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);

            var response = Assert.IsType<BaseResponseDto>(statusResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }
    }
}
