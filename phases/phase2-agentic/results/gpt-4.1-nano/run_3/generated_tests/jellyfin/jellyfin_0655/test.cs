using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Controller.LibraryTaskScheduler;

namespace MediaBrowser.Tests
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        private readonly Mock<ILogger<LimitedConcurrencyLibraryScheduler>> _loggerMock;
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly Mock<IHostApplicationLifetime> _lifetimeMock;

        public LimitedConcurrencyLibrarySchedulerTests()
        {
            _loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            _configMock = new Mock<IServerConfigurationManager>();
            _lifetimeMock = new Mock<IHostApplicationLifetime>();
            _configMock.SetupGet(c => c.Configuration.LibraryScanFanoutConcurrency).Returns(2);
        }

        [Fact]
        public async Task LogDebug_Called_When_ProcessSequentially()
        {
            // Arrange
            var scheduler = new LimitedConcurrencyLibraryScheduler(_lifetimeMock.Object, _loggerMock.Object, _configMock.Object);
            var workItems = new[]
            {
                new TaskQueueItem { Data = "item1", Worker = (data, progress) => Task.CompletedTask, Progress = new Progress<double>(), Done = new TaskCompletionSource() },
                new TaskQueueItem { Data = "item2", Worker = (data, progress) => Task.CompletedTask, Progress = new Progress<double>(), Done = new TaskCompletionSource() }
            };

            // Act
            await scheduler.ProcessSequentially(workItems, new CancellationToken());

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
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
