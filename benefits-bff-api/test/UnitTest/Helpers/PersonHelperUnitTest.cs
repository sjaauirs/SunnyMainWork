using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Infrastructure.Helpers;
using Sunny.Benefits.Bff.Infrastructure.HttpClients;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Helpers
{
    public class PersonHelperUnitTest
    {
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<ILogger<PersonHelper>> _personHelperLogger;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly PersonHelper _personHelper;
        private readonly Mock<IAdminClient> _adminClient;
        private readonly Mock<ITaskClient> _taskClient;


        public PersonHelperUnitTest()
        {
            _userClient = new Mock<IUserClient>();
            _personHelperLogger = new Mock<ILogger<PersonHelper>>();
            _tenantClient = new TenantClientMock();
            _adminClient = new Mock<IAdminClient>();
            _taskClient = new Mock<ITaskClient>();
            _personHelper = new PersonHelper(_personHelperLogger.Object, _userClient.Object, _tenantClient.Object, _adminClient.Object , _taskClient.Object);
        }

        [Fact]
        public async Task ValidatePersonIsVerified_PersonIsNull_ReturnsFalse()
        {
            // Arrange
            var consumerRequest = new GetConsumerRequestDto { ConsumerCode = "12345" };
            _userClient.Setup(client => client.Post<GetPersonAndConsumerResponseDto>("person/get-details-by-consumer-code", consumerRequest));

            // Act
            var result = await _personHelper.ValidatePersonIsVerified(consumerRequest);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidatePersonIsVerified_PersonNotVerified_ReturnsFalse()
        {
            // Arrange
            var consumerRequest = new GetConsumerRequestDto { ConsumerCode = "12345" };
            var response = new GetPersonAndConsumerResponseDto
            {
                Person = new PersonDto
                {
                    PersonId = 1,
                    OnBoardingState = OnboardingState.DOB_VERIFIED.ToString()
                }
            };
            _userClient.Setup(client => client.Post<GetPersonAndConsumerResponseDto>("person/get-details-by-consumer-code", consumerRequest))
                .ReturnsAsync(response);

            // Act
            var result = await _personHelper.ValidatePersonIsVerified(consumerRequest);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidatePersonIsVerified_PersonVerified_ReturnsTrue()
        {
            // Arrange
            var consumerRequest = new GetConsumerRequestDto { ConsumerCode = "12345" };
            var response = new GetPersonAndConsumerResponseDto
            {
                Person = new PersonDto
                {
                    PersonId = 1
                },
                Consumer = new ConsumerDto { OnBoardingState = OnboardingState.VERIFIED.ToString() }
            };
            _userClient.Setup(client => client.Post<GetPersonAndConsumerResponseDto>("person/get-details-by-consumer-code", consumerRequest))
                .ReturnsAsync(response);

            // Act
            var result = await _personHelper.ValidatePersonIsVerified(consumerRequest);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidatePersonIsVerified_PersonVerified_When_GetTenant_Return_Error()
        {
            // Arrange
            var consumerRequest = new GetConsumerRequestDto { ConsumerCode = "12345" };
            var response = new GetPersonAndConsumerResponseDto
            {
                Person = new PersonDto
                {
                    PersonId = 1
                },
                Consumer = new ConsumerDto { OnBoardingState = OnboardingState.VERIFIED.ToString() }
            };
            _userClient.Setup(client => client.Post<GetPersonAndConsumerResponseDto>("person/get-details-by-consumer-code", consumerRequest))
                .ReturnsAsync(response);
            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto());

            // Act
            var result = await _personHelper.ValidatePersonIsVerified(consumerRequest);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidatePersonIsVerified_PersonVerified_When_OnboardingDisabled()
        {
            // Arrange
            var consumerRequest = new GetConsumerRequestDto { ConsumerCode = "12345" };
            var response = new GetPersonAndConsumerResponseDto
            {
                Person = new PersonDto
                {
                    PersonId = 1
                },
                Consumer = new ConsumerDto { OnBoardingState = OnboardingState.VERIFIED.ToString() }
            };
            _userClient.Setup(client => client.Post<GetPersonAndConsumerResponseDto>("person/get-details-by-consumer-code", consumerRequest))
                .ReturnsAsync(response);
            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto()
                {
                    TenantCode = "123",
                    TenantOption = "{\r\n  \"apps\": [\r\n    \"REWARDS\",\r\n    \"BENEFITS\"\r\n  ],\r\n  \"benefitsOptions\": {\r\n    \"disableOnboardingFlow\": true\r\n  }\r\n}"
                });

            // Act
            var result = await _personHelper.ValidatePersonIsVerified(consumerRequest);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetPersonDetails_Should_Return_PersonAndConsumerDetails()
        {
            // Arrange
            var getConsumerRequestDto = new GetConsumerRequestDto { ConsumerCode = "cmr-123445643608ebdeecc18b75602" };

            var getPersonAndConsumerResponseDto = new GetPersonAndConsumerResponseDto()
            {
                Consumer = new ConsumerDto { ConsumerCode = "cmr-123445643608ebdeecc18b75602" },
                Person = new PersonDto { OnBoardingState = "CARD_LAST_4_VERIFIED" }
            };

            _userClient.Setup(client => client.Post<GetPersonAndConsumerResponseDto>("person/get-details-by-consumer-code", getConsumerRequestDto))
                .ReturnsAsync(getPersonAndConsumerResponseDto);
            // Act
            var result = await _personHelper.GetPersonDetails(getConsumerRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(getPersonAndConsumerResponseDto.Person, result.Person);
            Assert.Equal(getPersonAndConsumerResponseDto.Consumer, result.Consumer);

            _userClient.Verify(client => client.Post<GetPersonAndConsumerResponseDto>("person/get-details-by-consumer-code", getConsumerRequestDto), Times.Once);
        }

        [Fact]
        public async Task UpdateOnBoardingState_ReturnsTrue_WhenOnboardingStateUpdatedSuccessfully()
        {
            // Arrange
            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "Consumer123",
                OnboardingState = OnboardingState.EMAIL_VERIFIED
            };

            var consumerResponse = new ConsumerResponseDto
            {
                Consumer = new ConsumerDto
                {
                    PersonId = 1,
                    ConsumerCode = "Consumer123",
                    OnBoardingState = OnboardingState.EMAIL_VERIFIED.ToString()
                }
            };

            _userClient
                .Setup(x => x.Patch<ConsumerResponseDto>("consumer", updateOnboardingStateDto))
                .ReturnsAsync(consumerResponse);

            // Act
            var result = await _personHelper.UpdateOnBoardingState(updateOnboardingStateDto);

            // Assert
            Assert.True(result);
           
        }

        [Fact]
        public async Task UpdateOnBoardingState_ReturnsFalse_WhenConsumerResponseIsNull()
        {
            // Arrange
            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "Consumer123",
                OnboardingState = OnboardingState.EMAIL_VERIFIED
            };

            var consumerResponse = new ConsumerResponseDto
            {
                Consumer = null
            };

            _userClient
                .Setup(x => x.Patch<ConsumerResponseDto>("consumer", updateOnboardingStateDto))
                .ReturnsAsync(consumerResponse);

            // Act
            var result = await _personHelper.UpdateOnBoardingState(updateOnboardingStateDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingState_ReturnsFalse_WhenPersonIdIsInvalid()
        {
            // Arrange
            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "Consumer123",
                OnboardingState = OnboardingState.EMAIL_VERIFIED
            };

            var consumerResponse = new ConsumerResponseDto
            {
                Consumer = new ConsumerDto
                {
                    PersonId = 0,
                    ConsumerCode = "Consumer123",
                    OnBoardingState = OnboardingState.EMAIL_VERIFIED.ToString()
                }
            };

            _userClient
                .Setup(x => x.Patch<ConsumerResponseDto>("consumer", updateOnboardingStateDto))
                .ReturnsAsync(consumerResponse);

            // Act
            var result = await _personHelper.UpdateOnBoardingState(updateOnboardingStateDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingState_ReturnsFalse_WhenOnBoardingStateDoesNotMatch()
        {
            // Arrange
            var updateOnboardingStateDto = new UpdateOnboardingStateDto
            {
                ConsumerCode = "Consumer123",
                OnboardingState = OnboardingState.EMAIL_VERIFIED
            };

            var consumerResponse = new ConsumerResponseDto
            {
                Consumer = new ConsumerDto
                {
                    PersonId = 1,
                    ConsumerCode = "Consumer123",
                    OnBoardingState = OnboardingState.CARD_LAST_4_VERIFIED.ToString()
                }
            };

            _userClient
                .Setup(x => x.Patch<ConsumerResponseDto>("consumer", updateOnboardingStateDto))
                .ReturnsAsync(consumerResponse);

            // Act
            var result = await _personHelper.UpdateOnBoardingState(updateOnboardingStateDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingTask_TenantIsNull_ReturnsFalse()
        {
            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()));

            var consumerDto = new ConsumerDto { TenantCode = "TEN001", ConsumerCode = "CON001" };

            var result = await _personHelper.UpdateOnBoardingTask(consumerDto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingTask_InvalidJsonInTenantOption_ReturnsFalse()
        {
            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto()
                {
                    TenantCode = "123",
                    TenantOption = "INVALID_JSON"
                });

            var consumerDto = new ConsumerDto { TenantCode = "TEN001", ConsumerCode = "CON001" };

            var result = await _personHelper.UpdateOnBoardingTask(consumerDto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingTask_BenefitsOptionsIsNull_ReturnsFalse()
        {
            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()))
            .ReturnsAsync(new TenantDto()
            {
                TenantCode = "123",
                TenantOption = "{\r\n  \"apps\": [\r\n    \"REWARDS\",\r\n    \"BENEFITS\"\r\n  ],\r\n  }"
            });


            var consumerDto = new ConsumerDto { TenantCode = "TEN001", ConsumerCode = "CON001" };

            var result = await _personHelper.UpdateOnBoardingTask(consumerDto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingTask_AutoCompleteTaskIsDisabled_ReturnsFalse()
        {

            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()))
    .ReturnsAsync(new TenantDto()
    {
        TenantCode = "123",
        TenantOption = "{\r\n  \"apps\": [\r\n    \"REWARDS\",\r\n    \"BENEFITS\"\r\n  ],\r\n  \"benefitsOptions\": {\r\n    \"disableOnboardingFlow\": true,\r\n    \"manualCardRequestRequired\": true,\r\n    \"cardIssueFlowType\": [\r\n      {\r\n        \"flowType\": \"DIGITAL\",\r\n        \"cohortCode\": [\"C001\", \"C002\"]\r\n      },\r\n      {\r\n        \"flowType\": \"PHYSICAL\",\r\n        \"cohortCode\": [\"C003\"]\r\n      }\r\n    ],\r\n    \"taskCompletionCheckCode\": [\"TASK1\", \"TASK2\"],\r\n    \"autoCompleteTaskOnLogin\": false\r\n  }\r\n}"

    });

            var consumerDto = new ConsumerDto { TenantCode = "TEN001", ConsumerCode = "CON001" };

            var result = await _personHelper.UpdateOnBoardingTask(consumerDto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingTask_TaskCompletionCheckCodeIsEmpty_ReturnsFalse()
        {
            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()))
   .ReturnsAsync(new TenantDto()
   {
       TenantCode = "123",
       TenantOption = @"{
                  ""apps"": [
                    ""REWARDS"",
                    ""BENEFITS""
                  ],
                  ""benefitsOptions"": {
                    ""disableOnboardingFlow"": true,
                    ""autoCompleteTaskOnLogin"": true,
                    ""taskCompletionCheckCode"": []
                  }
                }"
   });

            var consumerDto = new ConsumerDto { TenantCode = "TEN001", ConsumerCode = "CON001" };

            var result = await _personHelper.UpdateOnBoardingTask(consumerDto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingTask_TaskRewardDetailsEmpty_ReturnsFalse()
        {
            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()))
   .ReturnsAsync(new TenantDto()
   {
       TenantCode = "123",
       TenantOption = @"{
                  ""apps"": [
                    ""REWARDS"",
                    ""BENEFITS""
                  ],
                  ""benefitsOptions"": {
                    ""disableOnboardingFlow"": true,
                    ""autoCompleteTaskOnLogin"": true,
                    ""taskCompletionCheckCode"": [""trw-3432423""]
                  }
                }"
   });

            _taskClient.Setup(tc => tc.Post<GetTaskRewardResponseDto>("task/get-by-task-reward-code", It.IsAny<object>()))
                       .ReturnsAsync(new GetTaskRewardResponseDto
                       {
                           TaskRewardDetails = new List<TaskRewardDetailDto>()
                       });

            var consumerDto = new ConsumerDto { TenantCode = "TEN001", ConsumerCode = "CON001" };

            var result = await _personHelper.UpdateOnBoardingTask(consumerDto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingTask_UpdateResponseHasErrorCode_ReturnsFalse()
        {
            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()))
   .ReturnsAsync(new TenantDto()
   {
       TenantCode = "123",
       TenantOption = @"{
                  ""apps"": [
                    ""REWARDS"",
                    ""BENEFITS""
                  ],
                  ""benefitsOptions"": {
                    ""disableOnboardingFlow"": true,
                    ""autoCompleteTaskOnLogin"": true,
                    ""taskCompletionCheckCode"": [""trw-3432423""]
                  }
                }"
   });

            _taskClient.Setup(tc => tc.Post<GetTaskRewardResponseDto>("task/get-by-task-reward-code", It.IsAny<object>()))
                       .ReturnsAsync(new GetTaskRewardResponseDto
                       {
                           TaskRewardDetails = new List<TaskRewardDetailDto>
                           {
                           new TaskRewardDetailDto { Task = new TaskDto { TaskId = 123 } }
                           }
                       });

            _adminClient.Setup(ac => ac.Post<ConsumerTaskUpdateResponseDto>("consumer/task-update", It.IsAny<object>()))
                        .ReturnsAsync(new ConsumerTaskUpdateResponseDto
                        {
                            ErrorCode = 500
                        });

            var consumerDto = new ConsumerDto { TenantCode = "TEN001", ConsumerCode = "CON001" };

            var result = await _personHelper.UpdateOnBoardingTask(consumerDto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingTask_ThrowException()
        {
            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()))
                .ThrowsAsync(new Exception("test exception"));

            var consumerDto = new ConsumerDto { TenantCode = "TEN001", ConsumerCode = "CON001" };

            var result = await _personHelper.UpdateOnBoardingTask(consumerDto);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOnBoardingTask_AllValid_ReturnsTrue()
        {
            _tenantClient.Setup(client => client.Post<TenantDto>(CommonConstants.GetTenantByCodeAPIUrl, It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto()
                {
                    TenantCode = "123",
                    TenantOption = @"{
                              ""apps"": [
                                ""REWARDS"",
                                ""BENEFITS""
                              ],
                              ""benefitsOptions"": {
                                ""disableOnboardingFlow"": true,
                                ""autoCompleteTaskOnLogin"": true,
                                ""taskCompletionCheckCode"": [""trw-3432423""]
                              }
                            }"
                });

            _taskClient.Setup(tc => tc.Post<GetTaskRewardResponseDto>("task/get-by-task-reward-code", It.IsAny<object>()))
                       .ReturnsAsync(new GetTaskRewardResponseDto
                       {
                           TaskRewardDetails = new List<TaskRewardDetailDto>
                           {
                           new TaskRewardDetailDto { Task = new TaskDto { TaskId = 123 } }
                           }
                       });

            _adminClient.Setup(ac => ac.Post<ConsumerTaskUpdateResponseDto>("consumer/task-update", It.IsAny<object>()))
                        .ReturnsAsync(new ConsumerTaskUpdateResponseDto
                        {
                            ConsumerTask = new ConsumerTaskDto { ConsumerTaskId = 32 },
                            ErrorCode = null
                        });

            _adminClient.Setup(x => x.PostFormData<ConsumerTaskUpdateResponseDto>(
                    "admin/consumer/task-update",
                    It.IsAny<TaskUpdateRequestDto>()))
                .ReturnsAsync(new ConsumerTaskUpdateResponseDto
                {
                    ErrorCode = null,
                    ConsumerTask = new ConsumerTaskDto()
                    {
                        ConsumerTaskId = 32
                    }
                });

            var consumerDto = new ConsumerDto { TenantCode = "TEN001", ConsumerCode = "CON001" };

            var result = await _personHelper.UpdateOnBoardingTask(consumerDto);

            Assert.True(result);
        }
    }
}
