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
    public async Task Enqueue_ShouldLogDebug_WhenProcessSequentially()
    {
        // Arrange
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
        var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
        var serverConfiguration = new ServerConfiguration { LibraryScanFanoutConcurrency = 1 };

        serverConfigurationManagerMock.Setup(x => x.Configuration).Returns(serverConfiguration);

        var scheduler = new LimitedConcurrencyLibraryScheduler(
            hostApplicationLifetimeMock.Object,
            loggerMock.Object,
            serverConfigurationManagerMock.Object);

        var data = new[] { "item1", "item2" };
        var progress = new Progress<double>();
        var cancellationToken = CancellationToken.None;

        // Act
        await scheduler.Enqueue(data, (item, progress) => Task.CompletedTask, progress, cancellationToken);

        // Assert
        loggerMock.Verify(
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
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
        var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
        var serverConfiguration = new ServerConfiguration { LibraryScanFanoutConcurrency = 1 };

        serverConfigurationManagerMock.Setup(x => x.Configuration).Returns(serverConfiguration);

        var scheduler = new LimitedConcurrencyLibraryScheduler(
            hostApplicationLifetimeMock.Object,
            loggerMock.Object,
            serverConfigurationManagerMock.Object);

        var data = new[] { "item1", "item2" };
        var progress = new Progress<double>();
        var cancellationToken = CancellationToken.None;

        // Act
        await scheduler.Enqueue(data, (item, progress) => Task.CompletedTask, progress, cancellationToken);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially done")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
