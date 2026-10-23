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
    // Minimal stub for ServerConfiguration to allow mocking
    public class ServerConfiguration
    {
        public int LibraryScanFanoutConcurrency { get; set; }
    }

    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task Enqueue_ShouldLogProcessSequentiallyDone_WhenForceSequentialOperationIsTrue()
        {
            // Arrange
            var hostAppLifetimeMock = new Mock<IHostApplicationLifetime>();
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var serverConfigManagerMock = new Mock<IServerConfigurationManager>();
            var serverConfig = new ServerConfiguration { LibraryScanFanoutConcurrency = 1 };
            serverConfigManagerMock.SetupGet(m => m.Configuration).Returns(serverConfig);

            var scheduler = new LimitedConcurrencyLibraryScheduler(
                hostAppLifetimeMock.Object,
                loggerMock.Object,
                serverConfigManagerMock.Object);

            var data = new[] { 1, 2, 3 };
            Func<int, IProgress<double>, Task> worker = (item, progress) =>
            {
                progress.Report(50);
                return Task.CompletedTask;
            };
            var progress = new Progress<double>();

            var cancellationToken = CancellationToken.None;

            // Act
            await scheduler.Enqueue(data, worker, progress, cancellationToken);

            // Assert
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
