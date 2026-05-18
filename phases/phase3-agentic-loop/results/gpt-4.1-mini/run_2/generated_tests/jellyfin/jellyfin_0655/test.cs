using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.LibraryTaskScheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.LibraryTaskScheduler
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task Enqueue_ShouldLogProcessSequentiallyDone_WhenSequentialProcessing()
        {
            // Arrange
            var hostAppLifetimeMock = new Mock<IHostApplicationLifetime>();
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var serverConfigManagerMock = new Mock<IServerConfigurationManager>();
            var serverConfigMock = new Mock<IServerConfiguration>();
            serverConfigMock.SetupGet(c => c.LibraryScanFanoutConcurrency).Returns(1); // Force sequential operation
            serverConfigManagerMock.SetupGet(m => m.Configuration).Returns(serverConfigMock.Object);

            var scheduler = new LimitedConcurrencyLibraryScheduler(
                hostAppLifetimeMock.Object,
                loggerMock.Object,
                serverConfigManagerMock.Object);

            var data = new[] { 1, 2, 3 };
            var progressMock = new Mock<IProgress<double>>();
            var cancellationToken = CancellationToken.None;

            // Worker that just completes immediately
            Func<int, IProgress<double>, Task> worker = (item, progress) =>
            {
                progress.Report(50);
                return Task.CompletedTask;
            };

            // Act
            await scheduler.Enqueue(data, worker, progressMock.Object, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Process sequentially."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Process sequentially done."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
