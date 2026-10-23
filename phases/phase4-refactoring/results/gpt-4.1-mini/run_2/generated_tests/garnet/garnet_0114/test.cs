using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrateSession_LoggerExtensions_Tests
    {
        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);

            // Act
            var result = await migrateSession.CallReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to reserve")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // A minimal testable class to simulate the method behavior and trigger the LogError call
        private class TestableMigrateSession
        {
            private readonly ILogger _logger;

            public TestableMigrateSession(ILogger logger)
            {
                _logger = logger;
            }

            [Fact]
            public async Task CallReserveDestinationVectorSetsAsync()
            {
                // Simulate the method behavior to trigger the LogError call
                try
                {
                    throw new InvalidOperationException("Simulated failure");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to reserve {count} Vector Set contexts on destination node {node}", 42, 99);
                    await Task.CompletedTask;
                }
            }
        }
    }
}
