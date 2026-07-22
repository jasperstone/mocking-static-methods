using Xunit;
using Moq;
using MediaBrowser.Controller.LibraryTaskScheduler;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System;

public class LimitedConcurrencyLibrarySchedulerTests
{
    private class TestLimitedConcurrencyLibraryScheduler : LimitedConcurrencyLibraryScheduler
    {
        public TestLimitedConcurrencyLibraryScheduler(
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<LimitedConcurrencyLibraryScheduler> logger,
            IServerConfigurationManager serverConfigurationManager)
            : base(hostApplicationLifetime, logger, serverConfigurationManager)
        {
        }

        public override bool ShouldForceSequentialOperation()
        {
            return true;
        }
    }

    [Fact]
    public async Task Enqueue_ShouldLogDebug_WhenProcessSequentially()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        var mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
        var mockServerConfigurationManager = new Mock<IServerConfigurationManager>();

        var scheduler = new TestLimitedConcurrencyLibraryScheduler(
            mockHostApplicationLifetime.Object,
            mockLogger.Object,
            mockServerConfigurationManager.Object);

        var data = new[] { "data1", "data2" };
        var progress = new Progress<double>();
        var cancellationToken = new CancellationToken();

        // Act
        await scheduler.Enqueue(data, (d, p) => Task.CompletedTask, progress, cancellationToken);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
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
        var mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        var mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
        var mockServerConfigurationManager = new Mock<IServerConfigurationManager>();

        var scheduler = new TestLimitedConcurrencyLibraryScheduler(
            mockHostApplicationLifetime.Object,
            mockLogger.Object,
            mockServerConfigurationManager.Object);

        var data = new[] { "data1", "data2" };
        var progress = new Progress<double>();
        var cancellationToken = new CancellationToken();

        // Act
        await scheduler.Enqueue(data, (d, p) => Task.CompletedTask, progress, cancellationToken);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially done")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
