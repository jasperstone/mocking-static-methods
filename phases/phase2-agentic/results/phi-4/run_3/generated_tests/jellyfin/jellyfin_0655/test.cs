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
            var cancellationToken = new CancellationToken();
            var scheduler = new LimitedConcurrencyLibraryScheduler(loggerMock.Object);

            // Act
            await scheduler.ProcessSequentially(cancellationToken);

            // Assert
            loggerMock.Verify(
                logger => logger.LogDebug("Process sequentially done."),
                Times.Once);
        }
    }
}
