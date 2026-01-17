using AutoMapper;
using FluentNHibernate.Testing.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class AdventureControllerTest
    {
        private readonly AdventureService _service;
        private readonly Mock<ILogger<AdventureService>> _serviceLogger;
        private readonly Mock<ILogger<AdventureController>> _controllerLogger;
        private readonly AdventureController _controller;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IAdventureRepo> _adventureRepo;
        private readonly Mock<ITenantAdventureRepo> _tenantAdventureRepo;
        public AdventureControllerTest()
        {
            _mapper = new Mock<IMapper>();
            _adventureRepo = new Mock<IAdventureRepo>();
            _tenantAdventureRepo = new Mock<ITenantAdventureRepo>();
            _serviceLogger = new Mock<ILogger<AdventureService>>();
            _controllerLogger = new Mock<ILogger<AdventureController>>();
            _service = new AdventureService(_serviceLogger.Object,_adventureRepo.Object,_mapper.Object, _tenantAdventureRepo.Object);
            _controller = new AdventureController(_controllerLogger.Object, _service);
        }
        [Fact]
        public async TaskAlias GetAll_Adventure_Should_Return_Ok_Response()
        {
            // Arrange 
            var requestDto = new GetAdventureRequestDto();
            var adventureModels = new List<AdventureModel>() { new AdventureMockModel() };
            _adventureRepo.Setup(x => x.GetAllAdventures(It.IsAny<string>())).ReturnsAsync(adventureModels);

            // Act 
            var response = await _controller.GetAllAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }
        [Fact]
        public async TaskAlias GetAll_Adventure_Should_Return_NotFound_Response()
        {
            // Arrange 
            var requestDto = new GetAdventureRequestDto();
            var adventureModels = new List<AdventureModel>() { new AdventureMockModel() };
            _adventureRepo.Setup(x => x.GetAllAdventures(It.IsAny<string>()));

            // Act 
            var response = await _controller.GetAllAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetAll_Adventure_Should_Return_InternalServer_Response()
        {
            // Arrange 
            var requestDto = new GetAdventureRequestDto();
            var adventureModels = new List<AdventureModel>() { new AdventureMockModel() };
            _adventureRepo.Setup(x => x.GetAllAdventures(It.IsAny<string>())).Throws(new Exception("testing"));

            // Act 
            var response = await _controller.GetAllAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }


        [Fact]
        public async TaskAlias GetTenant_Adventure_Should_Return_Ok_Response()
        {
            // Arrange 
            var requestDto = new ExportAdventureRequestDto();
            var adventureModels = new ExportAdventureResponseDto()
            {
                Adventures = new List<AdventureDto>() { new AdventureMockDto() },
                TenantAdventures = new List<TenantAdventureDto>() { new TenantAdventureMockDto() }
            };
            _adventureRepo.Setup(x => x.GetTenantAdventures(It.IsAny<string>())).ReturnsAsync(adventureModels);

            // Act 
            var response = await _controller.ExportAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }
        [Fact]
        public async TaskAlias GetTenant_Adventure_Should_Return_NotFound_Response()
        {
            // Arrange 
            var requestDto = new ExportAdventureRequestDto();
            var adventureModels = new ExportAdventureResponseDto()
            {
                ErrorCode = StatusCodes.Status404NotFound
            };
            _adventureRepo.Setup(x => x.GetTenantAdventures(It.IsAny<string>())).ReturnsAsync(adventureModels);

            // Act 
            var response = await _controller.ExportAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetTenant_Adventure_Should_Return_InternalServer_Response()
        {
            // Arrange 
            var requestDto = new ExportAdventureRequestDto();
            _adventureRepo.Setup(x => x.GetTenantAdventures(It.IsAny<string>())).Throws(new Exception("testing"));

            // Act 
            var response = await _controller.ExportAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias ImportAdventuresAndTenantAdventures_Should_Return_Success_WhenAdventure_and_TenantAdventureArethere()
        {
            // Arrange 
            var requestDto = new ImportAdventureRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                Adventures = new List<AdventureDto>
                { 
                    new AdventureDto
                    {
                        AdventureId = 201,
                        AdventureCode = "ADV-12345",
                        AdventureConfigJson = "{\"key\": \"value\"}",
                        CmsComponentCode = "CMS-002",
                        LanguageCode = "en-US",
                    }
                },
                TenantAdventures = new List<TenantAdventureDto>
                {
                    new TenantAdventureDto
                    {
                        AdventureId = 201,
                        TenantAdventureCode = "tenant-sample-adventure-code1",
                        TenantAdventureId = 1,
                        TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    }
                }
            };

            var adventureModel = new AdventureModel
            {
                AdventureId = 201,
                AdventureCode = "ADV-12345",
                AdventureConfigJson = "{\"key\": \"value\"}",
                CmsComponentCode = "CMS-002"
            };

            var tenantAdventureModel = new TenantAdventureModel
            {
                AdventureId = 201,
                TenantAdventureCode = "tenant-sample-adventure-code1",
                TenantAdventureId = 1,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            _adventureRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<AdventureModel, bool>>>(), false))
                .ReturnsAsync(new List<AdventureModel> { new AdventureModel { AdventureId = 201} });

            _tenantAdventureRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TenantAdventureModel, bool>>>(), false))
               .ReturnsAsync(new List<TenantAdventureModel> { new TenantAdventureModel { AdventureId = 201 ,TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4" , TenantAdventureId = 1 } });

            _mapper
                .Setup(m => m.Map<AdventureModel>(It.IsAny<AdventureDto>()))
                .Returns(adventureModel);
            _mapper
                .Setup(m => m.Map<TenantAdventureModel>(It.IsAny<TenantAdventureDto>()))
                .Returns(tenantAdventureModel);

            _adventureRepo
                .Setup(repo => repo.UpdateAsync(It.IsAny<AdventureModel>()))
                .ReturnsAsync(new AdventureModel { AdventureId = 201 });
            _tenantAdventureRepo
              .Setup(repo => repo.UpdateAsync(It.IsAny<TenantAdventureModel>()))
              .ReturnsAsync(new TenantAdventureModel { AdventureId = 201,TenantAdventureId = 1 });

            // Act 
            var response = await _controller.ImportAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }
        [Fact]
        public async TaskAlias ImportAdventuresAndTenantAdventures_Should_Return_Success_CreatesNewRecords()
        {
            // Arrange 
            var requestDto = new ImportAdventureRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                Adventures = new List<AdventureDto>
                {
                    new AdventureDto
                    {
                        AdventureId = 201,
                        AdventureCode = "ADV-12345",
                        AdventureConfigJson = "{\"key\": \"value\"}",
                        CmsComponentCode = "CMS-002",
                        LanguageCode = "en-US",
                    }
                },
                TenantAdventures = new List<TenantAdventureDto>
                {
                    new TenantAdventureDto
                    {
                        AdventureId = 201,
                        TenantAdventureCode = "tenant-sample-adventure-code1",
                        TenantAdventureId = 1,
                        TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    }
                }
            };
            var adventureModel = new AdventureModel
            {
                AdventureId = 201,
                AdventureCode = "ADV-12345",
                AdventureConfigJson = "{\"key\": \"value\"}",
                CmsComponentCode = "CMS-002"
            };

            var tenantAdventureModel = new TenantAdventureModel
            {
                AdventureId = 201,
                TenantAdventureCode = "tenant-sample-adventure-code1",
                TenantAdventureId = 1,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            _adventureRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<AdventureModel, bool>>>(), false)).ReturnsAsync(new List<AdventureModel>
            {
                new AdventureModel{ AdventureId = 201 }
            });

            _tenantAdventureRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TenantAdventureModel, bool>>>(), false)).ReturnsAsync(new List<TenantAdventureModel>
            {
                new TenantAdventureModel{ AdventureId = 201 , TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4" }
            });

            _mapper
                .Setup(m => m.Map<AdventureModel>(It.IsAny<AdventureDto>()))
                .Returns(adventureModel);
            _mapper
                .Setup(m => m.Map<TenantAdventureModel>(It.IsAny<TenantAdventureDto>()))
                .Returns(tenantAdventureModel);

            _adventureRepo
                .Setup(repo => repo.CreateAsync(It.IsAny<AdventureModel>()))
                .ReturnsAsync(new AdventureModel { AdventureId = 201 });
            _tenantAdventureRepo
              .Setup(repo => repo.CreateAsync(It.IsAny<TenantAdventureModel>()))
              .ReturnsAsync(new TenantAdventureModel { AdventureId = 201, TenantAdventureId = 1 });

            // Act 
            var response = await _controller.ImportAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }
        [Fact]
        public async TaskAlias ImportAdventuresAndTenantAdventures_Should_ThrowsException()
        {
            // Arrange 
            var requestDto = new ImportAdventureRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                Adventures = new List<AdventureDto>
                {
                    new AdventureDto
                    {
                         AdventureId = 201,
                    }
                },
                TenantAdventures = new List<TenantAdventureDto>
                {
                    new TenantAdventureDto
                    {
                        AdventureId = 201,
                        TenantAdventureId = 1
                    }
                }
            };
            _adventureRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<AdventureModel, bool>>>(), false)).ThrowsAsync(new Exception());
            // Act 
            var response = await _controller.ImportAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status206PartialContent, objectResult.StatusCode);

        }
        [Fact]
        public async TaskAlias ImportAdventuresAndTenantAdventures_Should_ThrowsException_InTenantAdventure()
        {
            // Arrange 
            var requestDto = new ImportAdventureRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                Adventures = new List<AdventureDto>
                {
                    new AdventureDto()
                },
                TenantAdventures = new List<TenantAdventureDto>
                {
                    new TenantAdventureDto()
                }
            };

            _tenantAdventureRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantAdventureModel, bool>>>(), false)).ThrowsAsync(new Exception());
            // Act 
            var response = await _controller.ImportAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status206PartialContent, objectResult.StatusCode);

        }
        [Fact]
        public async TaskAlias ImportAdventuresAndTenantAdventures_Should_Return_Success_Response()
        {
            // Arrange 
            var requestDto = new ImportAdventureRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                Adventures = new List<AdventureDto>
                {
                    new AdventureDto
                    {
                        AdventureId = 201,
                        AdventureCode = "ADV-12345",
                        AdventureConfigJson = "{\"key\": \"value\"}",
                        CmsComponentCode = "CMS-002",
                        LanguageCode = "en-US",
                    }
                },
                TenantAdventures = new List<TenantAdventureDto>
                {
                    new TenantAdventureDto
                    {
                        AdventureId = 201,
                        TenantAdventureCode = "tenant-sample-adventure-code1",
                        TenantAdventureId = 1,
                        TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    }
                }
            };
            var adventureModel = new AdventureModel
            {
                AdventureId = 201,
                AdventureCode = "ADV-12345",
                AdventureConfigJson = "{\"key\": \"value\"}",
                CmsComponentCode = "CMS-002"
            };

            var tenantAdventureModel = new TenantAdventureModel
            {
                AdventureId = 201,
                TenantAdventureCode = "tenant-sample-adventure-code1",
                TenantAdventureId = 1,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            _adventureRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<AdventureModel, bool>>>(), false));

            _tenantAdventureRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantAdventureModel, bool>>>(), false));

            _tenantAdventureRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TenantAdventureModel, bool>>>(), false)).ReturnsAsync(new List<TenantAdventureModel>() { new TenantAdventureModel() });
            _adventureRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<AdventureModel, bool>>>(), false)).ReturnsAsync(new List<AdventureModel>() { new AdventureModel() });
            _mapper
                .Setup(m => m.Map<AdventureModel>(It.IsAny<AdventureDto>()))
                .Returns(adventureModel);
            _mapper
                .Setup(m => m.Map<TenantAdventureModel>(It.IsAny<TenantAdventureDto>()))
                .Returns(tenantAdventureModel);

            _adventureRepo
                .Setup(repo => repo.CreateAsync(It.IsAny<AdventureModel>()))
                .ReturnsAsync(new AdventureModel { AdventureId = 201 });
            _tenantAdventureRepo
              .Setup(repo => repo.UpdateAsync(It.IsAny<TenantAdventureModel>()))
              .ReturnsAsync(new TenantAdventureModel { AdventureId = 201, TenantAdventureId = 1 });

            // Act 
            var response = await _controller.ImportAdventures(requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }
    }
}
