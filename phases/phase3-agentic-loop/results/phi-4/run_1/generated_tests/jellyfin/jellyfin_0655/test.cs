using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.LibraryTaskScheduler;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Hosting; // For IHostApplicationLifetime
using MediaBrowser.Model.Configuration; // For IServerConfigurationManager

public class LimitedConcurrencyLibrarySchedulerTests
{
    [Fact]
    public async Task ProcessSequentially_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
        var configurationMock = new Mock<IServerConfiguration>();
        configurationMock.Setup(c => c.LibraryScanFanoutConcurrency).Returns(1);
        serverConfigurationManagerMock.Setup(s => s.Configuration).Returns(configurationMock.Object);

        var scheduler = new LimitedConcurrencyLibraryScheduler(
            Mock.Of<IHostApplicationLifetime>(),
            loggerMock.Object,
            serverConfigurationManagerMock.Object);

        var cancellationToken = new CancellationToken(false);

        // Act
        await scheduler.Enqueue(new object[0], (data, progress) => Task.CompletedTask, null, cancellationToken);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug("Process sequentially done."),
            Times.Once);
    }
}
