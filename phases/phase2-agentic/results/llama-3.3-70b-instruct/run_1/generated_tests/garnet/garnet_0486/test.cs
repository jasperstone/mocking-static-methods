using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class ServerConfigTests
    {
        [Fact]
        public void TestClusterUsernameWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.SetupGet(sw => sw.clusterProvider).Returns((ClusterProvider?)null);
            var serverConfig = new RespServerSession(loggerMock.Object, storeWrapperMock.Object);

            // Act
            serverConfig.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(l => l.LogWarning("Cluster username is not provided, will use new password with existing username"), Times.Once);
        }
    }
}
