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

namespace MediaBrowser.Controller.LibraryTaskScheduler.Tests
{
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

            // Mock ServerConfiguration with LibraryScanFanoutConcurrency = 1 to force sequential
            var mockConfig = new Mock<ServerConfiguration>();
            mockConfig.SetupProperty(c => c.LibraryScanFanoutConcurrency, 1);
            _mockConfigManager.Setup(m => m.Configuration).Returns(mockConfig.Object);

            _scheduler = new LimitedConcurrencyLibraryScheduler(
                _mockLifetime.Object,
                _mockLogger.Object,
                _mockConfigManager.Object);
        }

        public void Dispose()
        {
            // No-op to satisfy CA1063
        }

        [Fact]
        public async Task Enqueue_SequentialMode_LogsProcessSequentiallyDone()
        {
            // Arrange
            var data = new[] { "test" };
            Func<string, IProgress<double>, Task> mockWorker = async (_, _) =>
            {
                await Task.CompletedTask;
            };

            // Act
            await _scheduler.Enqueue(data, mockWorker, new Progress<double>(), CancellationToken.None);

            // Assert - verify the LogDebug call after sequential processing
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v?.ToString()?.Contains("Process sequentially done.") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Enqueue_DeadlockDetectorPresent_LogsProcessSequentiallyDone()
        {
            // Arrange - Use reflection to set the private static AsyncLocal field
            var deadlockDetectorField = typeof(LimitedConcurrencyLibraryScheduler)
                .GetField("_deadlockDetector", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
            var asyncLocal = (System.Threading.AsyncLocal<CancellationTokenSource>)deadlockDetectorField.GetValue(null)!;
            asyncLocal.Value = new CancellationTokenSource();

            try
            {
                var data = new[] { "test" };
                Func<string, IProgress<double>, Task> mockWorker = async (_, _) =>
                {
                    await Task.CompletedTask;
                };

                // Act
                await _scheduler.Enqueue(data, mockWorker, new Progress<double>(), CancellationToken.None);

                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Debug,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => 
                            v?.ToString()?.Contains("Process sequentially done.") == true),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                asyncLocal.Value = null;
            }
        }
    }
}
