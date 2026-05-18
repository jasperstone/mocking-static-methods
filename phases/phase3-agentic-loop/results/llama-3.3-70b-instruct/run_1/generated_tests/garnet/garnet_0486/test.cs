using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class ServerConfigTests
    {
        [Fact]
        public void TestLogWarning_WhenClusterUsernameIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var respServerSession = new RespServerSession();
            respServerSession.logger = loggerMock.Object;
            respServerSession.clusterUsername = null;
            respServerSession.clusterPassword = "password";

            // Act
            respServerSession.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(l => l.LogWarning("Cluster username is not provided, will use new password with existing username"), Times.Once);
        }

        [Fact]
        public void TestLogWarning_WhenClusterUsernameIsNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var respServerSession = new RespServerSession();
            respServerSession.logger = loggerMock.Object;
            respServerSession.clusterUsername = "username";
            respServerSession.clusterPassword = "password";

            // Act
            respServerSession.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
        }
    }
}
