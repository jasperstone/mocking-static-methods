using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.LibraryTaskScheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

public sealed class LimitedConcurrencyLibrarySchedulerTests : IDisposable
{
    private readonly Mock<IHostApplicationLifetime> _mockLifetime;
    private readonly Mock<ILogger<LimitedConcurrencyLibraryScheduler>> _mockLogger;
    private readonly Mock<IServerConfigurationManager> _mockConfigManager;
    private readonly LimitedConcurrencyLibraryScheduler _scheduler;

    public LimitedConcurrencyLibrarySchedulerTests()
    {
        _mockLifetime = new Mock<IHostApplicationLifetime>();
        _mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        _mockConfigManager = new Mock<IServerConfigurationManager>();

        // Mock the configuration chain without needing ServerConfiguration type
        var mockConfig = new Mock<object>();
        mockConfig.SetupProperty(o => ((dynamic)o).LibraryScanFanoutConcurrency, 1);
        _mockConfigManager.SetupGet(c => c.Configuration).Returns(mockConfig.Object);

        _scheduler = new LimitedConcurrencyLibraryScheduler(
            _mockLifetime.Object,
            _mockLogger.Object,
            _mockConfigManager.Object);
    }

    public void Dispose()
    {
        _scheduler.DisposeAsync().AsTask().Wait();
    }

    [Fact]
    public async Task Enqueue_SequentialMode_LogsProcessSequentiallyDone()
    {
        // Arrange
        var data = new[] { "test" };
        bool workerCalled = false;
        Func<string, IProgress<double>, Task> mockWorker = async (_, _) =>
        {
            workerCalled = true;
            await Task.CompletedTask;
        };

        // Act
        await _scheduler.Enqueue(data, mockWorker, new Progress<double>(), CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => t != null && t.ToString()!.Contains("Process sequentially done.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        Assert.True(workerCalled);
    }
}
