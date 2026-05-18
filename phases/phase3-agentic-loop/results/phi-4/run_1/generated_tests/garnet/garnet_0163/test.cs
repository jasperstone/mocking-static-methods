using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task WaitForSyncCompletionAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var tokenSource = new CancellationTokenSource();
            var replicaSyncSession = new ReplicaSyncSession
            {
                logger = loggerMock.Object,
                token = tokenSource.Token
            };

            // Simulate an exception
            replicaSyncSession.signalCompletion = new SemaphoreSlim(0, 1);

            // Act
            await replicaSyncSession.WaitForSyncCompletionAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s.Contains("{method} failed waiting for sync")),
                    It.Is<object[]>(o => o[0].ToString() == nameof(ReplicaSyncSession.WaitForSyncCompletionAsync))
                ),
                Times.Once
            );
        }
    }
}
