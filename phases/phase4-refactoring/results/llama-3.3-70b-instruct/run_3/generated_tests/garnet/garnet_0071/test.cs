using Xunit;
using Moq;
using System.Net;
using System.Net.Security;
using Garnet.common;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class GarnetServerNodeTests
    {
        [Fact]
        public void LogWarning_IsCalled_WhenTaskFaults()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var endpoint = new IPEndPoint(IPAddress.Loopback, 1234);
            var tlsOptions = new SslClientAuthenticationOptions();
            var epoch = new LightEpoch();
            var garnetServerNode = new GarnetServerNode(clusterProviderMock.Object, endpoint, tlsOptions, epoch, loggerMock.Object);
            var task = Task.FromException(new Exception("Test exception"));

            // Act
            garnetServerNode.gossipTask = task;

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}
