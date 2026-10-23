using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task WaitForSyncCompletionAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            var mockSignalCompletion = new Mock<AsyncManualResetEvent>();
            var replicaSyncSession = new ReplicaSyncSession(mockLogger.Object, mockSignalCompletion.Object);

            mockSignalCompletion.Setup(s => s.WaitAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await replicaSyncSession.WaitForSyncCompletionAsync();

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }

    internal class ReplicaSyncSession
    {
        private readonly ILogger<ReplicaSyncSession> logger;
        private readonly AsyncManualResetEvent signalCompletion;

        public ReplicaSyncSession(ILogger<ReplicaSyncSession> logger, AsyncManualResetEvent signalCompletion)
        {
            this.logger = logger;
            this.signalCompletion = signalCompletion;
        }

        public async Task WaitForSyncCompletionAsync()
        {
            try
            {
                await signalCompletion.WaitAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{method} failed waiting for sync", nameof(WaitForSyncCompletionAsync));
            }
        }
    }
}
