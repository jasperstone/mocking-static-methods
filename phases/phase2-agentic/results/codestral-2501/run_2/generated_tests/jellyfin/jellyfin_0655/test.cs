using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.LibraryTaskScheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class LimitedConcurrencyLibrarySchedulerTests
{
    private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;
    private readonly Mock<ILogger<LimitedConcurrencyLibraryScheduler>> _loggerMock;
    private readonly Mock<IServerConfigurationManager> _serverConfigurationManagerMock;
    private readonly LimitedConcurrencyLibraryScheduler _scheduler;

    public LimitedConcurrencyLibrarySchedulerTests()
    {
        _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
        _loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        _serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

        _scheduler = new LimitedConcurrencyLibraryScheduler(
            _hostApplicationLifetimeMock.Object,
            _loggerMock.Object,
            _serverConfigurationManagerMock.Object);
    }

    [Fact]
    public async Task Enqueue_ShouldLogDebug_WhenProcessSequentially()
    {
        // Arrange
        var data = new int[] { 1, 2, 3 };
        var progress = new Progress<double>();
        var cancellationToken = CancellationToken.None;

        _serverConfigurationManagerMock.Setup(x => x.Configuration.LibraryScanFanoutConcurrency).Returns(1);

        // Act
        await _scheduler.Enqueue(data, (item, progress) => Task.CompletedTask, progress, cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task Enqueue_ShouldLogDebug_WhenProcessSequentiallyDone()
    {
        // Arrange
        var data = new int[] { 1, 2, 3 };
        var progress = new Progress<double>();
        var cancellationToken = CancellationToken.None;

        _serverConfigurationManagerMock.Setup(x => x.Configuration.LibraryScanFanoutConcurrency).Returns(1);

        // Act
        await _scheduler.Enqueue(data, (item, progress) => Task.CompletedTask, progress, cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially done")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task Enqueue_ShouldLogDebug_WhenWaitForWorkersToComplete()
    {
        // Arrange
        var data = new int[] { 1, 2, 3 };
        var progress = new Progress<double>();
        var cancellationToken = CancellationToken.None;

        _serverConfigurationManagerMock.Setup(x => x.Configuration.LibraryScanFanoutConcurrency).Returns(2);

        // Act
        await _scheduler.Enqueue(data, (item, progress) => Task.CompletedTask, progress, cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Wait for")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task Enqueue_ShouldLogDebug_WhenWorkersCompleted()
    {
        // Arrange
        var data = new int[] { 1, 2, 3 };
        var progress = new Progress<double>();
        var cancellationToken = CancellationToken.None;

        _serverConfigurationManagerMock.Setup(x => x.Configuration.LibraryScanFanoutConcurrency).Returns(2);

        // Act
        await _scheduler.Enqueue(data, (item, progress) => Task.CompletedTask, progress, cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("completed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
