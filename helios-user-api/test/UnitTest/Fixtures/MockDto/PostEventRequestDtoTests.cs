using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public class PostEventRequestDtoTests
    {
        [Fact]
        public void Should_Serialize_With_Dynamic_EventData()
        {
            // Arrange
            var dto = new PostEventRequestDto
            {
                EventType = "Login",
                EventSubtype = "Success",
                EventSource = "MobileApp",
                TenantCode = "T001",
                ConsumerCode = "C001",
                EventData = "{\"ip\":\"127.0.0.1\",\"device\":\"iPhone\"}"
            };

            // Act
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            // Assert
            Assert.Contains($"\"ip\":\"127.0.0.1\"",json);
        }

        [Fact]
        public void Should_Deserialize_Dynamic_EventData()
        {
            // Arrange
            var json = @"
            {
                ""EventType"": ""Purchase"",
                ""EventSubtype"": ""Confirmed"",
                ""EventSource"": ""Web"",
                ""TenantCode"": ""T002"",
                ""ConsumerCode"": ""C002"",
                ""EventData"": { ""orderId"": 1234, ""amount"": 25.5 }
            }";

            // Act
            var dto = JsonSerializer.Deserialize<PostEventRequestDto>(json);

            // Assert
            Assert.Equal("Purchase", dto.EventType);

           
        }

        [Fact]
        public void Should_Fail_Validation_When_Required_Fields_Missing()
        {
            // Arrange
            var dto = new PostEventRequestDto(); // All required fields are null

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

            // Assert
            Assert.False(isValid);
        }
    }
}
