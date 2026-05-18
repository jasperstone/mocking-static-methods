using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.LibraryTaskScheduler;

namespace MediaBrowser.Tests
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        private readonly Mock<ILogger<LimitedConcurrencyLibraryScheduler>> _loggerMock;
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly Mock<IHostApplicationLifetime> _hostLifetimeMock;

        public LimitedConcurrencyLibrarySchedulerTests()
        {
            _loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            _configMock = new Mock<IServerConfigurationManager>();
            _hostLifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public async Task ProcessSequentiallyLogsDebugAndReturns()
        {
            // Arrange
            _configMock.Setup(c => c.Configuration.LibraryScanFanoutConcurrency).Returns(1);
            var scheduler = new LimitedConcurrencyLibraryScheduler(_hostLifetimeMock.Object, _loggerMock.Object, _configMock.Object);

            // Act
            // Since the actual method that logs "Process sequentially." is private, we simulate the condition
            // by calling Enqueue with dummy data and a worker that does nothing.
            var dummyProgress = new Mock<IProgress<double>>();
            var dummyData = new int[] { 1, 2, 3 };
            var cts = new CancellationTokenSource();

            await scheduler.Enqueue(dummyData, async (item, progress) =>
            {
                await Task.Delay(10);
            }, dummyProgress.Object, cts.Token);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
