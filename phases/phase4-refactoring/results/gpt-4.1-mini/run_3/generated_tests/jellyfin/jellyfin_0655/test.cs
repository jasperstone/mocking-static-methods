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
        private class FakeServerConfigurationManager : IServerConfigurationManager
        {
            public IServerApplicationPaths ApplicationPaths => throw new NotImplementedException();

            public MediaBrowser.Model.Configuration.ServerConfiguration Configuration { get; }

            public FakeServerConfigurationManager(int concurrency)
            {
                Configuration = new MediaBrowser.Model.Configuration.ServerConfiguration
                {
                    LibraryScanFanoutConcurrency = concurrency
                };
            }
        }

        [Fact]
        public async Task Enqueue_SequentialProcessing_LogsProcessSequentiallyDone()
        {
            // Arrange
            var hostAppLifetimeMock = new Mock<IHostApplicationLifetime>();
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();

            var serverConfigManager = new FakeServerConfigurationManager(1);

            var scheduler = new LimitedConcurrencyLibraryScheduler(
                hostAppLifetimeMock.Object,
                loggerMock.Object,
                serverConfigManager);

            var data = new[] { 1, 2, 3 };
            var progress = new Mock<IProgress<double>>();
            var cancellationToken = CancellationToken.None;

            // Act
            await scheduler.Enqueue<int>(
                data,
                async (item, prog) =>
                {
                    await Task.Delay(10);
                    prog.Report(50);
                },
                progress.Object,
                cancellationToken);

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
