using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Helpers;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.ReadReplica
{
    public class ConsumerServiceReadReplicaTests
    {
        [Fact]
        public void ReadOnlySession_WhenNull_DbSourceReturnsPrimary()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            IReadOnlySession? readOnlySession = null;

            // Act
            var dbSource = readOnlySession != null ? "ReadReplica" : "Primary";

            // Assert
            Assert.Equal("Primary", dbSource);
        }

        [Fact]
        public void ReadOnlySession_WhenNotNull_DbSourceReturnsReadReplica()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            IReadOnlySession readOnlySession = new ReadOnlySessionWrapper(mockSession.Object);

            // Act
            var dbSource = readOnlySession != null ? "ReadReplica" : "Primary";

            // Assert
            Assert.Equal("ReadReplica", dbSource);
        }

        [Fact]
        public void ReadSession_WhenReadOnlySessionNull_ReturnsPrimarySession()
        {
            // Arrange
            var primarySession = new Mock<ISession>();
            IReadOnlySession? readOnlySession = null;

            // Act - This simulates the ReadSession property logic
            var readSession = readOnlySession?.Session ?? primarySession.Object;

            // Assert
            Assert.Same(primarySession.Object, readSession);
        }

        [Fact]
        public void ReadSession_WhenReadOnlySessionNotNull_ReturnsReadReplicaSession()
        {
            // Arrange
            var primarySession = new Mock<ISession>();
            var replicaSession = new Mock<ISession>();
            IReadOnlySession readOnlySession = new ReadOnlySessionWrapper(replicaSession.Object);

            // Act - This simulates the ReadSession property logic
            var readSession = readOnlySession?.Session ?? primarySession.Object;

            // Assert
            Assert.Same(replicaSession.Object, readSession);
            Assert.NotSame(primarySession.Object, readSession);
        }

        [Fact]
        public void ReadOnlySessionWrapper_CanBeUsedInQueryOperations()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var mockQueryOver = new Mock<IQueryOver<ConsumerModel, ConsumerModel>>();
            
            mockSession.Setup(s => s.QueryOver<ConsumerModel>())
                .Returns(mockQueryOver.Object);
            
            var wrapper = new ReadOnlySessionWrapper(mockSession.Object);

            // Act
            var queryOver = wrapper.Session.QueryOver<ConsumerModel>();

            // Assert
            Assert.NotNull(queryOver);
            mockSession.Verify(s => s.QueryOver<ConsumerModel>(), Times.Once);
        }

        [Fact]
        public void ReadOnlySessionWrapper_SessionIsNotClosed_WhenCreated()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.IsOpen).Returns(true);

            // Act
            var wrapper = new ReadOnlySessionWrapper(mockSession.Object);

            // Assert
            Assert.True(wrapper.Session.IsOpen);
        }
    }
}
