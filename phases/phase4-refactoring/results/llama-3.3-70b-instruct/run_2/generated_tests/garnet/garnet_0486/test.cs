using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server.Tests
{
    public class RespServerSessionTests
    {
        [Fact]
        public void NetworkCONFIG_SET_LogsWarning_WhenClusterUsernameIsNotProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var respServerSession = new RespServerSession(loggerMock.Object);

            // Act
            respServerSession.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(l => l.Log(LogLevel.Warning, "Cluster username is not provided, will use new password with existing username"), Times.Once);
        }
    }
}
