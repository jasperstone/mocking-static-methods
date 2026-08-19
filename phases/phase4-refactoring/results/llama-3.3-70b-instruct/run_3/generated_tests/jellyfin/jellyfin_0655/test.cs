using Xunit;
using Moq;
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
            var scheduler = new LimitedConcurrencyLibraryScheduler(Mock.Of<IHostApplicationLifetime>(), loggerMock.Object, Mock.Of<IServerConfigurationManager>());

            // Act
            await scheduler.RunAsync(default);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Process sequentially."), Times.Once);
            loggerMock.Verify(l => l.LogDebug("Process sequentially done."), Times.Once);
        }

        [Fact]
        public async Task ProcessParallel_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(Mock.Of<IHostApplicationLifetime>(), loggerMock.Object, Mock.Of<IServerConfigurationManager>());

            // Act
            await scheduler.RunAsync(default);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Wait for {NoWorkers} to complete.", It.IsAny<int>()), Times.Once);
            loggerMock.Verify(l => l.LogDebug("{NoWorkers} completed.", It.IsAny<int>()), Times.Once);
        }
    }
}
