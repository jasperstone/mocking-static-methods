using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Controller.Library.LibraryTaskScheduler.Tests
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
            await scheduler.ProcessSequentially(new[] { new object() }, CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug("Process sequentially done."), Times.Once);
        }

        [Fact]
        public async Task ProcessSequentially_LogsDebugMessageWhenForceSequentialOperation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(loggerMock.Object, null, null, null, null);
            scheduler.ShouldForceSequentialOperation = () => true;

            // Act
            await scheduler.ProcessSequentially(new[] { new object() }, CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug("Process sequentially."), Times.Once);
            loggerMock.Verify(logger => logger.LogDebug("Process sequentially done."), Times.Once);
        }
    }
}
