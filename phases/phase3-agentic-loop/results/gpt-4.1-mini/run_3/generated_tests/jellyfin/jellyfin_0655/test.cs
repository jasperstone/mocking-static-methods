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
    // Mock ServerConfiguration class with the needed property
    public class ServerConfiguration
    {
        public int LibraryScanFanoutConcurrency { get; set; }
    }

    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task Enqueue_LogsProcessSequentiallyDone_WhenSequentialProcessing()
        {
            // Arrange
            var hostAppLifetimeMock = new Mock<IHostApplicationLifetime>();
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var serverConfigManagerMock = new Mock<IServerConfigurationManager>();

            // Setup Configuration property to return our mock ServerConfiguration
            serverConfigManagerMock.SetupGet(m => m.Configuration).Returns(new ServerConfiguration
            {
                LibraryScanFanoutConcurrency = 1 // Force sequential operation
            });

            var scheduler = new LimitedConcurrencyLibraryScheduler(
                hostAppLifetimeMock.Object,
                loggerMock.Object,
                serverConfigManagerMock.Object);

            var data = new[] { 1, 2, 3 };
            var progressMock = new Mock<IProgress<double>>();
            var cancellationToken = CancellationToken.None;

            // Act
            await scheduler.Enqueue<int>(
                data,
                async (item, progress) =>
                {
                    await Task.Delay(10);
                    progress.Report(50);
                },
                progressMock.Object,
                cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially done.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
