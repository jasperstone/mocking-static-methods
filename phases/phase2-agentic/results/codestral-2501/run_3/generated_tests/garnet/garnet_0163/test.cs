using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task WaitForFlushAsync_ShouldLogErrorAndSetStatusOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
            var replicaSyncSession = new ReplicaSyncSession
            {
                flushTask = Task.FromException(new Exception("Test exception")),
                logger = loggerMock.Object
            };

            // Act
            await replicaSyncSession.WaitForFlushAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Once);

            Assert.Equal(SyncStatus.FAILED, replicaSyncSession.GetSyncStatusInfo.syncStatus);
            Assert.Equal("Flush task faulted", replicaSyncSession.GetSyncStatusInfo.error);
        }

        [Fact]
        public async Task WaitForSyncCompletionAsync_ShouldLogErrorAndSetStatusOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
            var replicaSyncSession = new ReplicaSyncSession
            {
                logger = loggerMock.Object
            };

            // Act
            await replicaSyncSession.WaitForSyncCompletionAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Once);

            Assert.Equal(SyncStatus.FAILED, replicaSyncSession.GetSyncStatusInfo.syncStatus);
            Assert.Equal("Wait for sync task faulted", replicaSyncSession.GetSyncStatusInfo.error);
        }
    }
}
