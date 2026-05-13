using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.LibraryTaskScheduler;
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

        var scheduler = new LimitedConcurrencyLibraryScheduler(
            hostApplicationLifetimeMock.Object,
            loggerMock.Object,
            serverConfigurationManagerMock.Object);

        var data = new object[] { new object() };
        var worker = new Func<object, IProgress<double>, Task>((obj, progress) => Task.CompletedTask);
        var progress = new Progress<double>();
        var cancellationToken = CancellationToken.None;

        // Act
        await scheduler.Enqueue(data, worker, progress, cancellationToken);

        // Assert
        loggerMock.Verify(
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
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
        var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

        var scheduler = new LimitedConcurrencyLibraryScheduler(
            hostApplicationLifetimeMock.Object,
            loggerMock.Object,
            serverConfigurationManagerMock.Object);

        var data = new object[] { new object() };
        var worker = new Func<object, IProgress<double>, Task>((obj, progress) => Task.CompletedTask);
        var progress = new Progress<double>();
        var cancellationToken = CancellationToken.None;

        // Act
        await scheduler.Enqueue(data, worker, progress, cancellationToken);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially done")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Enqueue_ShouldLogDebug_WhenWaitForWorkersToComplete()
    {
        // Arrange
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
        var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

        var scheduler = new LimitedConcurrencyLibraryScheduler(
            hostApplicationLifetimeMock.Object,
            loggerMock.Object,
            serverConfigurationManagerMock.Object);

        var data = new object[] { new object() };
        var worker = new Func<object, IProgress<double>, Task>((obj, progress) => Task.CompletedTask);
        var progress = new Progress<double>();
        var cancellationToken = CancellationToken.None;

        // Act
        await scheduler.Enqueue(data, worker, progress, cancellationToken);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Wait for")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Enqueue_ShouldLogDebug_WhenWorkersCompleted()
    {
        // Arrange
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
        var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

        var scheduler = new LimitedConcurrencyLibraryScheduler(
            hostApplicationLifetimeMock.Object,
            loggerMock.Object,
            serverConfigurationManagerMock.Object);

        var data = new object[] { new object() };
        var worker = new Func<object, IProgress<double>, Task>((obj, progress) => Task.CompletedTask);
        var progress = new Progress<double>();
        var cancellationToken = CancellationToken.None;

        // Act
        await scheduler.Enqueue(data, worker, progress, cancellationToken);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("completed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
