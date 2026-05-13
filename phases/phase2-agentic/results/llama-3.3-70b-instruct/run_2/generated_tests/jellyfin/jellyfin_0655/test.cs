using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.LibraryTaskScheduler
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task ProcessSequentially_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(loggerMock.Object, null, null, null, null);

            // Act
            await scheduler.ProcessSequentially(new[] { new WorkItem() }, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Process sequentially."), Times.Once);
            loggerMock.Verify(l => l.LogDebug("Process sequentially done."), Times.Once);
        }

        [Fact]
        public async Task ProcessParallel_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(loggerMock.Object, null, null, null, null);

            // Act
            await scheduler.ProcessParallel(new[] { new WorkItem() }, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Wait for {NoWorkers} to complete.", 1), Times.Once);
            loggerMock.Verify(l => l.LogDebug("{NoWorkers} completed.", 1), Times.Once);
        }
    }
}
