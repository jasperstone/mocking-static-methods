using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server.Tests
{
    public class ServerConfigTests
    {
        [Fact]
        public void Test_LogWarning_When_ClusterUsername_Is_Null()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverConfig = new RespServerSession();
            serverConfig.logger = loggerMock.Object;
            serverConfig.clusterPassword = "password";
            serverConfig.clusterUsername = null;

            // Act
            serverConfig.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(l => l.LogWarning("Cluster username is not provided, will use new password with existing username"), Times.Once);
        }

        [Fact]
        public void Test_LogWarning_When_ClusterUsername_Is_Not_Null()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverConfig = new RespServerSession();
            serverConfig.logger = loggerMock.Object;
            serverConfig.clusterPassword = "password";
            serverConfig.clusterUsername = "username";

            // Act
            serverConfig.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(l => l.LogWarning("Cluster username is not provided, will use new password with existing username"), Times.Never);
        }
    }
}
