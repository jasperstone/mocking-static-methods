using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_Exception_LogsCritical()
        {
            // Arrange
            var replicaId = "replica1";
            var configByteArray = new byte[] { 1, 2, 3 };
            var failoverTimeout = TimeSpan.FromSeconds(10);
            var cts = new CancellationTokenSource();

            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<GarnetClient>();
            var clusterProviderMock = new Mock<ClusterProvider>();

            clientMock.Setup(client => client.GossipAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Test exception"));

            var session = (FailoverSession)Activator.CreateInstance(typeof(FailoverSession), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { clusterProviderMock.Object, loggerMock.Object, failoverTimeout, cts.Token }, null);

            // Act
            var method = typeof(FailoverSession).GetMethod("BroadcastConfigAndRequestAttachAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method.Invoke(session, new object[] { replicaId, configByteArray });

            // Assert
            loggerMock.Verify(
                logger => logger.LogCritical(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
