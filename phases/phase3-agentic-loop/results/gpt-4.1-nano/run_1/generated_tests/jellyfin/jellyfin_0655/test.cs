using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.LibraryTaskScheduler;

namespace MediaBrowser.Tests
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task ProcessSequentiallyLogsDebugAndReturns()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var mockHostLifetime = new Mock<IHostApplicationLifetime>();
            var mockConfigManager = new Mock<IServerConfigurationManager>();
            mockConfigManager.Setup(c => c.Configuration.LibraryScanFanoutConcurrency).Returns(0);
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                mockHostLifetime.Object,
                mockLogger.Object,
                mockConfigManager.Object);

            // Use reflection or internal access to set private fields if needed
            // For simplicity, assume we can call the method directly or test the internal logic
            // Here, we will invoke the private method via reflection for the test

            var method = typeof(LimitedConcurrencyLibraryScheduler).GetMethod("ProcessSequentially", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Create dummy work items
            var workItems = new[]
            {
                new TaskQueueItem { Data = "item1", Progress = new Progress<double>(), Worker = (data, progress) => Task.CompletedTask, Done = new TaskCompletionSource() },
                new TaskQueueItem { Data = "item2", Progress = new Progress<double>(), Worker = (data, progress) => Task.CompletedTask, Done = new TaskCompletionSource() }
            };

            // Act
            await (Task)method.Invoke(scheduler, new object[] { workItems, CancellationToken.None });

            // Assert
            mockLogger.Verify(l => l.LogDebug("Process sequentially."), Times.Once);
            mockLogger.Verify(l => l.LogDebug("Process sequentially done."), Times.Once);
        }

        [Fact]
        public async Task LogsDebugWhenSchedulingCleanup()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var mockHostLifetime = new Mock<IHostApplicationLifetime>();
            var mockConfigManager = new Mock<IServerConfigurationManager>();
            mockConfigManager.Setup(c => c.Configuration.LibraryScanFanoutConcurrency).Returns(0);
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                mockHostLifetime.Object,
                mockLogger.Object,
                mockConfigManager.Object);

            // Use reflection to call ScheduleTaskCleanup
            var method = typeof(LimitedConcurrencyLibraryScheduler).GetMethod("ScheduleTaskCleanup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            method.Invoke(scheduler, null);

            // Wait a bit for async log to be called
            await Task.Delay(50);

            // Assert
            mockLogger.Verify(l => l.LogDebug(It.Is<string>(s => s.Contains("Schedule cleanup task in"))), Times.Once);
        }
    }
}
