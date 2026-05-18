using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using System.Threading;
using System.Net;
using System;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsAcquireCheckpointEntryIteration()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            // Minimal mocks for dependencies
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockReplicationManager = new Mock<object>(); // We don't know the type, so object
            var mockStoreWrapper = new Mock<object>();

            // Create ReplicaSyncSession with minimal dependencies and logger
            var session = new ReplicaSyncSession(
                storeWrapper: null,
                clusterProvider: null,
                logger: mockLogger.Object);

            // Act
            try
            {
                await session.SendCheckpointAsync();
            }
            catch
            {
                // Ignore exceptions, we only want to verify logging
            }

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AcquireCheckpointEntry iteration")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
