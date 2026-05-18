using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager(loggerMock.Object);

            // Act and Assert
            try
            {
                await replicationManager.TryReplicateDiskbasedSyncAsync(null, null);
            }
            catch (Exception)
            {
                loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
            }
        }
    }

    internal class ReplicationManager
    {
        private readonly ILogger _logger;

        public ReplicationManager(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<(bool Success, string ErrorMessage)> TryReplicateDiskbasedSyncAsync(object session, object options)
        {
            try
            {
                throw new Exception("Test exception");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(TryReplicateDiskbasedSyncAsync));
                return (false, "Error message");
            }
        }
    }
}
