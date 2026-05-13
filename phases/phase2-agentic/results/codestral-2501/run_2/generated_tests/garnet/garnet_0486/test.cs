using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
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
            var clusterProviderMock = new Mock<ClusterProvider>();
            storeWrapperMock.Setup(sw => sw.clusterProvider).Returns(clusterProviderMock.Object);

            var session = new RespServerSession(loggerMock.Object, storeWrapperMock.Object);

            // Act
            session.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning("Cluster username is not provided, will use new password with existing username"),
                Times.Once);
        }
    }
}
