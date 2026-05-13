using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.LibraryTaskScheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.LibraryTaskScheduler.Tests
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task Enqueue_ShouldLogProcessSequentiallyAndProcessItems_WhenShouldForceSequentialOperationIsTrue()
        {
            // Arrange
            var hostAppLifetimeMock = new Mock<IHostApplicationLifetime>();
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var serverConfigManagerMock = new Mock<IServerConfigurationManager>();
            var serverConfigMock = new Mock<IServerConfiguration>();
            serverConfigMock.SetupGet(c => c.LibraryScanFanoutConcurrency).Returns(1);
            serverConfigManagerMock.SetupGet(m => m.Configuration).Returns(serverConfigMock.Object);

            var scheduler = new LimitedConcurrencyLibraryScheduler(hostAppLifetimeMock.Object, loggerMock.Object, serverConfigManagerMock.Object);

            var progress = new Progress<double>();
            var cancellationToken = CancellationToken.None;

            var data = new[] { 1, 2, 3 };

            // Act
            await scheduler.Enqueue<int>(data, async (item, prog) =>
            {
                await Task.Delay(10);
            }, progress, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially done.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
