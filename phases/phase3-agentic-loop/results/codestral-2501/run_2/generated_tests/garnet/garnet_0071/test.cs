using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Garnet.server;

public class GarnetServerNodeTests
{
    [Fact]
    public void LogWarning_WhenGossipTaskFaults()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockClusterProvider = new Mock<ClusterProvider>();
        var mockClusterManager = new Mock<ClusterManager>();
        var mockGossipStats = new Mock<GossipStats>();
        var mockGarnetClient = new Mock<GarnetClient>();
        var mockSslClientAuthenticationOptions = new Mock<SslClientAuthenticationOptions>();
        var mockLightEpoch = new Mock<LightEpoch>();

        mockClusterProvider.Setup(cp => cp.clusterManager).Returns(mockClusterManager.Object);
        mockClusterManager.Setup(cm => cm.gossipStats).Returns(mockGossipStats.Object);
        mockClusterManager.Setup(cm => cm.CurrentConfig).Returns(new ClusterConfig());

        var node = new GarnetServerNode(
            mockClusterProvider.Object,
            new IPEndPoint(IPAddress.Loopback, 12345),
            mockSslClientAuthenticationOptions.Object,
            mockLightEpoch.Object,
            mockLogger.Object
        );

        var task = Task.FromException(new Exception("Test exception"));
        node.gossipTask = task;

        // Act
        var result = node.SendGossipMessageOrProcessResponseAndSendAgain();

        // Assert
        mockLogger.Verify(
            logger => logger.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
        Assert.False(result);
    }
}
