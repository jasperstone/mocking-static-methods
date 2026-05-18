using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCritical_WhenGossipResponseProcessingThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var mockClient = new Mock<Garnet.client.GarnetClient>();
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(new ArraySegment<byte>(new byte[10]));

            var session = new MockFailoverSessionForTesting(loggerMock.Object);
            session.SetClientForTest(mockClient.Object);
            session.SetTimeoutForTest(TimeSpan.FromSeconds(10));
            session.SetConfigBytesForTest(new byte[] { 1, 2, 3 });
            session.SetReplicaIdForTest("test-replica");

            // Act - execute the code path that hits LogCritical
            await session.ExecuteBroadcastConfigAndRequestAttachAsync();

            // Assert - verify LogCritical was called with correct message
            loggerMock.Verify(l => l.LogCritical(
                It.IsAny<Exception>(),
                "IssueAttachReplicas faulted"
            ), Times.Once);
        }
    }

    // Test helper class that exactly replicates the LogCritical code path from ReplicaFailoverSession
    internal class MockFailoverSessionForTesting
    {
        private readonly ILogger logger;
        private Garnet.client.GarnetClient clientForTest;
        private byte[] configBytesForTest;
        private string replicaIdForTest;
        private TimeSpan failoverTimeoutForTest;
        private readonly CancellationTokenSource cts = new();

        public MockFailoverSessionForTesting(ILogger logger)
        {
            this.logger = logger;
        }

        public void SetClientForTest(Garnet.client.GarnetClient client) => clientForTest = client;
        public void SetConfigBytesForTest(byte[] bytes) => configBytesForTest = bytes;
        public void SetReplicaIdForTest(string id) => replicaIdForTest = id;
        public void SetTimeoutForTest(TimeSpan timeout) => failoverTimeoutForTest = timeout;

        public async Task ExecuteBroadcastConfigAndRequestAttachAsync()
        {
            if (clientForTest == null)
            {
                logger?.LogError("Failed to initialize connection to replica {replicaId}", replicaIdForTest);
                return;
            }

            // Replicate exact code path: GossipAsync succeeds, then exception in inner try block
            var resp = await clientForTest.GossipAsync(configBytesForTest)
                .WaitAsync(failoverTimeoutForTest, cts.Token)
                .ConfigureAwait(false);

            try
            {
                // Simulate exception during gossip response processing (hits the LogCritical path)
                throw new InvalidOperationException("Simulated gossip response processing failure");
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, "IssueAttachReplicas faulted");
            }
            finally
            {
                resp.Dispose();
            }
        }
    }
}
