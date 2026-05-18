using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.client;
using Garnet.common;

namespace Garnet.cluster.Server.Failover.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCritical_WhenGossipResponseProcessingThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            // Mock the response object that will cause exception in processing
            var mockResp = new Mock<IDisposable>();
            mockResp.Setup(r => r.Length).Returns(10);
            mockResp.Setup(r => r.Span).Throws(new InvalidOperationException("Test exception"));
            mockResp.Setup(r => r.Dispose()).Verifiable();

            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(mockResp.Object);

            // Create test session with minimal dependencies
            var session = new TestableFailoverSession(loggerMock.Object)
            {
                primaryClient = mockClient.Object,
                failoverTimeout = TimeSpan.FromSeconds(1),
                cts = new CancellationTokenSource()
            };

            // Set up required fields to avoid null refs before the target exception
            session.oldConfig = new Mock<ClusterConfig>().Object;
            var mockClusterProvider = new Mock<ClusterProvider>(); // Concrete class from namespace
            mockClusterProvider.Setup(cp => cp.clusterManager).Returns(new Mock<ClusterManager>().Object);
            session.clusterProvider = mockClusterProvider.Object;

            var configByteArray = new byte[] { 1, 2, 3 };
            var replicaId = "replica1";

            // Act
            await session.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert - verify LogCritical was called
            loggerMock.Verify(
                l => l.LogCritical(
                    It.IsAny<Exception>(),
                    "IssueAttachReplicas faulted"),
                Times.Once);

            // Verify dispose was called in finally block
            mockResp.Verify(r => r.Dispose(), Times.AtLeastOnce);
        }
    }

    // Testable version that exposes constructor and makes private method public for testing
    internal class TestableFailoverSession : FailoverSession
    {
        public TestableFailoverSession(ILogger<FailoverSession> logger)
        {
            this.logger = logger;
        }

        public GarnetClient primaryClient;
        public TimeSpan failoverTimeout = TimeSpan.FromSeconds(1);
        public CancellationTokenSource cts = new CancellationTokenSource();
        public ClusterConfig oldConfig;
        public ClusterProvider clusterProvider;

        public Task BroadcastConfigAndRequestAttachAsyncPublic(string replicaId, byte[] configByteArray) =>
            BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);
    }
}
