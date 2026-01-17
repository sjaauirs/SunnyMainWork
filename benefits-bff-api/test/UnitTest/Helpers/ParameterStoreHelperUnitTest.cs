using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Infrastructure.Helpers;
using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Helpers
{
    public class ParameterStoreHelperUnitTest
    {
        private readonly Mock<IAmazonSimpleSystemsManagement> _ssmClientMock;
        private readonly Mock<ILogger<ParameterStoreHelper>> _loggerMock;

        public ParameterStoreHelperUnitTest()
        {
            _ssmClientMock = new Mock<IAmazonSimpleSystemsManagement>();
            _loggerMock = new Mock<ILogger<ParameterStoreHelper>>();
        }

        #region GetRawValueAsync Tests

        [Fact]
        public async Task GetRawValueAsync_Success_ReturnsParameterValue()
        {
            // Arrange
            var parameterName = "/test/parameter";
            var expectedValue = "test-value";
            
            var response = new GetParameterResponse
            {
                Parameter = new Parameter
                {
                    Name = parameterName,
                    Value = expectedValue
                },
                HttpStatusCode = HttpStatusCode.OK
            };

            _ssmClientMock
                .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            var result = await helper.GetRawValueAsync(parameterName);

            // Assert
            Assert.Equal(expectedValue, result);
            _ssmClientMock.Verify(x => x.GetParameterAsync(
                It.Is<GetParameterRequest>(r => r.Name == parameterName && r.WithDecryption == true),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRawValueAsync_WithDecryptionFalse_PassesCorrectFlag()
        {
            // Arrange
            var parameterName = "/test/parameter";
            var expectedValue = "test-value";
            
            var response = new GetParameterResponse
            {
                Parameter = new Parameter
                {
                    Name = parameterName,
                    Value = expectedValue
                },
                HttpStatusCode = HttpStatusCode.OK
            };

            _ssmClientMock
                .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            var result = await helper.GetRawValueAsync(parameterName, withDecryption: false);

            // Assert
            Assert.Equal(expectedValue, result);
            _ssmClientMock.Verify(x => x.GetParameterAsync(
                It.Is<GetParameterRequest>(r => r.Name == parameterName && r.WithDecryption == false),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRawValueAsync_ParameterNotFound_ReturnsNullAndLogsWarning()
        {
            // Arrange
            var parameterName = "/test/nonexistent";
            
            _ssmClientMock
                .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ParameterNotFoundException("Parameter not found"));

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            var result = await helper.GetRawValueAsync(parameterName);

            // Assert
            Assert.Null(result);
            VerifyLogWarning(_loggerMock, "Parameter not found in SSM");
        }

        [Fact]
        public async Task GetRawValueAsync_OtherException_ThrowsAndLogsError()
        {
            // Arrange
            var parameterName = "/test/parameter";
            var exception = new Exception("SSM service error");
            
            _ssmClientMock
                .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => helper.GetRawValueAsync(parameterName));
            Assert.Equal(exception, thrownException);
            VerifyLogError(_loggerMock, "Error retrieving parameter from SSM");
        }

        [Fact]
        public async Task GetRawValueAsync_NullParameter_ReturnsNull()
        {
            // Arrange
            var parameterName = "/test/parameter";
            
            var response = new GetParameterResponse
            {
                Parameter = null,
                HttpStatusCode = HttpStatusCode.OK
            };

            _ssmClientMock
                .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            var result = await helper.GetRawValueAsync(parameterName);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetDeserializedValueAsync Tests

        [Fact]
        public async Task GetDeserializedValueAsync_Success_ReturnsDeserializedObject()
        {
            // Arrange
            var parameterName = "/test/parameter";
            var testObject = new { Name = "Test", Value = 123 };
            var jsonValue = JsonSerializer.Serialize(testObject);
            
            var response = new GetParameterResponse
            {
                Parameter = new Parameter
                {
                    Name = parameterName,
                    Value = jsonValue
                },
                HttpStatusCode = HttpStatusCode.OK
            };

            _ssmClientMock
                .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            var result = await helper.GetDeserializedValueAsync<TestDto>(parameterName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
            Assert.Equal(123, result.Value);
        }

        [Fact]
        public async Task GetDeserializedValueAsync_NullRawValue_ReturnsDefaultAndLogsWarning()
        {
            // Arrange
            var parameterName = "/test/parameter";
            
            _ssmClientMock
                .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ParameterNotFoundException("Parameter not found"));

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            var result = await helper.GetDeserializedValueAsync<TestDto>(parameterName);

            // Assert
            Assert.Null(result);
            VerifyLogWarning(_loggerMock, "Empty or null parameter value for");
        }

        [Fact]
        public async Task GetDeserializedValueAsync_EmptyRawValue_ReturnsDefaultAndLogsWarning()
        {
            // Arrange
            var parameterName = "/test/parameter";
            
            var response = new GetParameterResponse
            {
                Parameter = new Parameter
                {
                    Name = parameterName,
                    Value = ""
                },
                HttpStatusCode = HttpStatusCode.OK
            };

            _ssmClientMock
                .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            var result = await helper.GetDeserializedValueAsync<TestDto>(parameterName);

            // Assert
            Assert.Null(result);
            VerifyLogWarning(_loggerMock, "Empty or null parameter value for");
        }

        [Fact]
        public async Task GetDeserializedValueAsync_InvalidJson_ThrowsAndLogsError()
        {
            // Arrange
            var parameterName = "/test/parameter";
            var invalidJson = "{ invalid json }";
            
            var response = new GetParameterResponse
            {
                Parameter = new Parameter
                {
                    Name = parameterName,
                    Value = invalidJson
                },
                HttpStatusCode = HttpStatusCode.OK
            };

            _ssmClientMock
                .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => helper.GetDeserializedValueAsync<TestDto>(parameterName));
            VerifyLogError(_loggerMock, "Failed to deserialize parameter");
        }

        [Fact]
        public async Task GetDeserializedValueAsync_CaseInsensitive_DeserializesCorrectly()
        {
            // Arrange
            var parameterName = "/test/parameter";
            var jsonValue = "{\"name\":\"Test\",\"value\":123}"; // lowercase property names
            
            var response = new GetParameterResponse
            {
                Parameter = new Parameter
                {
                    Name = parameterName,
                    Value = jsonValue
                },
                HttpStatusCode = HttpStatusCode.OK
            };

            _ssmClientMock
                .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            var result = await helper.GetDeserializedValueAsync<TestDto>(parameterName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
            Assert.Equal(123, result.Value);
        }

        #endregion

        #region SaveValueAsync Tests

        [Fact]
        public async Task SaveValueAsync_Success_ReturnsTrue()
        {
            // Arrange
            var parameterName = "/test/parameter";
            var testObject = new TestDto { Name = "Test", Value = 123 };
            
            var response = new PutParameterResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            _ssmClientMock
                .Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            var result = await helper.SaveValueAsync(parameterName, testObject);

            // Assert
            Assert.True(result);
            _ssmClientMock.Verify(x => x.PutParameterAsync(
                It.Is<PutParameterRequest>(r => 
                    r.Name == parameterName && 
                    r.Type == ParameterType.String &&
                    r.Overwrite == true &&
                    !string.IsNullOrEmpty(r.Value)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SaveValueAsync_NonOkStatusCode_ReturnsFalse()
        {
            // Arrange
            var parameterName = "/test/parameter";
            var testObject = new TestDto { Name = "Test", Value = 123 };
            
            var response = new PutParameterResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest
            };

            _ssmClientMock
                .Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            var result = await helper.SaveValueAsync(parameterName, testObject);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SaveValueAsync_SerializesCorrectly()
        {
            // Arrange
            var parameterName = "/test/parameter";
            var testObject = new TestDto { Name = "Test", Value = 123 };
            
            PutParameterRequest capturedRequest = null;
            var response = new PutParameterResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            };

            _ssmClientMock
                .Setup(x => x.PutParameterAsync(It.IsAny<PutParameterRequest>(), It.IsAny<CancellationToken>()))
                .Callback<PutParameterRequest, CancellationToken>((req, ct) => capturedRequest = req)
                .ReturnsAsync(response);

            var helper = new ParameterStoreHelper(_ssmClientMock.Object, _loggerMock.Object);

            // Act
            await helper.SaveValueAsync(parameterName, testObject);

            // Assert
            Assert.NotNull(capturedRequest);
            var deserialized = JsonSerializer.Deserialize<TestDto>(capturedRequest.Value);
            Assert.NotNull(deserialized);
            Assert.Equal("Test", deserialized.Name);
            Assert.Equal(123, deserialized.Value);
        }

        #endregion

        #region Helper Methods

        private void VerifyLogWarning(Mock<ILogger<ParameterStoreHelper>> loggerMock, string messageContains)
        {
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(messageContains)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private void VerifyLogError(Mock<ILogger<ParameterStoreHelper>> loggerMock, string messageContains)
        {
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(messageContains)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region Test DTOs

        private class TestDto
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
        }

        #endregion
    }
}

