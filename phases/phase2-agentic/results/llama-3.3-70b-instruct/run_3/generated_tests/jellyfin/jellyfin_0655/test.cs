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
        public async Task Enqueue_LogsSequentialProcessing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            serverConfigurationManagerMock.Setup(s => s.Configuration.LibraryScanFanoutConcurrency).Returns(1);

            var scheduler = new LimitedConcurrencyLibraryScheduler(
                hostApplicationLifetimeMock.Object,
                loggerMock.Object,
                serverConfigurationManagerMock.Object);

            // Act
            await scheduler.Enqueue(
                new object[] { new object() },
                (data, progress) => Task.CompletedTask,
                new Progress<double>(),
                CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Process sequentially."), Times.Once);
            loggerMock.Verify(l => l.LogDebug("Process sequentially done."), Times.Once);
        }

        [Fact]
        public async Task Enqueue_LogsParallelProcessing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            serverConfigurationManagerMock.Setup(s => s.Configuration.LibraryScanFanoutConcurrency).Returns(2);

            var scheduler = new LimitedConcurrencyLibraryScheduler(
                hostApplicationLifetimeMock.Object,
                loggerMock.Object,
                serverConfigurationManagerMock.Object);

            // Act
            await scheduler.Enqueue(
                new object[] { new object(), new object() },
                (data, progress) => Task.CompletedTask,
                new Progress<double>(),
                CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Wait for {NoWorkers} to complete.", 2), Times.Once);
            loggerMock.Verify(l => l.LogDebug("{NoWorkers} completed.", 2), Times.Once);
        }
    }
}
