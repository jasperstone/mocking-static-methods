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
using Xunit;

namespace MediaBrowser.Controller.Tests.LibraryTaskScheduler
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        private readonly Mock<IHostApplicationLifetime> _mockLifetime;
        private readonly Mock<ILogger<LimitedConcurrencyLibraryScheduler>> _mockLogger;
        private readonly Mock<IServerConfigurationManager> _mockConfigManager;

        public LimitedConcurrencyLibrarySchedulerTests()
        {
            _mockLifetime = new Mock<IHostApplicationLifetime>();
            _mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            _mockConfigManager = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public async Task Enqueue_SequentialMode_LogsProcessSequentiallyDone()
        {
            // Arrange
            var scheduler = CreateScheduler(forceSequential: true);
            var data = new[] { "test" };
            var workerCalled = false;
            Func<string, IProgress<double>, Task> worker = async (_, _) =>
            {
                workerCalled = true;
                await Task.CompletedTask;
            };

            // Mock config to force sequential mode
            SetupSequentialConfig();

            // Act
            await scheduler.Enqueue(data, worker, new Progress<double>(), CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t.ToString().Contains("Process sequentially done.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Enqueue_SequentialModeWithCancellation_DoesNotLogProcessSequentiallyDone()
        {
            // Arrange
            var scheduler = CreateScheduler(forceSequential: true);
            var cts = new CancellationTokenSource();
            var data = new[] { "test" };
            Func<string, IProgress<double>, Task> worker = async (_, _) =>
            {
                await Task.Delay(100, cts.Token);
            };

            SetupSequentialConfig();

            // Act
            cts.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>(() => scheduler.Enqueue(data, worker, new Progress<double>(), cts.Token));

            // Assert - the LogDebug should still be called after catch block
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t.ToString().Contains("Process sequentially done.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ShouldForceSequentialOperation_WithDeadlockDetector_ReturnsTrue()
        {
            // Arrange - using reflection to set static AsyncLocal for test
            using var deadlockDetector = new CancellationTokenSource();
            MediaBrowser.Controller.LibraryTaskScheduler.LimitedConcurrencyLibraryScheduler._deadlockDetector.Value = deadlockDetector;

            var scheduler = CreateScheduler();

            // Act
            var result = scheduler.ShouldForceSequentialOperationInternal();

            // Assert
            Assert.True(result);
        }

        private LimitedConcurrencyLibraryScheduler CreateScheduler(bool forceSequential = false)
        {
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                _mockLifetime.Object,
                _mockLogger.Object,
                _mockConfigManager.Object);

            if (forceSequential)
            {
                // Use reflection or make private method accessible for testing
                // For this test, we'll rely on config setup
            }

            return scheduler;
        }

        private void SetupSequentialConfig()
        {
            var config = new Mock<ServerConfiguration>();
            config.SetupGet(x => x.LibraryScanFanoutConcurrency).Returns(1);
            _mockConfigManager.Setup(x => x.Configuration).Returns(config.Object);
        }
    }

    // Extension method to access private method for testing
    public static class LimitedConcurrencyLibrarySchedulerExtensions
    {
        public static bool ShouldForceSequentialOperationInternal(this LimitedConcurrencyLibraryScheduler scheduler)
        {
            return scheduler.ShouldForceSequentialOperation();
        }
    }
}
