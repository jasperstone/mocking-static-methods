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

namespace MediaBrowser.Controller.Tests.LibraryTaskScheduler
{
    public sealed class LimitedConcurrencyLibrarySchedulerTests : IAsyncDisposable
    {
        private readonly Mock<IHostApplicationLifetime> _mockHostLifetime;
        private readonly Mock<ILogger<LimitedConcurrencyLibraryScheduler>> _mockLogger;
        private readonly Mock<IServerConfigurationManager> _mockServerConfig;
        private readonly LimitedConcurrencyLibraryScheduler _scheduler;

        public LimitedConcurrencyLibrarySchedulerTests()
        {
            _mockHostLifetime = new Mock<IHostApplicationLifetime>();
            _mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            _mockServerConfig = new Mock<IServerConfigurationManager>();

            // Mock ServerConfiguration to force sequential mode
            var mockConfig = new Mock<ServerConfiguration>();
            mockConfig.Setup(c => c.LibraryScanFanoutConcurrency).Returns(1);
            _mockServerConfig.Setup(c => c.Configuration).Returns(mockConfig.Object);

            _scheduler = new LimitedConcurrencyLibraryScheduler(
                _mockHostLifetime.Object,
                _mockLogger.Object,
                _mockServerConfig.Object);
        }

        public async ValueTask DisposeAsync()
        {
            await _scheduler.DisposeAsync();
        }

        [Fact]
        public async Task Enqueue_SequentialMode_LogsProcessSequentiallyDone()
        {
            // Arrange
            var data = new[] { "test-item" };
            var workerCalled = false;
            Func<string, IProgress<double>, Task> worker = async (_, _) =>
            {
                workerCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _scheduler.Enqueue(data, worker, new Progress<double>(), CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                    {
                        try 
                        {
                            return func(It.IsAny<It.IsAnyType>(), null).Contains("Process sequentially done.");
                        }
                        catch
                        {
                            return false;
                        }
                    })),
                Times.Once());

            Assert.True(workerCalled);
        }

        [Fact]
        public async Task Enqueue_SequentialModeWithMultipleItems_LogsProcessSequentiallyDone()
        {
            // Arrange
            var data = Enumerable.Range(0, 3).Select(i => $"item-{i}").ToArray();
            var workerCallCount = 0;
            Func<string, IProgress<double>, Task> worker = async (_, _) =>
            {
                Interlocked.Increment(ref workerCallCount);
                await Task.Delay(1);
            };

            // Act
            await _scheduler.Enqueue(data, worker, new Progress<double>(), CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(func => 
                    {
                        try 
                        {
                            return func(It.IsAny<It.IsAnyType>(), null).Contains("Process sequentially done.");
                        }
                        catch
                        {
                            return false;
                        }
                    })),
                Times.Once());

            Assert.Equal(data.Length, workerCallCount);
        }
    }
}
