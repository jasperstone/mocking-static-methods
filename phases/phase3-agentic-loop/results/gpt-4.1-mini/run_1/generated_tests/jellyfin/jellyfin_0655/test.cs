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
        public async Task Enqueue_LogsProcessSequentiallyDone_WhenSequentialProcessing()
        {
            // Arrange
            var hostAppLifetimeMock = new Mock<IHostApplicationLifetime>();
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();

            var serverConfigMock = new Mock<ServerConfiguration>();
            serverConfigMock.SetupProperty(c => c.LibraryScanFanoutConcurrency, 1);

            var serverConfigManagerMock = new Mock<IServerConfigurationManager>();
            serverConfigManagerMock.SetupGet(m => m.Configuration).Returns(serverConfigMock.Object);

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
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
