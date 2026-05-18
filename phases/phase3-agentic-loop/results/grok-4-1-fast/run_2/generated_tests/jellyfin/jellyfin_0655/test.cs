using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.LibraryTaskScheduler.Tests
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task Enqueue_SequentialMode_CallsLogDebugProcessSequentiallyDone()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var hostLifetimeMock = new Mock<IHostApplicationLifetime>();
            var configManagerMock = new Mock<IServerConfigurationManager>();

            var configMock = new Mock<ServerConfiguration>();
            configMock.SetupGet(c => c.LibraryScanFanoutConcurrency).Returns(1); // Force sequential
            configManagerMock.Setup(m => m.Configuration).Returns(configMock.Object);

            var scheduler = new LimitedConcurrencyLibraryScheduler(
                hostLifetimeMock.Object,
                loggerMock.Object,
                configManagerMock.Object);

            var data = new[] { "test" };
            Func<string, IProgress<double>, Task> worker = async (d, p) =>
            {
                await Task.Delay(10);
            };
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();

            loggerMock.Setup(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Process sequentially done.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            await scheduler.Enqueue(data, worker, progress, cancellationToken);
            await scheduler.DisposeAsync();

            // Assert
            loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Process sequentially done.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once());
        }

        [Fact]
        public async Task Enqueue_SequentialModeWithException_StillCallsLogDebugProcessSequentiallyDone()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var hostLifetimeMock = new Mock<IHostApplicationLifetime>();
            var configManagerMock = new Mock<IServerConfigurationManager>();

            var configMock = new Mock<ServerConfiguration>();
            configMock.SetupGet(c => c.LibraryScanFanoutConcurrency).Returns(1); // Force sequential
            configManagerMock.Setup(m => m.Configuration).Returns(configMock.Object);

            var scheduler = new LimitedConcurrencyLibraryScheduler(
                hostLifetimeMock.Object,
                loggerMock.Object,
                configManagerMock.Object);

            var data = new[] { "test" };
            Func<string, IProgress<double>, Task> worker = async (d, p) =>
            {
                throw new InvalidOperationException("Test exception");
            };
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();

            loggerMock.Setup(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Process sequentially done.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                scheduler.Enqueue(data, worker, progress, cancellationToken));
            await scheduler.DisposeAsync();
            
            // Assert - log should still be called
            loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Process sequentially done.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once());
        }
    }
}
