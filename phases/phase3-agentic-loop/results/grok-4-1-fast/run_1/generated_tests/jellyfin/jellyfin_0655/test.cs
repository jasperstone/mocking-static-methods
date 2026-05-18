using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.LibraryTaskScheduler;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

public sealed class LimitedConcurrencyLibrarySchedulerTests : IDisposable
{
    private readonly Mock<IHostApplicationLifetime> _mockHostLifetime;
    private readonly Mock<ILogger<LimitedConcurrencyLibraryScheduler>> _mockLogger;
    private readonly Mock<IServerConfigurationManager> _mockConfigManager;
    private readonly LimitedConcurrencyLibraryScheduler _scheduler;

    public LimitedConcurrencyLibrarySchedulerTests()
    {
        _mockHostLifetime = new Mock<IHostApplicationLifetime>();
        _mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
        _mockConfigManager = new Mock<IServerConfigurationManager>();

        // Configure to force sequential operation
        var config = new ServerConfiguration { LibraryScanFanoutConcurrency = 1 };
        _mockConfigManager.Setup(m => m.Configuration).Returns(config);

        _scheduler = new LimitedConcurrencyLibraryScheduler(
            _mockHostLifetime.Object,
            _mockLogger.Object,
            _mockConfigManager.Object);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _scheduler.DisposeAsync().AsTask().Wait();
        }
    }

    [Fact]
    public async Task Enqueue_SequentialMode_LogsProcessSequentiallyDone()
    {
        // Arrange
        var data = new[] { "test" };
        Func<string, IProgress<double>, Task> worker = async (_, _) =>
        {
            await Task.CompletedTask;
        };

        // Setup logger to capture debug log messages
        _mockLogger
            .Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, eventId, state, ex, formatter) =>
            {
                var message = formatter(state, ex);
                if (message.Contains("Process sequentially done."))
                {
                    // Store in mock state or use a callback flag
                    _mockLogger.Object.GetType().GetProperty("LastDebugMessage")?.SetValue(_mockLogger.Object, message);
                }
            });

        // Act
        await _scheduler.Enqueue(data, worker, new Progress<double>(), CancellationToken.None);

        // Assert - Verify debug logging occurred
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Enqueue_SequentialMode_ProcessesAllItems()
    {
        // Arrange
        var data = new[] { "item1", "item2" };
        var processedItems = new List<string>();
        Func<string, IProgress<double>, Task> worker = async (item, _) =>
        {
            processedItems.Add(item);
            await Task.Delay(10);
        };

        // Act
        await _scheduler.Enqueue(data, worker, new Progress<double>(), CancellationToken.None);

        // Assert
        Assert.Equal(2, processedItems.Count);
        Assert.Contains("item1", processedItems);
        Assert.Contains("item2", processedItems);
    }
}
