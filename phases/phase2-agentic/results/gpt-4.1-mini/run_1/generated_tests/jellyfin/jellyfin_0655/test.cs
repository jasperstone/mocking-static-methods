using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.LibraryTaskScheduler;
using MediaBrowser.Controller.Configuration;
using Microsoft.Extensions.Hosting;

namespace MediaBrowser.Controller.LibraryTaskScheduler.Tests
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        private readonly Mock<ILogger<LimitedConcurrencyLibraryScheduler>> _loggerMock;
        private readonly Mock<IHostApplicationLifetime> _hostAppLifetimeMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigMock;

        public LimitedConcurrencyLibrarySchedulerTests()
        {
            _loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            _hostAppLifetimeMock = new Mock<IHostApplicationLifetime>();
            _serverConfigMock = new Mock<IServerConfigurationManager>();
            _serverConfigMock.SetupGet(s => s.Configuration).Returns(new ServerConfiguration
            {
                LibraryScanFanoutConcurrency = 1 // force sequential operation
            });
            _hostAppLifetimeMock.SetupGet(h => h.ApplicationStopping).Returns(CancellationToken.None);
        }

        [Fact]
        public async Task Enqueue_WhenShouldForceSequentialOperation_LogsProcessSequentiallyAndDone()
        {
            // Arrange
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                _hostAppLifetimeMock.Object,
                _loggerMock.Object,
                _serverConfigMock.Object);

            var data = new[] { 1, 2, 3 };
            var progress = new Progress<double>();
            var cancellationToken = CancellationToken.None;

            // Worker that just completes immediately
            Func<int, IProgress<double>, Task> worker = (item, prog) =>
            {
                prog.Report(50);
                return Task.CompletedTask;
            };

            // Act
            await scheduler.Enqueue(data, worker, progress, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Process sequentially."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Process sequentially done."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal stub for ServerConfiguration to satisfy IServerConfigurationManager.Configuration property
    public class ServerConfiguration
    {
        public int LibraryScanFanoutConcurrency { get; set; }
    }

    public interface IServerConfigurationManager
    {
        ServerConfiguration Configuration { get; }
    }
}
