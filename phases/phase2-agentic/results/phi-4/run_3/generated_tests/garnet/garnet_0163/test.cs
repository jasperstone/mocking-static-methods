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
        public async Task WaitForSyncCompletionAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaSyncSession = new ReplicaSyncSession
            {
                logger = loggerMock.Object
            };

            // Simulate an exception
            var exception = new Exception("Test exception");
            replicaSyncSession.signalCompletion = new Mock<ManualResetEventSlim>().Object;

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
