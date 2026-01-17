using Moq;
using NHibernate;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.ReadReplica
{
    public class ReadOnlySessionWrapperTests
    {
        [Fact]
        public void Constructor_WithValidSession_SetsSessionProperty()
        {
            // Arrange
            var mockSession = new Mock<ISession>();

            // Act
            var wrapper = new ReadOnlySessionWrapper(mockSession.Object);

            // Assert
            Assert.NotNull(wrapper.Session);
            Assert.Same(mockSession.Object, wrapper.Session);
        }

        [Fact]
        public void Session_ReturnsInjectedSession()
        {
            // Arrange
            var mockSession = new Mock<ISession>();
            var wrapper = new ReadOnlySessionWrapper(mockSession.Object);

            // Act
            var result = wrapper.Session;

            // Assert
            Assert.Equal(mockSession.Object, result);
        }

        [Fact]
        public void Wrapper_ImplementsIReadOnlySession()
        {
            // Arrange
            var mockSession = new Mock<ISession>();

            // Act
            var wrapper = new ReadOnlySessionWrapper(mockSession.Object);

            // Assert
            Assert.IsAssignableFrom<IReadOnlySession>(wrapper);
        }
    }
}

