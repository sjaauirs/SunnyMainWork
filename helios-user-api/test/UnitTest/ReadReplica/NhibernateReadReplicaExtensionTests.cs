using Microsoft.Extensions.DependencyInjection;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Mappings;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.ReadReplica
{
    public class NhibernateReadReplicaExtensionTests
    {
        [Fact]
        public void AddNhibernateReadReplica_WithNullConnectionString_DoesNotRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddNhibernateReadReplica<ConsumerMap>(null);

            // Assert
            var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IReadOnlySession));
            Assert.Null(serviceDescriptor);
        }

        [Fact]
        public void AddNhibernateReadReplica_WithEmptyConnectionString_DoesNotRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddNhibernateReadReplica<ConsumerMap>(string.Empty);

            // Assert
            var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IReadOnlySession));
            Assert.Null(serviceDescriptor);
        }

        [Fact]
        public void AddNhibernateReadReplica_WithWhitespaceConnectionString_DoesNotRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddNhibernateReadReplica<ConsumerMap>("   ");

            // Assert
            var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IReadOnlySession));
            Assert.Null(serviceDescriptor);
        }

        [Fact]
        public void AddNhibernateReadReplica_ReturnsServiceCollectionForChaining()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddNhibernateReadReplica<ConsumerMap>(null);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddNhibernateReadReplica_WithEmptyString_ReturnsServiceCollectionForChaining()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddNhibernateReadReplica<ConsumerMap>(string.Empty);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddNhibernateReadReplica_WithTabsAndNewlines_DoesNotRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddNhibernateReadReplica<ConsumerMap>("   \t\n  ");

            // Assert
            var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IReadOnlySession));
            Assert.Null(serviceDescriptor);
        }

        [Fact]
        public void AddNhibernateReadReplica_CanBeCalledMultipleTimes_WithNull()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddNhibernateReadReplica<ConsumerMap>(null);
            services.AddNhibernateReadReplica<ConsumerMap>(null);
            services.AddNhibernateReadReplica<ConsumerMap>(null);

            // Assert
            Assert.Empty(services);
        }

        [Fact]
        public void AddNhibernateReadReplica_WhenSkipped_DoesNotModifyExistingServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<object>(new object());
            var originalCount = services.Count;

            // Act
            services.AddNhibernateReadReplica<ConsumerMap>(null);

            // Assert
            Assert.Equal(originalCount, services.Count);
        }
    }
}

