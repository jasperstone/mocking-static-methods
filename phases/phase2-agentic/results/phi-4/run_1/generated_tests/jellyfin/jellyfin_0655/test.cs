using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task ProcessSequentially_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cancellationToken = new CancellationTokenSource().Token;
            var scheduler = new LimitedConcurrencyLibraryScheduler(loggerMock.Object, cancellationToken);

            // Act
            await scheduler.ProcessSequentially();

            // Assert
            loggerMock.Verify(
                logger => logger.LogDebug("Process sequentially done."),
                Times.Once);
        }
    }
}
