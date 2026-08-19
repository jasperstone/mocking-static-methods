using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.LibraryTaskScheduler
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task LogDebug_Called_When_Processing_Sequentially()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                Mock.Of<IHostApplicationLifetime>(),
                loggerMock.Object,
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IDeadlockDetector>(),
                Mock.Of<IWorkerFactory>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IConfiguration>(),
                Mock.Of<ILoggerFactory>()
            );

            // Act
            await scheduler.StartAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Process sequentially."), Times.Once);
            loggerMock.Verify(l => l.LogDebug("Process sequentially done."), Times.Once);
        }

        [Fact]
        public async Task LogDebug_Called_When_Waiting_For_Workers_To_Complete()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                Mock.Of<IHostApplicationLifetime>(),
                loggerMock.Object,
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IDeadlockDetector>(),
                Mock.Of<IWorkerFactory>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IConfiguration>(),
                Mock.Of<ILoggerFactory>()
            );

            // Act
            await scheduler.StartAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Wait for {NoWorkers} to complete.", It.IsAny<int>()), Times.Once);
            loggerMock.Verify(l => l.LogDebug("{NoWorkers} completed.", It.IsAny<int>()), Times.Once);
        }
    }
}
