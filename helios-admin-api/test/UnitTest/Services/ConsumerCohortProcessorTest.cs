using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate.Mapping;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Reflection;
using Xunit;
using TaskA = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class ConsumerCohortEventProcessorTests
    {
        private readonly Mock<ILogger<ConsumerCohortEventProcessor>> _logger = new();
        private readonly Mock<IUserClient> _userClient = new();
        private readonly Mock<ICohortClient> _cohortClient = new();
        private readonly Mock<IConsumerCohortRuleProcessor> _ruleProcessor = new();
        private readonly ConsumerCohortEventProcessor processor;
        private readonly Mock<IConsumerPurseCohortAssignmentService> _consumerPurseCohortAssignmentService = new();

        public ConsumerCohortEventProcessorTests()
        {
            processor = new ConsumerCohortEventProcessor(_logger.Object, _userClient.Object, _cohortClient.Object, _ruleProcessor.Object);
        }

        private void ArrangeUser()
        {
            _userClient
                            .Setup(c => c.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                            .ReturnsAsync(new GetConsumerResponseDto
                            {
                                Consumer = new ConsumerDto { ConsumerCode = "C-1", PersonId = 101, TenantCode = "TEN-1" }
                            });

            _userClient
                .Setup(c => c.GetById<PersonDto>("person/", 101))
                .ReturnsAsync(new PersonDto { FirstName = "John", LastName = "Doe" });
        }

        [Fact]
        public async TaskA ProcessEvent_ConsumerNotFound_ReturnsFalse()
        {
            var eventRequest = GetEventDto();

            _userClient
                .Setup(c => c.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseDto { Consumer = null });

            var ok = await processor.ProcessEvent(eventRequest);

            Assert.False(ok);
        }

        private EventDto<CohortEventDto> GetEventDto()
        {
            var eventDto = new EventDto<CohortEventDto>()
            {
                Header = new EventHeaderDto
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventType = "COHORT_EVENT",
                    EventSubtype = "COHORT_ASSIGNMENT",
                    PublishTs = DateTime.UtcNow,
                    TenantCode = "tem-010101",
                    ConsumerCode = "con-11111",
                    SourceModule = "UserService"
                },
                Data = new CohortEventDto()
                {

                    EventId = "req-etrte",
                    TenantCode = "ten-3432",
                    TriggeredBy = "TEST",
                    ConsumerCode = "con-11111",

                }
            };
            return eventDto;
        }

        [Fact]
        public async TaskA ProcessEvent_personNotFound_ReturnsFalse()
        {

            var eventRequest = GetEventDto();

            _userClient
                             .Setup(c => c.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                             .ReturnsAsync(new GetConsumerResponseDto
                             {
                                 Consumer = new ConsumerDto { ConsumerCode = "C-1", PersonId = 101, TenantCode = "TEN-1" }
                             });

            _userClient
                .Setup(c => c.GetById<PersonDto>("person/", 101));

            var ok = await processor.ProcessEvent(eventRequest);

            Assert.False(ok);
        }

        [Fact]
        public async TaskA ProcessEvent_NoCohortForTenantNotFound_ReturnsFalse()
        {

            ArrangeUser();
            var eventRequest = GetEventDto();

            _cohortClient
                .Setup(c => c.Post<TenantCohortResponseDto>("cohort/get-tenant-cohorts", It.IsAny<TenantCohortRequestDto>()));

            var ok = await processor.ProcessEvent(eventRequest);

            Assert.False(ok);
        }

        [Fact]
        public async TaskA ProcessEvent_counsumer_Serialization()
        {
            var eventRequest = GetEventDto();
            ArrangeUser();

            _cohortClient
                .Setup(c => c.Post<CohortsResponseDto>("consumer-cohorts", It.IsAny<ConsumerCohortsRequestDto>()))
                .ReturnsAsync(new CohortsResponseDto());

            _ruleProcessor
                .Setup(r => r.EvaluateRule(It.IsAny<string>(), It.IsAny<CohortRuleArrayJson>(), It.IsAny<CohortRuleInput>()))
                .ReturnsAsync(new CohortRuleExecutionDto { RulesExecutionResult = false });

            AddTenantCohort();

            var ok = await processor.ProcessEvent(eventRequest);

            Assert.True(ok);
            _userClient.Verify(c => c.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()), Times.Once);
            _userClient.Verify(c => c.GetById<PersonDto>("person/", 101), Times.Once);
            _cohortClient.Verify(c => c.Post<CohortsResponseDto>("consumer-cohorts", It.IsAny<ConsumerCohortsRequestDto>()), Times.Once);
            _cohortClient.Verify(c => c.Post<BaseResponseDto>("add-consumer", It.IsAny<CohortConsumerRequestDto>()), Times.Never);
            _cohortClient.Verify(c => c.Post<BaseResponseDto>("remove-consumer", It.IsAny<CohortConsumerRequestDto>()), Times.Never);
        }


        [Fact]
        public async TaskA ProcessEvent_counsumer_notINCohort_nither_cohart_Added_Nor_Removed()
        {
            var eventRequest = GetEventDto();
            ArrangeUser();

            _cohortClient
                .Setup(c => c.Post<CohortsResponseDto>("consumer-cohorts", It.IsAny<ConsumerCohortsRequestDto>()))
                .ReturnsAsync(new CohortsResponseDto());

            _ruleProcessor
                .Setup(r => r.EvaluateRule(It.IsAny<string>(), It.IsAny<CohortRuleArrayJson>(), It.IsAny<CohortRuleInput>()))
                .ReturnsAsync(new CohortRuleExecutionDto { RulesExecutionResult = false });

            AddTenantCohort();

            var ok = await processor.ProcessEvent(eventRequest);

            Assert.True(ok);
            _userClient.Verify(c => c.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()), Times.Once);
            _userClient.Verify(c => c.GetById<PersonDto>("person/", 101), Times.Once);
            _cohortClient.Verify(c => c.Post<CohortsResponseDto>("consumer-cohorts", It.IsAny<ConsumerCohortsRequestDto>()), Times.Once);
            _cohortClient.Verify(c => c.Post<BaseResponseDto>("add-consumer", It.IsAny<CohortConsumerRequestDto>()), Times.Never);
            _cohortClient.Verify(c => c.Post<BaseResponseDto>("remove-consumer", It.IsAny<CohortConsumerRequestDto>()), Times.Never);
        }

        private void AddTenantCohort()
        {
            
            var cohortRule = new CohortRuleArrayJson
            {
                RuleExpr = new List<string>
                {
                    "person.City.ToLower() == \"delhi\""
                },
                SuccessExpr = "true"
            };

            _cohortClient
                .Setup(c => c.Post<TenantCohortResponseDto>("cohort/get-tenant-cohorts", It.IsAny<TenantCohortRequestDto>()))
                .ReturnsAsync(new TenantCohortResponseDto
                {
                    Cohort = new List<CohortDto>
                    {
                        new CohortDto
                        {
                            CohortCode = "c1",
                            CohortName = "C1",
                            CohortRule = cohortRule.ToJson()
                        }
                    }
                });
        }

        [Fact]
        public async TaskA ProcessEvent_RulePasses_ConsumerNotInCohort_AddCalled_ReturnsTrueOnSuccess()
        {

            ArrangeUser();
            AddTenantCohort();

            var eventRequest = GetEventDto();
            _cohortClient
               .Setup(c => c.Post<CohortsResponseDto>("consumer-cohorts", It.IsAny<ConsumerCohortsRequestDto>()))
               .ReturnsAsync(new CohortsResponseDto());

            _ruleProcessor
                .Setup(r => r.EvaluateRule(It.IsAny<string>(), It.IsAny<CohortRuleArrayJson>(), It.IsAny<CohortRuleInput>()))
                .ReturnsAsync(new CohortRuleExecutionDto { RulesExecutionResult = true });

            _cohortClient
                .Setup(c => c.Post<BaseResponseDto>("add-consumer", It.IsAny<CohortConsumerRequestDto>()))
                .ReturnsAsync(new BaseResponseDto()); // success

            var ok = await processor.ProcessEvent(eventRequest);

            Assert.True(ok);
            _userClient.Verify(c => c.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()), Times.Once);
            _userClient.Verify(c => c.GetById<PersonDto>("person/", 101), Times.Once);
            _cohortClient.Verify(c => c.Post<CohortsResponseDto>("consumer-cohorts", It.IsAny<ConsumerCohortsRequestDto>()), Times.Once);
            _cohortClient.Verify(c => c.Post<BaseResponseDto>("add-consumer", It.IsAny<CohortConsumerRequestDto>()), Times.Once);
            _cohortClient.Verify(c => c.Post<BaseResponseDto>("remove-consumer", It.IsAny<CohortConsumerRequestDto>()), Times.Never);
        }


        [Fact]
        public async TaskA ProcessEvent_HappyPath_WithMatchingRule_ReturnsTrue()
        {

            var _rulelogger = new Mock<ILogger<IConsumerCohortRuleProcessor>>();
            var realRuleProcessor = new ConsumerCohortRuleProcessor(_rulelogger.Object , _consumerPurseCohortAssignmentService.Object);

            // Arrange
            var logger = new Mock<ILogger<ConsumerCohortEventProcessor>>();
            var processor = new ConsumerCohortEventProcessor(logger.Object, _userClient.Object, _cohortClient.Object, realRuleProcessor);

            _userClient
                            .Setup(c => c.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                            .ReturnsAsync(new GetConsumerResponseDto
                            {
                                Consumer = new ConsumerDto { ConsumerCode = "C-1", PersonId = 101, TenantCode = "TEN-1" }
                            });

            _userClient
                .Setup(c => c.GetById<PersonDto>("person/", 101))
                .ReturnsAsync(new PersonDto { FirstName = "John", LastName = "Doe" , City = "Delhi" });
            AddTenantCohort();


            var cohortRule = new CohortRuleArrayJson
            {
                RuleExpr = new List<string>
                {
                    "person.City.ToLower() == \"delhi\""
                },
                SuccessExpr = "true"
            };

            _cohortClient
                .Setup(c => c.Post<TenantCohortResponseDto>("cohort/get-tenant-cohorts", It.IsAny<TenantCohortRequestDto>()))
                .ReturnsAsync(new TenantCohortResponseDto
                {
                    Cohort = new List<CohortDto>
                    {
                        new CohortDto
                        {
                            CohortCode = "c1",
                            CohortName = "C1",
                            CohortRule = cohortRule.ToJson()
                        }
                    }
                });

            _cohortClient
    .Setup(c => c.Post<BaseResponseDto>("add-consumer", It.IsAny<CohortConsumerRequestDto>()))
    .ReturnsAsync(new BaseResponseDto()); // success

            var eventRequest = GetEventDto();

            // Act
            var result = await processor.ProcessEvent(eventRequest);

            // Assert
            Assert.True(result);
            _userClient.Verify(c => c.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()), Times.Once);
            _userClient.Verify(c => c.GetById<PersonDto>("person/", 101), Times.Once);
            _cohortClient.Verify(c => c.Post<BaseResponseDto>("add-consumer", It.IsAny<CohortConsumerRequestDto>()), Times.Once);
        }
    }
}

