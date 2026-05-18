using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;
using System.Text;

namespace Garnet.Tests
{
    public class ServerConfigTests
    {
        [Fact]
        public void LogWarning_WhenClusterUsernameIsNotProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.Setup(sw => sw.clusterProvider).Returns((ClusterProvider)null);

            var serverConfig = new ServerConfig();
            var sbErrorMsg = new StringBuilder();

            // Act
            serverConfig.NetworkCONFIG_SET(loggerMock.Object, storeWrapperMock.Object, sbErrorMsg);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning("Cluster username is not provided, will use new password with existing username"),
                Times.Once);
        }
    }
}
