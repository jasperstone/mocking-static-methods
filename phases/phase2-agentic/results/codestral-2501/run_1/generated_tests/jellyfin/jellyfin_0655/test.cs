using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.LibraryTaskScheduler;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller.Configuration;
using System;

namespace MediaBrowser.Controller.Tests.LibraryTaskScheduler
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
        private readonly Mock<ILogger<LimitedConcurrencyLibraryScheduler>> _mockLogger;
        private readonly Mock<IServerConfigurationManager> _mockServerConfigurationManager;
        private readonly LimitedConcurrencyLibraryScheduler _scheduler;

        public LimitedConcurrencyLibrarySchedulerTests()
        {
            _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
            _mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            _mockServerConfigurationManager = new Mock<IServerConfigurationManager>();
            _scheduler = new LimitedConcurrencyLibraryScheduler(
                _mockHostApplicationLifetime.Object,
                _mockLogger.Object,
                _mockServerConfigurationManager.Object);
        }

        [Fact]
        public async Task Enqueue_ShouldLogDebug_WhenProcessSequentially()
        {
            // Arrange
            var data = new int[] { 1, 2, 3 };
            var progress = new Progress<double>();
            var cancellationToken = CancellationToken.None;

            _mockServerConfigurationManager.Setup(x => x.Configuration.LibraryScanFanoutConcurrency).Returns(1);

            // Act
            await _scheduler.Enqueue(data, (item, progress) => Task.CompletedTask, progress, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task Enqueue_ShouldLogDebug_WhenProcessSequentiallyDone()
        {
            // Arrange
            var data = new int[] { 1, 2, 3 };
            var progress = new Progress<double>();
            var cancellationToken = CancellationToken.None;

            _mockServerConfigurationManager.Setup(x => x.Configuration.LibraryScanFanoutConcurrency).Returns(1);

            // Act
            await _scheduler.Enqueue(data, (item, progress) => Task.CompletedTask, progress, cancellationToken);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially done")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
