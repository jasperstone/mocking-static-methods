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
            var token = tokenSource.Token;
            var replicaSyncSession = new ReplicaSyncSession
            {
                logger = loggerMock.Object,
                token = token
            };

            // Simulate an exception
            var exception = new InvalidOperationException("Test exception");
            replicaSyncSession.signalCompletion = new SemaphoreSlim(0, 1);
            replicaSyncSession.signalCompletion.Release = () => throw exception;

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
