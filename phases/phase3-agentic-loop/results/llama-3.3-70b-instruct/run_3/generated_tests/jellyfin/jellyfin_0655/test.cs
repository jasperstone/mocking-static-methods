using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.LibraryTaskScheduler;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task Enqueue_LogsDebugWhenProcessingSequentially()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                new Mock<IHostApplicationLifetime>().Object,
                loggerMock.Object,
                new Mock<IServerConfigurationManager>().Object);

            // Act
            await scheduler.Enqueue(new object[] { new object() }, (data, progress) => Task.CompletedTask, new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Process sequentially."), Times.Once);
            loggerMock.Verify(l => l.LogDebug("Process sequentially done."), Times.Once);
        }

        [Fact]
        public async Task Enqueue_LogsDebugWhenProcessingInParallel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                new Mock<IHostApplicationLifetime>().Object,
                loggerMock.Object,
                new Mock<IServerConfigurationManager>().Object);

            // Act
            await scheduler.Enqueue(new object[] { new object(), new object() }, (data, progress) => Task.CompletedTask, new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Wait for {NoWorkers} to complete.", 2), Times.Once);
            loggerMock.Verify(l => l.LogDebug("{NoWorkers} completed.", 2), Times.Once);
        }
    }
}
