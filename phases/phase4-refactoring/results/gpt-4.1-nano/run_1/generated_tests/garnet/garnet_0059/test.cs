using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private readonly Mock<ILogger<FailoverSession>> _loggerMock;
        private readonly Mock<cluster.ClusterProvider> _clusterProviderMock;
        private readonly FailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<FailoverSession>>();
            _clusterProviderMock = new Mock<cluster.ClusterProvider>();
            _session = new FailoverSession(_clusterProviderMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task LogCritical_IsCalledOnExceptionInBroadcastConfigAndRequestAttachAsync()
        {
            // Arrange
            var replicaId = "replica1";
            var configData = new byte[] { 1, 2, 3 };
            var mockClient = new Mock<GarnetClient>();
            var tcs = new TaskCompletionSource<bool>();
            var cts = new CancellationTokenSource();

            // Setup GossipAsync to throw
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new Mock<IAsyncEnumerable<byte[]>>().Object);

            // Setup WaitAsync to throw
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new Mock<IAsyncEnumerable<byte[]>>().Object);

            // Replace GetConnectionAsync to return our mock client
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new cluster.ClusterConfig());
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new cluster.ClusterConfig());
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new cluster.ClusterConfig());
            _clusterProviderMock.Setup(cp => cp.clusterManager.CurrentConfig).Returns(new cluster.ClusterConfig());

            // To simulate the exception, we need to make the call to GossipAsync throw
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(() => throw new Exception("Test exception"));

            // Act
            await _session.BroadcastConfigAndRequestAttachAsync(replicaId, configData);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas faulted")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
