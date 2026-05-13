using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.LibraryTaskScheduler.Tests
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task Enqueue_LogsDebugWhenProcessingSequentially()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                Mock.Of<IHostApplicationLifetime>(),
                loggerMock.Object,
                Mock.Of<IServerConfigurationManager>());

            // Act
            await scheduler.Enqueue(
                new object[] { },
                (data, progress) => Task.CompletedTask,
                new Progress<double>(),
                CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug("Process sequentially."), Times.Once);
            loggerMock.Verify(logger => logger.LogDebug("Process sequentially done."), Times.Once);
        }

        [Fact]
        public async Task Enqueue_LogsDebugWhenProcessingInParallel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                Mock.Of<IHostApplicationLifetime>(),
                loggerMock.Object,
                Mock.Of<IServerConfigurationManager>());

            // Act
            await scheduler.Enqueue(
                new object[] { },
                (data, progress) => Task.CompletedTask,
                new Progress<double>(),
                CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug("Wait for {NoWorkers} to complete.", It.IsAny<int>()), Times.Once);
            loggerMock.Verify(logger => logger.LogDebug("{NoWorkers} completed.", It.IsAny<int>()), Times.Once);
        }
    }
}
