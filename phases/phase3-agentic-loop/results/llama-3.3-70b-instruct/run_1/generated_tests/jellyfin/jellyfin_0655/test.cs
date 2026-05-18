using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Controller.LibraryTaskScheduler.Tests
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task ProcessSequentially_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(null, loggerMock.Object, null);

            // Act
            await scheduler.RunAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Process sequentially done."), Times.Once);
        }

        [Fact]
        public async Task ProcessSequentially_LogsDebugMessageWhenForceSequentialOperation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(null, loggerMock.Object, null);

            // Act
            await scheduler.RunAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Process sequentially."), Times.Once);
        }
    }
}
