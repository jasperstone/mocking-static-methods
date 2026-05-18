using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.LibraryTaskScheduler;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class LimitedConcurrencyLibrarySchedulerTests
{
    [Fact]
    public async Task LogDebug_ProcessSequentiallyDone_ShouldBeCalled()
    {
        // Arrange
        var mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
        var mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        var mockServerConfigurationManager = new Mock<IServerConfigurationManager>();
        mockServerConfigurationManager.Setup(x => x.Configuration).Returns(new ServerConfiguration { LibraryScanFanoutConcurrency = 1 });

        var scheduler = new LimitedConcurrencyLibraryScheduler(
            mockHostApplicationLifetime.Object,
            mockLogger.Object,
            mockServerConfigurationManager.Object);

        // Act
        await scheduler.Enqueue(new object[0], (data, progress) => Task.CompletedTask, new Progress<double>(), CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially done.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);
    }
}
