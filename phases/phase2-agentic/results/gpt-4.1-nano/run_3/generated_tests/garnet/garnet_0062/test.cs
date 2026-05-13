using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private readonly Mock<ILogger<ReplicaFailoverSession>> _loggerMock;
        private readonly ReplicaFailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<ReplicaFailoverSession>>();
            _session = new ReplicaFailoverSession(/* dependencies as needed, possibly mocked or default */);
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarningOnException()
        {
            // Arrange
            var replicaId = "replica1";
            var configByteArray = new byte[] { 1, 2, 3 };
            var mockClient = new Mock<GarnetClient>();
            mockClient.Setup(c => c.GossipAsync(It.IsAny<byte[]>()).WaitAsync(It.IsAny<TimeSpan>(), It.IsAny<System.Threading.CancellationToken>()))
                      .ThrowsAsync(new Exception("Test exception"));

            // Inject a way to replace the client creation or set the primaryClient directly
            // For this example, assume we can set primaryClient directly
            _session.primaryClient = mockClient.Object;

            // Act
            await _session.BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "WaitingForAttachToComplete Error"),
                Times.Once);
        }
    }
}
