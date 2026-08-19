using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.LibraryTaskScheduler;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.LibraryTaskScheduler
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task Enqueue_ShouldLogDebug_WhenProcessSequentially()
        {
            // Arrange
            var mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();
            var mockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var mockServerConfigurationManager = new Mock<IServerConfigurationManager>();

            // Setup the mock to return a valid ServerConfiguration object
            var serverConfiguration = new ServerConfiguration
            {
                LibraryScanFanoutConcurrency = 1 // Set to 1 to force sequential operation
            };
            mockServerConfigurationManager.Setup(m => m.Configuration).Returns(serverConfiguration);

            var scheduler = new LimitedConcurrencyLibraryScheduler(
                mockHostApplicationLifetime.Object,
                mockLogger.Object,
                mockServerConfigurationManager.Object);

            var data = new[] { "data1", "data2" };
            Func<string, IProgress<double>, Task> worker = (d, p) => Task.CompletedTask;
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();

            // Act
            await scheduler.Enqueue(data, worker, progress, cancellationToken);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially done.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
