using Moq;
using NHibernate;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.ReadReplica
{
    public class IReadOnlySessionTests
    {
        [Fact]
        public void IReadOnlySession_CanBeMocked()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var mockReadOnlySession = new Mock<IReadOnlySession>();
            mockReadOnlySession.Setup(x => x.Session).Returns(mockSession.Object);

            // Act
            var session = mockReadOnlySession.Object.Session;

            // Assert
            Assert.NotNull(session);
            Assert.Same(mockSession.Object, session);
        }

        [Fact]
        public void IReadOnlySession_SessionProperty_CanBeAccessed()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var wrapper = new ReadOnlySessionWrapper(mockSession.Object);
            IReadOnlySession readOnlySession = wrapper;

            // Act
            var session = readOnlySession.Session;

            // Assert
            Assert.NotNull(session);
            Assert.IsAssignableFrom<ISession>(session);
        }

        [Fact]
        public void ReadOnlySessionWrapper_AsIReadOnlySession_ReturnsCorrectSession()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.IsOpen).Returns(true);

            // Act
            IReadOnlySession wrapper = new ReadOnlySessionWrapper(mockSession.Object);

            // Assert
            Assert.True(wrapper.Session.IsOpen);
        }
    }
}

