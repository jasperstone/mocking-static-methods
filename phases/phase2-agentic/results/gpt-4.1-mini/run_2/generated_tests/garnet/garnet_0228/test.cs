using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManagerForTest(loggerMock.Object);

            // Act
            var result = await replicationManager.TryReplicateDiskbasedSyncAsync(null, new ReplicateSyncOptionsForTest { ThrowException = true });

            // Assert
            Assert.False(result.Success);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TryReplicateDiskbasedSyncAsync")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ReplicaSyncAttachTaskAsync_LogsErrorWhenPrimaryAddressNotAssigned()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManagerForTest(loggerMock.Object);
            replicationManager.SetPrimaryAddress(null, -1);

            // Act
            var errorMsg = await replicationManager.InvokeReplicaSyncAttachTaskAsync(false, false);

            // Assert
            Assert.NotNull(errorMsg);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("not assigned primary")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper classes to expose internal methods and simulate conditions
        private class ReplicationManagerForTest : ReplicationManager
        {
            private readonly ILogger _logger;
            private string _address;
            private int _port;

            public ReplicationManagerForTest(ILogger logger)
            {
                _logger = logger;
            }

            public new ILogger logger => _logger;

            public void SetPrimaryAddress(string address, int port)
            {
                _address = address;
                _port = port;
            }

            public async Task<string> InvokeReplicaSyncAttachTaskAsync(bool downgradeLock, bool forceAsync)
            {
                return await base.ReplicaSyncAttachTaskAsync(downgradeLock, forceAsync);
            }

            public override (string address, int port) GetLocalNodePrimaryAddress()
            {
                return (_address, _port);
            }

            public override Task<(bool Success, ReadOnlyMemory<byte> ErrorMessage)> TryReplicateDiskbasedSyncAsync(ClusterSession session, ReplicateSyncOptions options)
            {
                if (options is ReplicateSyncOptionsForTest testOptions && testOptions.ThrowException)
                {
                    throw new Exception("Test exception");
                }
                return base.TryReplicateDiskbasedSyncAsync(session, options);
            }
        }

        private class ReplicateSyncOptionsForTest : ReplicateSyncOptions
        {
            public bool ThrowException { get; set; }
        }
    }
}
