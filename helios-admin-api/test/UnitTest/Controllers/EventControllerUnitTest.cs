using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using Xunit;
using MSTask = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.Api.Tests.Controllers
{
    public class EventControllerTests
    {
        private readonly Mock<ILogger<EventController>> _mockLogger;
        private readonly EventService _eventService;
        private readonly EventController _controller;
        private readonly IMapper _mapper;


        private readonly Mock<IAwsQueueService> _mockAwsQueueService;
        private readonly Mock<ILogger<AwsQueueService>> _mockAwsLogger;
        private readonly Mock<ILogger<EventService>> _mockserviceLogger;

        public EventControllerTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PostEventRequestDto, PostEventRequestModel>();
                cfg.CreateMap<PostEventRequestModel, PostEventRequestDto>();
            });

            _mapper = config.CreateMapper();

            _mockAwsQueueService = new Mock<IAwsQueueService>();
            _mockAwsLogger = new Mock<ILogger<AwsQueueService>>();
            _mockserviceLogger = new Mock<ILogger<EventService>>();
            _eventService = new EventService(_mockserviceLogger.Object, _mockAwsQueueService.Object, _mapper);
            _mockLogger = new Mock<ILogger<EventController>>();
            _controller = new EventController(_mockLogger.Object, _eventService);

        }

        [Fact]
        public async MSTask PostEvents_ReturnsOk_ForSuccessfulResponse()
        {
            // Arrange
            var request = new PostEventRequestDto
            {
                EventType = "TEST",
                EventSubtype = "SUBTYPE",
                EventSource = "SOURCE",
                TenantCode = "TENANT",
                ConsumerCode = "CONSUMER",
                EventData = null
            };


            var expectedResult = (true, "Event pushed successfully");

            var SQSresponse = new PostEventResponseDto
            {
                ErrorCode = null,
                ErrorMessage = null
            };


            _mockAwsQueueService
                .Setup(service => service.PushEventToConsumerEventQueue(It.IsAny<PostEventRequestModel>()))
                .ReturnsAsync(expectedResult);


            // Act
            var result = await _controller.PostEvents(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resultt = Assert.IsType<PostEventResponseDto>(okResult.Value);
            Assert.Null(resultt.ErrorMessage);
        }

        [Fact]
        public async MSTask PostEvents_ReturnsInternalServerError_ForErrorCode500()
        {
            // Arrange
            var request = new PostEventRequestDto
            {
                EventType = "TEST",
                EventSubtype = "SUBTYPE",
                EventSource = "SOURCE",
                TenantCode = "TENANT",
                ConsumerCode = "CONSUMER",
                EventData = null
            };

            var expectedResult = (false, "Error !- Event pushed failed");

            var SQSresponse = new PostEventResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = "Error !- Event pushed failed"
            };

            _mockAwsQueueService
                .Setup(service => service.PushEventToConsumerEventQueue(It.IsAny<PostEventRequestModel>()))
                .ReturnsAsync(expectedResult);


            // Act
            var result = await _controller.PostEvents(request);


            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async MSTask PostEvents_LogsException_WhenExceptionThrown()
        {
            var request = new PostEventRequestDto
            {
                EventType = "TEST",
                EventSubtype = "SUBTYPE",
                EventSource = "SOURCE",
                TenantCode = "TENANT",
                ConsumerCode = "CONSUMER",
                EventData = null
            };

            var SQSresponse = new PostEventResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = null
            };

            _mockAwsQueueService
                .Setup(service => service.PushEventToConsumerEventQueue(It.IsAny<PostEventRequestModel>()))
                .ThrowsAsync(new Exception("Test Exception"));



            // Act
            var result = await _controller.PostEvents(request);

            // Assert
            var responseDto = Assert.IsType<PostEventResponseDto>(result.Value);
            Assert.Equal("Test Exception", responseDto.ErrorMessage);
        }
    }
}
